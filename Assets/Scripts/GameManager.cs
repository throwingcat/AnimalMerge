using System;
using System.Collections;
using System.Collections.Generic;
using BackEnd;
using Define;
using MessagePack;
using MessagePack.Resolvers;
using MessagePack.Unity;
using MessagePack.Unity.Extension;
using Server;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Violet;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public static eLanguage CurrentLangauge = eLanguage.Korean;

    public static Vector2 Resolution = new Vector2(1280, 720);

    private static bool _isSerializerRegisted;

    private string _loadingDescription = "";
    private float _loadingProgress = 0f;

    private PanelTitle _panelTitle;
    public GameCore AICore;
    public CSVDownloadConfig CSVDownloadConfig;

    public eGAME_STATE CurrentGameState = eGAME_STATE.Intro;

    public GameCore GameCore;
    public string GUID = "";
    public bool isAdventure;

    public bool isSinglePlay = true;
    public bool isUnlockHero;
    public string StageKey = "";

    public void Awake()
    {
        Instance = this;
        //SceneDirector.OnApplicationInitialize();
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
                GameCore.OnUpdate(delta);
                if (isSinglePlay)
                    AICore.OnUpdate(delta);
                break;
        }

        //지연 이벤트 호출 업데이트
        for (var i = 0; i < _delayInvokeList.Count; i++)
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
        for (var i = 0; i < _simpleTimerList.Count; i++)
        {
            _simpleTimerList[i].delta += delta;
            if (_simpleTimerList[i].t <= _simpleTimerList[i].delta)
            {
                _simpleTimerList.RemoveAt(i);
                i--;
            }
        }

        AnimalMergeServer.Instance.OnUpdate();
    }

    private IEnumerator InitalizeProcess()
    {
#if UNITY_EDITOR
        Application.runInBackground = true;
#endif
        RegisterSerializer();

        ChangeGameState(eGAME_STATE.Intro);

        _panelTitle = UIManager.Instance.Show<PanelTitle>();
        //기본 데이터 다운로드
        yield return StartCoroutine(LoadSheetData());

        //서버 접속
        yield return StartCoroutine(ConnectionServer());

        //게임 시작
        yield return StartCoroutine(InitializeGame());
    }

    private static void RegisterSerializer()
    {
        if (_isSerializerRegisted) return;
        _isSerializerRegisted = true;

        Debug.Log("RegisterSerializer");

        StaticCompositeResolver.Instance.Register(
            GeneratedResolver.Instance,
            StandardResolver.Instance
        );

        var options = MessagePackSerializerOptions.Standard.WithResolver(StaticCompositeResolver.Instance);
        MessagePackSerializer.DefaultOptions = options;
    }

    private IEnumerator LoadSheetData()
    {
        var isDone = false;

        CSVDownloadConfig.Download(this, v => { _panelTitle.RefreshGauge(v); }, () =>
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
        var isDone = false;
        //로그인
        var result = NetworkManager.Instance.Login();
        while (result == false)
        {
            Debug.Log("로그인 재시도");
            yield return new WaitForSeconds(1f);
            result = NetworkManager.Instance.Login();
        }

        var indate = Backend.UserInDate;
        var nickname = Backend.UserNickName;

        //닉네임 생성
        if (nickname.IsNullOrEmpty())
        {
            var popup = UIManager.Instance.ShowPopup<PopupLogin>();
            popup.OnFinish = text =>
            {
                var result = NetworkManager.Instance.CreateNickname(text);
                if (result)
                {
                    SUIPanel.BackPressForce();
                    isDone = true;
                }
            };
        }
        else
        {
            isDone = true;
        }

        while (isDone == false)
            yield return null;

        if (isSinglePlay)
        {
            ChangeGameState(eGAME_STATE.Battle);
        }
        else
        {
            yield return StartCoroutine(DownloadDB());
            ChangeGameState(eGAME_STATE.Lobby);
        }
    }

    private IEnumerator DownloadDB()
    {
        var isDone = false;

        foreach (var db in AnimalMergeServer.Instance.DBList)
        {
            Debug.Log(string.Format("Download {0}", db.GetType()));
            isDone = false;
            db.Download(() => { isDone = true; });
            while (isDone == false)
                yield return null;
        }

        foreach (var data in PlayerDataManager.Instance.Datas)
        {
            isDone = false;
            data.Download(() => { isDone = true; });
            while (isDone == false)
                yield return null;
        }
    }

    public void ChangeGameState(eGAME_STATE state)
    {
        //Leave
        switch (CurrentGameState)
        {
            case eGAME_STATE.Intro:
                if (_panelTitle != null)
                    _panelTitle.BackPress();
                _panelTitle = null;
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
        while (0 < SUIPanel.StackCount)
            SUIPanel.BackPressForce();
        UIManager.Instance.Show<PanelLobby>();
        yield break;
    }

    private IEnumerator OnLeaveLobby()
    {
        yield break;
    }

    private IEnumerator OnEnterBattle()
    {
        SUIPanel.IgnoreBackPress = true;

        UIManager.Instance.LoadingScreen.SetActive(true);

        //모든 UI 정리
        while (0 < SUIPanel.StackCount)
            SUIPanel.CurrentPanel.Hide();

        GameCore.Initialize(true);
        if (isSinglePlay)
        {
            AICore.Initialize(false);
            GameCore.SyncManager.SetTo(AICore);
            AICore.SyncManager.SetTo(GameCore);
        }

        yield return new WaitForSeconds(2f);

        UIManager.Instance.LoadingScreen.SetActive(false);
    }

    private IEnumerator OnLeaveBattle()
    {
        if (isSinglePlay)
        {
            GameCore.Clear();
            AICore.Clear();
        }
        else
        {
            //게임 오브젝트 삭제
            GameCore.Clear();

            //네트워크 종료
            NetworkManager.Instance.ClearEvent();
            NetworkManager.Instance.DisconnectIngameServer();
            NetworkManager.Instance.DisconnectGameRoom();
            NetworkManager.Instance.DisconnectMatchServer();
        }

        isSinglePlay = false;
        isAdventure = false;

        SUIPanel.IgnoreBackPress = false;

        yield break;
    }

#if UNITY_EDITOR
    [InitializeOnLoadMethod]
    private static void EditorInitialize()
    {
        RegisterSerializer();
    }
#endif


    public static void EnterBattle(bool isSinglePlay)
    {
        Instance.isSinglePlay = isSinglePlay;
        Instance.ChangeGameState(eGAME_STATE.Battle);
    }

    public static void EnterAdventure(string stageKey)
    {
        Instance.isSinglePlay = true;
        Instance.isAdventure = true;
        Instance.StageKey = stageKey;
        Instance.isUnlockHero = false;
        Instance.ChangeGameState(eGAME_STATE.Battle);
    }

    public class BypassCertificate : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            //Simply return true no matter what
            return true;
        }
    }

    public class Guid
    {
        public static ulong Index;

        public static ulong NewGuid()
        {
            return Index++;
        }
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

    private static readonly List<DelayInvokeData> _delayInvokeList = new List<DelayInvokeData>();

    public static ulong DelayInvoke(Action action, float t)
    {
        var invoke = new DelayInvokeData
        {
            GUID = Guid.NewGuid(),
            action = action,
            t = t,
            delta = 0f
        };
        _delayInvokeList.Add(invoke);

        return invoke.GUID;
    }

    public static void DelayInvokeCancel(ulong guid)
    {
        for (var i = 0; i < _delayInvokeList.Count; i++)
            if (_delayInvokeList[i].GUID == guid)
            {
                _delayInvokeList.RemoveAt(i);
                break;
            }
    }

    public class DelayInvokeData
    {
        public Action action;
        public float delta;
        public ulong GUID;
        public float t;
    }

    private static readonly List<SimpleTimerData> _simpleTimerList = new List<SimpleTimerData>();

    public static void SimpleTimer(string key, float t)
    {
        if (ContainsTimer(key))
        {
            for (var i = 0; i < _simpleTimerList.Count; i++)
                if (_simpleTimerList[i].Key == key)
                {
                    _simpleTimerList[i].t = t;
                    _simpleTimerList[i].delta = 0f;
                    break;
                }
        }
        else
        {
            _simpleTimerList.Add(new SimpleTimerData
            {
                Key = key,
                t = t,
                delta = 0f
            });
        }
    }

    public static bool ContainsTimer(string key)
    {
        for (var i = 0; i < _simpleTimerList.Count; i++)
            if (_simpleTimerList[i].Key == key)
                return true;

        return false;
    }

    public class SimpleTimerData
    {
        public float delta;
        public string Key;
        public float t;
    }

    public class TimeSystem
    {
        public DateTime LastUpdatedTime;
        public DateTime CompleteTime { get; private set; }

        public int MaxTime { get; private set; }

        public int RemainSeconds => (int) (RemainMilliSeconds / 1000);

        public float RemainMilliSeconds =>
            Mathf.Clamp((float) (CompleteTime - GetTime()).TotalMilliseconds, 0, float.MaxValue);

        public float Progress => 1f - RemainMilliSeconds / 1000 / MaxTime;
        public bool isComplete => 0 >= RemainMilliSeconds;

        public void ReduceTime(int seconds)
        {
            CompleteTime = CompleteTime.AddSeconds(-seconds);
        }

        public void Set(int seconds)
        {
            CompleteTime = GetTime().AddSeconds(seconds);
            MaxTime = seconds;
            LastUpdatedTime = GetTime();
        }
    }

    #endregion
}