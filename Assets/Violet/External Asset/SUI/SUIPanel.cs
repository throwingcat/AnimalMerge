using System;
using System.Collections;
using System.Collections.Generic;
using Coffee.UIExtensions;
using DG.Tweening;
using UnityEngine;
using Violet.Audio;

namespace Violet
{
    // 화면관리.
    public class SUIPanel : MonoBehaviour
    {
        public const string AUDIO_CLIP_POPUP_OPEN = "Sound/SFX_UI/popup_open";
        public const string AUDIO_CLIP_POPUP_CLOSE = "Sound/SFX_UI/popup_close";

        public RectTransform safeRect;

        [SerializeField] protected DOTweenAnimation[] _tweens;

        public bool WithCurtain;
        private UIEffectCapturedImage _captureBlur;
#pragma warning disable 0414
        private bool _isRunningHideProcess;
#pragma warning restore 0414
        public virtual ePANEL_TYPE PanelType => ePANEL_TYPE.None;

        public RectTransform RectTrn { get; private set; }
        public bool IsShow { get; private set; }
        public bool IsPopup { get; private set; }

        public SUIPanel PrevPanel
        {
            get
            {
                if (_PanelStack[_LastIndex] == this)
                    return _PanelStack[_LastIndex - 1];
                return _PanelStack[_LastIndex];
            }
        }

        public bool IsInitPanel { get; private set; }

        // 초기화.
        private void InitPanel()
        {
            if (IsInitPanel)
                return;
            IsInitPanel = true;

            RectTrn = GetComponent<RectTransform>();
            _captureBlur = GetComponentInChildren<UIEffectCapturedImage>();
            if (_captureBlur != null)
                _captureBlur.captureOnEnable = false;
            SceneDirector.SetSafeArea(safeRect);
        }

        // 표시.
        public void Show()
        {
            InitPanel();

            // 이미 표시중이면 무시.
            if (IsShow)
                return;
            IsShow = true;

            // 화면 스택에 추가.
            _PanelStack.Add(this);

            // 최초 로비화면이면 무시.
            if (_PanelStack.Count <= 1)
            {
                gameObject.SetActive(true);
                OnShow();
                return;
            }

            var prev = PrevPanel;
            if (IsPopup == false)
            {
                prev.IsShow = false;
                prev.gameObject.SetActive(false);
            }

            gameObject.SetActive(true);
            OnShow();

            if (_captureBlur != null)
                _captureBlur.Capture();
        }

        // 숨김.
        public void Hide()
        {
            // 이미 숨겼으면 무시.
            if (!IsShow)
                return;
            IsShow = false;

            // 마지막 화면인 경우.
            if (_PanelStack.Count <= 1)
            {
                gameObject.SetActive(false);
                OnHide();
                _PanelStack.RemoveAt(_LastIndex);

                return;
            }

            var prev = PrevPanel;

            // 화면적인 처리.
            prev.IsShow = true;
            prev.gameObject.SetActive(true);
            gameObject.SetActive(false);

            // 논리적인 처리.
            HidedPanel = this;
            OnHide();
            _PanelStack.RemoveAt(_LastIndex);
            prev.OnReShow();

            if (IsPopup)
                AudioManager.Instance.Play(AUDIO_CLIP_POPUP_CLOSE);

            // 대화상자 커튼변경.
            if (prev.IsPopup)
            {
                if (prev.WithCurtain)
                    UIManager.Instance.ShowCurtain(prev.transform);
                else
                    UIManager.Instance.HideCurtain();
            }
            else
            {
                UIManager.Instance.HideCurtain();
            }
        }

        // 즉시숨김.
        public void HideDirect()
        {
            IsShow = false;
            gameObject.SetActive(false);
            OnHideDirect();
        }

        // 이전화면을 안닫고 현재화면을 표시.
        public void ShowPopup(bool withCurtain = true)
        {
            transform.SetAsLastSibling();

            WithCurtain = withCurtain;

            if (WithCurtain)
                UIManager.Instance.ShowCurtain(transform);

            //AudioManager.Instance.Play(AUDIO_CLIP_POPUP_OPEN, eAUDIO_TYPE.SFX);

            IsPopup = true;
            Show();
        }

        // 스택 맨위로 추가된 직후에 표시될 경우.
        protected virtual void OnShow()
        {
            ToggleTween(true);
        }

        // 숨김상태로 변경될 경우.
        protected virtual void OnHide()
        {
        }

        // 즉시숨겨질 경우.
        protected virtual void OnHideDirect()
        {
        }

        // 숨김상태에서 다시 표시될 경우.
        protected virtual void OnReShow()
        {
        }

        // 뒤로버튼 눌렀을때.
        public virtual void BackPress()
        {
            if (IgnoreBackPress == false)
            {
                if (_tweens.Length == 0)
                    Hide();
                else
                    StartCoroutine(HideProcess());
            }
        }

