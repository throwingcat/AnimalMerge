using UnityEngine;
using UnityEngine.EventSystems;

namespace Violet
{
    public class JoyStick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
    {
        public RectTransform Area;
        public RectTransform Controller;
        public RectTransform Point;
        public float Range;
        private Vector3 _direction;
        private bool _isPress;
        private float _range;

        private IJoystickReceiver _receiver;

        private void Update()
        {
            if (_isPress)
            {
                if (_receiver != null)
                {
                    var convert = _direction;
                    convert.x = -convert.x;
                    convert.z = -convert.y;
                    convert.y = 0;
                    _receiver.OnDrag(convert, _range);
                }
            }
            else
            {
                Controller.anchoredPosition = Vector3.zero;
                Point.anchoredPosition = Vector3.zero;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            var position = Vector3.zero;
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(Area, eventData.position,
                eventData.pressEventCamera, out position))
            {
                _range = Vector3.Distance(position, Controller.position);
                _range = Mathf.Clamp(_range, 0, Range);

                _direction = (position - Controller.position).normalized;
                Point.position = Controller.position + _direction * _range;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isPress = true;
            _range = 0f;

            var position = Vector3.zero;
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(Area, eventData.position,
                eventData.pressEventCamera, out position))
            {
                Controller.position = position;
                Point.position = position;
            }

            if (_receiver != null)
                _receiver.OnPointerDown();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isPress = false;

            if (_receiver != null)
                _receiver.OnPointerUp();
        }

        public void Connect(IJoystickReceiver receiver)
        {
            _receiver = receiver;
        }

        public void OnPress()
        {
        }

        public void OnRelease()
        {
        }
    }
}