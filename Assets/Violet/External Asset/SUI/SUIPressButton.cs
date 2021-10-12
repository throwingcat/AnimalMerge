using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Violet
{
    public class SUIPressButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        public ePRESS_TYPE pressType = ePRESS_TYPE.Sprite;

        [Range(0.5f, 1.5f)] public float scaleTarget = 0.95f;

        public Image imgTarget;
        public Sprite sprTarget;
        public Color colorTarget = Color.white;

        // 버튼동작여부.
        public bool enableButton = true;

        // 이벤트.
        public UnityEvent onPress, onRelease, onClick;
        private bool _isDown;
        private Color _oriColor;
        private Sprite _oriSprite;
        private RectTransform _rectTrn;

        private readonly float _scalePressSpeed = 0.1f;
        private readonly float _scaleReleaseSpeed = 0.06f;
        private Tweener _tweener;

        // 클릭시.
        public void OnPointerClick(PointerEventData eventData)
        {
            if (onClick != null)
                onClick.Invoke();
        }

        // 눌렀을때.
        public void OnPointerDown(PointerEventData eventData)
        {
            Init();

            if (!enableButton)
                return;

            if (onPress != null)
                onPress.Invoke();

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
                    _tweener = _rectTrn.DOScale(scaleTarget, _scalePressSpeed);
                    break;
            }
        }

        // 뗐을때.
        public void OnPointerUp(PointerEventData eventData)
        {
            if (!enableButton)
                return;

            if (!_isDown)
                return;

            if (onRelease != null)
                onRelease.Invoke();

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
                    _rectTrn.localScale = new Vector3(scaleTarget, scaleTarget, 1.0f);
                    _tweener = _rectTrn.DOScale(Vector3.one, _scaleReleaseSpeed).SetEase(Ease.OutQuart);
                    break;
            }
        }

        // 초기화.
        private void Init()
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
    }
}