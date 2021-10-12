using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Violet.Audio;

namespace Violet
{
    public class SceneDirector : MonoSingleton<SceneDirector>
    {
        public const int SCREEN_WIDTH = 1080;
        public const int SCREEN_HEIGHT = 1920;
        public static Action OnSceneLoadedCallback;

        public static Vector2 canvasSize;

        // 안전영역 설정.
        public static float tempTestSafeArea;

        // 모듈.
        public UIManager uiManager;
        public AudioManager audioManager;
        public RectTransform rectTrnStaticCanvas;

        private Coroutine _changeSceneCoroutine;

        private string _nextSceneName;

        public Action ChangeBeforeCallback;
        public Action<eSCENE_TYPE> ChangeCompleteCallback;
        public static bool IsInit { get; private set; }
        public static eSCENE_TYPE PrevSceneType { get; set; }
        public static eSCENE_TYPE CurrentSceneType { get; set; }

        public static eLANGUAGE_TYPE LanguageType { get; set; }

        public static int TargetWidth { get; private set; }
        public static int TargetHeight { get; private set; }

        public static int OriginalWidth { get; private set; }
        public static int OriginalHeight { get; private set; }

        private void OnApplicationPause(bool pauseStatus)
        {
        }

        public static void OnApplicationInitialize()
        {
            SetScreenSize();
        }

        // 화면비 설정.
        private static void SetScreenSize()
        {
            OriginalWidth = Screen.width;
            OriginalHeight = Screen.height;
            Application.targetFrameRate = 60;

            float targetRatio = SCREEN_HEIGHT / SCREEN_WIDTH;
            var curRatio = Screen.height / (float) Screen.width;
            TargetWidth = SCREEN_WIDTH;
            TargetHeight = SCREEN_HEIGHT;

            if (targetRatio != curRatio)
            {
                if (targetRatio > curRatio)
                    TargetWidth = (int) (SCREEN_HEIGHT / curRatio);
                else
                    TargetHeight = (int) (SCREEN_WIDTH * curRatio);
            }

            Screen.SetResolution(TargetWidth, TargetHeight, true);

            Debug.LogFormat("{0} / {1} / {2}", OriginalWidth, Screen.width, Screen.safeArea);
        }

        public static void SetSafeArea(RectTransform rectTrn)
        {
            if (rectTrn == null)
                return;

            var safeArea = Screen.safeArea;

#if (UNITY_EDITOR)
            // 노치 테스트용 임시값.
            if (tempTestSafeArea > 0)
            {
                safeArea.x = tempTestSafeArea;
                safeArea.width = OriginalWidth - tempTestSafeArea * 2;
            }

            rectTrn.anchorMin = new Vector2(Mathf.Clamp01(safeArea.x / Screen.width),
                Mathf.Clamp01(safeArea.y / Screen.height));
            rectTrn.anchorMax = new Vector2(Mathf.Clamp01((safeArea.x + safeArea.width) / Screen.width),
                Mathf.Clamp01((safeArea.y + safeArea.height) / Screen.height));
#elif (UNITY_ANDROID)
			rectTrn.anchorMin =
 new Vector2(Mathf.Clamp01(safeArea.x / Screen.width), Mathf.Clamp01(safeArea.y / Screen.height));
			rectTrn.anchorMax =
 new Vector2(Mathf.Clamp01((safeArea.x + safeArea.width) / Screen.width), Mathf.Clamp01((safeArea.y + safeArea.height) / Screen.height));
#elif (UNITY_IPHONE || UNITY_IOS)
			rectTrn.anchorMin =
 new Vector2(Mathf.Clamp01(safeArea.x / OriginalWidth), Mathf.Clamp01(safeArea.y / OriginalHeight));
			rectTrn.anchorMax =
 new Vector2(Mathf.Clamp01((safeArea.x + safeArea.width) / OriginalWidth), Mathf.Clamp01((safeArea.y + safeArea.height) / OriginalHeight));
#endif
        }

        // 씬 로드 완료시.
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            GC.Collect();
            SUIPanel.ResetStack();

            if (OnSceneLoadedCallback != null)
                OnSceneLoadedCallback();
        }

        public T CreateManager<T>() where T : MonoSingleton<T>
        {
            T mgr = null;
            var obj = new GameObject(typeof(T).ToString());
            mgr = obj.AddComponent<T>();
            return mgr;
        }
    }
}