        public virtual IEnumerator HideProcess()
        {
            _isRunningHideProcess = true;

            ToggleTween(false);

            Hide();

            yield return new WaitForSeconds(0.5f);

            _isRunningHideProcess = false;
        }

        // 패널 얻기.
        public static SUIPanel GetPanel(int index)
        {
            if (_PanelStack.Count <= index)
                return null;
            return _PanelStack[index];
        }

        // 트윈 연출.
        public virtual void ToggleTween(bool isShow, List<string> ignore = null)
        {
            for (var i = 0; i < _tweens.Length; i++)
            {
                if (ignore != null)
                    if (ignore.Contains(_tweens[i].name))
                        continue;

                if (isShow)
                {
                    _tweens[i].DORewind();
                    _tweens[i].DOPlayForward();
                }
                else
                {
                    _tweens[i].DOComplete();
                    _tweens[i].DOPlayBackwards();
                }
            }
        }

        #region static

        public static int StackCount => _PanelStack.Count;

        public static SUIPanel CurrentPanel
        {
            get
            {
                if (_LastIndex < 0)
                    return null;
                return _PanelStack[_LastIndex];
            }
        }

        public static bool IgnoreBackPress
        {
            get => _ignoreBackPress;

            set => _ignoreBackPress = value;
        }

        public static bool _ignoreBackPress;
        public static SUIPanel HidedPanel { get; private set; }

        private static readonly List<SUIPanel> _PanelStack = new List<SUIPanel>();

        private static int _LastIndex => _PanelStack.Count - 1;

        // 뒤로버튼 강제발생.
        public static void BackPressForce()
        {
            if (_LastIndex >= 0)
                _PanelStack[_LastIndex].BackPress();
        }

        // 모든스택 초기화.
        public static void ResetStack()
        {
            _PanelStack.Clear();
        }

        // 최초 화면으로 이동.
        public static void ShowTopPanel()
        {
            if (_PanelStack.Count <= 0)
                return;

            for (var i = 0; i < _PanelStack.Count; i++)
                _PanelStack[i].HideDirect();

            var topPanel = _PanelStack[0];
            ResetStack();
            topPanel.Show();
        }

        public static void BackToTheTop()
        {
            if (_PanelStack.Count <= 0)
                return;

            for (var i = 0; i < _PanelStack.Count; i++)
            {
                _PanelStack[i].IsShow = false;
                _PanelStack[i].gameObject.SetActive(false);
            }

            var topPanel = _PanelStack[0];

            topPanel.gameObject.SetActive(true);
            topPanel.IsShow = true;
            topPanel.OnReShow();
            ResetStack();
            _PanelStack.Add(topPanel);
        }

        // 스택만 쌓기.
        public static void AddStack(SUIPanel panel)
        {
            _PanelStack.Add(panel);
        }

        //전역용 트윈 유틸
        public static void ToggleTween(DOTweenAnimation[] tweens, bool isShow, bool isSmooth = false)
        {
            for (var i = 0; i < tweens.Length; i++)
            {
                if (tweens == null) continue;
                if (isShow)
                {
                    if (isSmooth == false)
                        tweens[i].DORewind();
                    tweens[i].DOPlayForward();
                }
                else
                {
                    if (isSmooth == false)
                        tweens[i].DOComplete();
                    tweens[i].DOPlayBackwards();
                }
            }
        }

        public static void ToggleTween(List<DOTweenAnimation> tweens, bool isShow, bool isSmooth = false)
        {
            for (var i = 0; i < tweens.Count; i++)
            {
                if (tweens == null) continue;
                if (isShow)
                {
                    if (isSmooth == false)
                        tweens[i].DORewind();
                    tweens[i].DOPlayForward();
                }
                else
                {
                    if (isSmooth == false)
                        tweens[i].DOComplete();
                    tweens[i].DOPlayBackwards();
                }
            }
        }

        public void SetActiveAllPanel(bool isActive)
        {
            foreach (var panel in _PanelStack)
                panel.gameObject.SetActive(isActive);
        }

        #endregion

        #region Editor Only

#if UNITY_EDITOR

        private static bool _isInitEditor;
        private static SUIPanelInfo _panelInfo;

        private void Awake()
        {
            if (_isInitEditor)
                return;
            _isInitEditor = true;

            var go = new GameObject("[SUIPanelInfo]");
            _panelInfo = go.AddComponent<SUIPanelInfo>();
            if (_panelInfo == null)
                VioletLogger.LogErrorFormat("{0} is null", "_panelInfo");
            DontDestroyOnLoad(go);
        }

        [ContextMenu("Set Tween")]
        private void SetTweens()
        {
            _tweens = transform.GetComponentsInChildren<DOTweenAnimation>(true);
        }
#endif

        #endregion
    }

    public class SUIPanelAttribute : Attribute
    {
        public bool isPopup = false;
        public Type PanelType;
    }
}