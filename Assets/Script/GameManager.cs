using System;
using System.Collections;
using System.Collections.Generic;
using BackEnd;
using Define;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using Violet;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public CSVDownloadConfig CSVDownloadConfig;

    public static eLanguage CurrentLangauge = eLanguage.Korean;

    public static Vector2 Resolution = new Vector2(1280, 720);

    public PartBase PartSceneChange;

    public eGAME_STATE CurrentGameState = eGAME_STATE.Intro;

    private string _loadingDescription = "";
    private float _loadingProgress = 0f;

    public bool isSinglePlay = true;
    public string GUID = "";

    public void Awake()
    {
        Instance = this;
        SceneDirector.OnApplicationInitialize();
        StartCoroutine(InitalizeProcess());
    }

    public void Update()
    {
        var delta = Time.deltaTime;
        switch (CurrentGameState)
        {
            case eGAME_STATE.Intro:
                break;
            case eGAME_STATE.Lobby:
                break;
            case eGAME_STATE.Battle:
                GameCore.Instance.OnUpdate(delta);
                break;
        }

        //지연 이벤트 호출 업데이트
        for (int i = 0; i < _delayInvokeList.Count; i++)
        {
            _delayInvokeList[i].delta += delta;
            if (_delayInvokeList[i].t <= _delayInvokeList[i].delta)
            {
                _delayInvokeList[i].action?.Invoke();
                _delayInvokeList.RemoveAt(i);
                i--;
            }
        }
        
        //심플 타이머 업데이트
        for (int i = 0; i < _simpleTimerList.Count; i++)
        {
            _simpleTimerList[i].delta += delta;
            if (_simpleTimerList[i].t <= _simpleTimerList[i].delta)
            {
                _simpleTimerList.RemoveAt(i);
                i--;
            }
        }
    }

    private void OnGUI()
    {
        // if (GUILayout.Button("Create BabyDuck", GUILayout.Width(Screen.width * 0.15f),
        //     GUILayout.Height(Screen.height * 0.075f)) == true)
        // {
        //     PlayerData.CreateNewDuck();
        // }
        //
        // if (GUILayout.Button("On Fever Mode", GUILayout.Width(Screen.width * 0.15f),
        //     GUILayout.Height(Screen.height * 0.075f)) == true)
        // {
        //     Player.ChargeVolt(999999f);
        // }
    }

    private IEnumerator InitalizeProcess()
    {
        PartSceneChange.OnShow();

        ChangeGameState(eGAME_STATE.Intro);

        //기본 데이터 다운로드
        yield return StartCoroutine(LoadSheetData());

        //서버 접속
        yield return StartCoroutine(ConnectionServer());

        //게임 시작
        yield return StartCoroutine(InitializeGame());

        PartSceneChange.OnHide();
    }

    private IEnumerator LoadSheetData()
    {
        var isDone = false;

        CSVDownloadConfig.Download(this, delegate { }, () =>
        {
            TableManager.Instance.Load();
            isDone = true;
        });
        while (isDone == false)
            yield return null;
    }

    private IEnumerator ConnectionServer()
    {
        //뒤끝 SDK 초기화
        var bro = Backend.Initialize(true);
        if (bro.IsSuccess())
        {
        }
        else
        {
            Application.Quit();
        }

        yield break;
    }

    private IEnumerator InitializeGame()
    {
        bool isDone = false;
        //로그인
        bool result = NetworkManager.Instance.Login();
        while (result == false)
        {
            Debug.Log("로그인 재시도");
            yield return new WaitForSeconds(1f);
            result = NetworkManager.Instance.Login();
        }

        string indate = Backend.UserInDate;
        string nickname = Backend.UserNickName;

        //닉네임 생성
        if (nickname.IsNullOrEmpty())
        {
            var popup = UIManager.Instance.ShowPopup<PopupLogin>();
            popup.OnFinish = (text) =>
            {
                bool result = NetworkManager.Instance.CreateNickname(text);
                if (result)
                {
                    SUIPanel.BackPressForce();
                    isDone = true;
                }
            };
        }
        else
            isDone = true;

        while (isDone == false)
            yield return null;

        if (isSinglePlay)
            ChangeGameState(eGAME_STATE.Battle);
        else
            ChangeGameState(eGAME_STATE.Lobby);
    }

    public void ChangeGameState(eGAME_STATE state)
    {
        //Leave
        switch (CurrentGameState)
        {
            case eGAME_STATE.Intro:
                break;
            case eGAME_STATE.Lobby:
                StartCoroutine(OnLeaveLobby());
                break;
            case eGAME_STATE.Battle:
                StartCoroutine(OnLeaveBattle());
                break;
        }

        CurrentGameState = state;

        UIManager.Instance.HideCurtain();

        //Enter
        switch (CurrentGameState)
        {
            case eGAME_STATE.Lobby:
                StartCoroutine(OnEnterLobby());
                break;
            case eGAME_STATE.Battle:
                StartCoroutine(OnEnterBattle());
                break;
        }
    }

    private IEnumerator OnEnterLobby()
    {
        UIManager.Instance.Show<PanelLobby>();
        yield break;
    }

    private IEnumerator OnLeaveLobby()
    {
        yield break;
    }

    private IEnumerator OnEnterBattle()
    {
        GameCore.Instance.Initialize();
        yield break;
    }

    private IEnumerator OnLeaveBattle()
    {
        //게임 오브젝트 삭제
        GameCore.Instance.Clear();
        
        //네트워크 종료
        NetworkManager.Instance.ClearEvent();
        NetworkManager.Instance.DisconnectIngameServer();
        NetworkManager.Instance.DisconnectGameRoom();
        NetworkManager.Instance.DisconnectMatchServer();
        
        yield break;
    }

    #region Utility

    public void LoadVFX(string key, int capacity, GameObject root = null)
    {
        var pool = MonoBehaviourPool<FXController>.CreatePool(key, delegate
        {
            var prefab = ResourceManager.Instance.LoadPrefab(key);
            if (prefab == null)
            {
                Debug.LogError(string.Format("{0} is not Found Prefab", key));
                return null;
            }

            var vfx = Instantiate(prefab).GetComponent<FXController>();
            if (vfx == null)
            {
                Debug.LogError(string.Format("{0} is not attach FXController", key));
                return null;
            }

            vfx.gameObject.SetActive(false);
            vfx.OnRestore =
                v => { MonoBehaviourPool<FXController>.GetPool(key).Restore(v); };
            return vfx;
        }, capacity, root);
    }

    public static DateTime GetTime()
    {
        return DateTime.UtcNow;
    }

    private static List<DelayInvokeData> _delayInvokeList = new List<DelayInvokeData>();

    public static string DelayInvoke(Action action, float t)
    {
        var invoke = new DelayInvokeData()
        {
            GUID = Guid.NewGuid().ToString(),
            action = action,
            t = t,
            delta = 0f,
        };
        _delayInvokeList.Add(invoke);
        
        return invoke.GUID;
    }

    public static void DelayInvokeCancel(string guid)
    {
        for (int i = 0; i < _delayInvokeList.Count; i++)
        {
            if (_delayInvokeList[i].GUID == guid)
            {
                _delayInvokeList.RemoveAt(i);
                break;
            }
        }
    }

    public class DelayInvokeData
    {
        public string GUID = "";
        public Action action = null;
        public float t = 0f;
        public float delta = 0f;
    }

    private static List<SimpleTimerData> _simpleTimerList =new List<SimpleTimerData>();
    public static void SimpleTimer(string key, float t)
    {
        if (ContainsTimer(key))
        {
            for (int i = 0; i < _simpleTimerList.Count; i++)
            {
                if (_simpleTimerList[i].Key == key)
                {
                    _simpleTimerList[i].t = t;
                    _simpleTimerList[i].delta = 0f;
                    break;
                }
            }
        }
        else
        {
             _simpleTimerList.Add(new SimpleTimerData()
             {
                 Key = key,
                 t = t,
                 delta = 0f,
             });
        }
    }

    public static bool ContainsTimer(string key)
    {
        for (int i = 0; i < _simpleTimerList.Count; i++)
        {
            if (_simpleTimerList[i].Key == key)
                return true;
        }

        return false;
    }
    public class SimpleTimerData
    {
        public string Key;
        public float t = 0f;
        public float delta = 0f;
    }
    #endregion
}