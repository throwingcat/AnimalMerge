using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Violet
{
    [RequireComponent(typeof(Image))]
    public class SUIFlow : MonoBehaviour
    {
        public TextAnchor direction;
        public float duration;
        private TextAnchor _direction;
        private Image _img;
        private Vector2 _imgSize, _imgHalfSize, _initPos, _startPos, _endPos;

        private RectTransform _rectTrn;

        // 초기 설정.
        private void Awake()
        {
            _rectTrn = GetComponent<RectTransform>();
            _img = GetComponent<Image>();

            _initPos = _rectTrn.anchoredPosition;
            _imgSize = new Vector2(_img.sprite.rect.width, _img.sprite.rect.height);
            _imgHalfSize = _imgSize * 0.5f;
            _rectTrn.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _rectTrn.rect.width + _imgSize.x);
            _rectTrn.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _rectTrn.rect.height + _imgSize.y);

            InitValue();
        }

        // 흐르는 처리.
        private void OnEnable()
        {
            _rectTrn.DOKill();

            if (_direction != direction)
                InitValue();

            if (_direction == TextAnchor.MiddleCenter)
                return;

            _rectTrn.anchoredPosition = _startPos;
            _rectTrn.DOAnchorPos(_endPos, duration).SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart);
        }

        // 중지.
        private void OnDisable()
        {
            _rectTrn.DOKill();
        }

        // 값 설정.
        private void InitValue()
        {
            _rectTrn.anchoredPosition = _initPos;
            _direction = direction;
            _startPos = _rectTrn.anchoredPosition;
            var gap = Vector2.zero;

            switch (direction)
            {
                case TextAnchor.UpperLeft:
                    gap = new Vector2(_imgHalfSize.x, -_imgHalfSize.y);
                    break;
                case TextAnchor.UpperCenter:
                    gap = new Vector2(0, -_imgHalfSize.y);
                    break;
                case TextAnchor.UpperRight:
                    gap = new Vector2(-_imgHalfSize.x, -_imgHalfSize.y);
                    break;
                case TextAnchor.MiddleLeft:
                    gap = new Vector2(_imgHalfSize.x, 0);
                    break;
                case TextAnchor.MiddleCenter:
                    gap = new Vector2(0, 0);
                    break;
                case TextAnchor.MiddleRight:
                    gap = new Vector2(-_imgHalfSize.x, 0);
                    break;
                case TextAnchor.LowerLeft:
                    gap = new Vector2(_imgHalfSize.x, _imgHalfSize.y);
                    break;
                case TextAnchor.LowerCenter:
                    gap = new Vector2(0, _imgHalfSize.y);
                    break;
                case TextAnchor.LowerRight:
                    gap = new Vector2(-_imgHalfSize.x, _imgHalfSize.y);
                    break;
            }

            _startPos += gap;
            _endPos = _startPos - gap * 2;
        }
    }
}