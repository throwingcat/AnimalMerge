using System;
using System.Collections;
using BackEnd;
using Define;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using Violet;

public class GameManager : MonoBehaviour
{    
    public static GameManager Instance;

    public static eLanguage CurrentLangauge = eLanguage.Korean;

    public static Vector2 Resolution = new Vector2(1280, 720);

    public PartBase PartSceneChange;

    public eGAME_STATE CurrentGameState = eGAME_STATE.Intro;

    private string _loadingDescription = "";
    private float _loadingProgress = 0f;

    public void Awake()
    {
        Instance = this;
        SceneDirector.OnApplicationInitialize();
        NavMesh.avoidancePredictionTime = 0.5f;
    }

    public void Start()
    {
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
                GameCore.Instance.OnUpdate(delta);
                break;
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

        GameCore.Instance.CSVDownloadConfig.Download(this, delegate { }, () =>
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
        ChangeGameState(eGAME_STATE.Lobby);
        yield break;
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
        }

        CurrentGameState = state;

        UIManager.Instance.HideCurtain();

        //Enter
        switch (CurrentGameState)
        {
            case eGAME_STATE.Lobby:
                StartCoroutine(OnEnterLobby());
                break;
        }
    }

    private IEnumerator OnEnterLobby()
    {
        GameCore.Instance.Initialize();
        yield break;
    }

    private IEnumerator OnLeaveLobby()
    {
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

    public static void DelayInvoke(Action action, float t)
    {
        Instance.StartCoroutine(Instance.DelayInvokeProcess(action, t));
    }

    private IEnumerator DelayInvokeProcess(Action action, float t)
    {
        yield return new WaitForSeconds(t);
        action?.Invoke();
    }

    #endregion
}