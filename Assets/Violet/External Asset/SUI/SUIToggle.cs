using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Violet
{
    public class SUIToggle : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        public eTOGGLE_ACTIVE_TYPE activeType = eTOGGLE_ACTIVE_TYPE.None;
        public ePRESS_TYPE pressType = ePRESS_TYPE.None;

        public Text text;

        [Range(0.5f, 1.5f)] public float scaleTarget = 0.95f;

        public RectTransform rectTarget;
        public Image imgTarget;
        public Sprite sprTarget;
        public Color colorTarget = Color.white;

        public GameObject goDeactive, goActive;
        public Image imgActive;
        public Sprite sprActive;
        public Color colorActive = Color.white;

        // 버튼동작여부.
        public bool enableButton = true;

        // 이벤트.
        [SerializeField] public SUIToggleEvent onValueChange;

        private bool _isDown;
        private Color _oriColor;
        private Color _oriColorActive;
        private Sprite _oriSprite;

        private Sprite _oriSpriteActive;
        private RectTransform _rectTrn;

        private readonly float _scalePressSpeed = 0.1f;
        private readonly float _scaleReleaseSpeed = 0.06f;
        private Tweener _tweener;

        // 체크여부.
        public bool IsCheck { get; private set; }

        // 클릭시.
        public void OnPointerClick(PointerEventData eventData)
        {
            if (!enableButton)
                return;

            if (onValueChange != null)
                onValueChange.Invoke(this);
        }

        // 눌렀을때.
        public void OnPointerDown(PointerEventData eventData)
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
                    _tweener = rectTarget.DOScale(scaleTarget, _scalePressSpeed);
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
                    rectTarget.localScale = new Vector3(scaleTarget, scaleTarget, 1.0f);
                    _tweener = rectTarget.DOScale(Vector3.one, _scaleReleaseSpeed).SetEase(Ease.OutQuart);
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

            if (imgActive != null)
            {
                _oriColorActive = imgActive.color;
                _oriSpriteActive = imgActive.sprite;
            }
        }

        // 토글 설정.
        public void SetToggle(bool enable)
        {
            Init();

            IsCheck = enable;

            switch (activeType)
            {
                case eTOGGLE_ACTIVE_TYPE.Color:
                    imgActive.color = IsCheck ? colorActive : _oriColorActive;
                    break;
                case eTOGGLE_ACTIVE_TYPE.Sprite:
                    imgActive.sprite = IsCheck ? sprActive : _oriSpriteActive;
                    break;
                case eTOGGLE_ACTIVE_TYPE.Check:
                    goActive.SetActive(IsCheck);
                    break;
                case eTOGGLE_ACTIVE_TYPE.ToggleGameObject:
                    goDeactive.SetActive(!IsCheck);
                    goActive.SetActive(IsCheck);
                    break;
            }
        }
    }

    [Serializable]
    public class SUIToggleEvent : UnityEvent<SUIToggle>
    {
    }

    public enum eTOGGLE_ACTIVE_TYPE
    {
        None,

        // 색깔 변경.
        Color,

        // Sprite 변경.
        Sprite,

        // 단일 오브젝트 꺼킴.
        Check,

        // 2개의 오브젝트 꺼킴.
        ToggleGameObject
    }
}