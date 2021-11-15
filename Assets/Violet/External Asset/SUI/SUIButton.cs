using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Violet
{
    public class SUIButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        public ePRESS_TYPE pressType = ePRESS_TYPE.Sprite;
        public eSOUND_TYPE soundType = eSOUND_TYPE.Default;
        public eSOUND_EXCUTE_TYPE soundExcuteType = eSOUND_EXCUTE_TYPE.Press;

        [Range(0.5f, 1.5f)] public float scaleTarget = 0f;

        public Image imgTarget;
        public Sprite sprTarget;
        public Color colorTarget = Color.white;

        // 버튼동작여부.
        public bool enableButton = true;

        // 이벤트.
        public UnityEvent onClick;
        private bool _isDown;
        private Color _oriColor;
        private Sprite _oriSprite;
        protected RectTransform _rectTrn;

        private readonly float _scalePressSpeed = 0.1f;
        private readonly float _scaleReleaseSpeed = 0.06f;
        private Tweener _tweener;

        [ContextMenu("Disable All Child Raycast Target")]
        private void DisableRaycastTarget()
        {
            var arr = GetComponentsInChildren<MaskableGraphic>();
            foreach (var r in arr)
            {
                if (r.gameObject == this.gameObject) continue;
                r.raycastTarget = false;
            }
        }
        // 클릭시.
        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (!enableButton)
                return;

            if (onClick != null)
                onClick.Invoke();
        }

        // 눌렀을때.
        public virtual void OnPointerDown(PointerEventData eventData)
        {
            Init();

            if (!enableButton)
                return;

            _isDown = true;

            switch (pressType)
            {
                case ePRESS_TYPE.Color:
                    if (imgTarget != null)
                        imgTarget.color = colorTarget;
                    break;
                case ePRESS_TYPE.Sprite:
                    if (imgTarget != null)
                        imgTarget.sprite = sprTarget;
                    break;
                case ePRESS_TYPE.Scale:
                    if (_tweener != null && _tweener.IsActive())
                        _tweener.Kill();
                    _tweener = _rectTrn.ButtonPressPlay(scaleTarget);
                    break;
            }

            if (soundExcuteType == eSOUND_EXCUTE_TYPE.Press)
                PlaySound();
        }

        // 뗐을때.
        public virtual void OnPointerUp(PointerEventData eventData)
        {
            if (!enableButton)
                return;

            if (!_isDown)
                return;

            _isDown = false;

            switch (pressType)
            {
                case ePRESS_TYPE.Color:
                    if (imgTarget != null)
                        imgTarget.color = _oriColor;
                    break;
                case ePRESS_TYPE.Sprite:
                    if (imgTarget != null)
                        imgTarget.sprite = _oriSprite;
                    break;
                case ePRESS_TYPE.Scale:
                    if (_tweener != null && _tweener.IsActive())
                        _tweener.Kill();
                    //_rectTrn.localScale = new Vector3(scaleTarget, scaleTarget, 1.0f);
                    _tweener = _rectTrn.ButtonReleasePlay();//_rectTrn.DOScale(Vector3.one, _scaleReleaseSpeed).SetEase(Ease.OutQuart);
                    break;
            }

            if (soundExcuteType == eSOUND_EXCUTE_TYPE.Release)
                PlaySound();
        }

        // 초기화.
        protected void Init()
        {
            if (_rectTrn != null)
                return;

            _rectTrn = GetComponent<RectTransform>();
            if (imgTarget != null)
            {
                _oriColor = imgTarget.color;
                _oriSprite = imgTarget.sprite;
            }
        }

        public void PlaySound()
        {
            switch (soundType)
            {
                case eSOUND_TYPE.Default:
                {
                    //string path = "Sound/SFX_UI/ui_button_click";
                    //AudioManager.Instance.Play(path, eAUDIO_TYPE.SFX);
                }
                    break;
                case eSOUND_TYPE.Mute:
                    break;
            }
        }
    }

    public enum ePRESS_TYPE
    {
        None,
        Color,
        Sprite,
        Scale
    }

    public enum eSOUND_TYPE
    {
        Default,
        Mute
    }

    public enum eSOUND_EXCUTE_TYPE
    {
        Press,
        Release
    }
}