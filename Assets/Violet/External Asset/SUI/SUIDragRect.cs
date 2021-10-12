using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Violet
{
    public class SUIDragRect : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        public UnityEvent onBeginDrag, onEndDrag;
        public UnityEventVector2 onDrag, onClick;

#pragma warning disable 0414
        private bool _isDrag;
#pragma warning restore 0414

        public void OnBeginDrag(PointerEventData eventData)
        {
            _isDrag = true;

            if (onBeginDrag != null)
                onBeginDrag.Invoke();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (onDrag != null)
                onDrag.Invoke(eventData.delta);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _isDrag = false;

            if (onEndDrag != null)
                onEndDrag.Invoke();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (onClick != null)
                onClick.Invoke(eventData.position);
        }
    }

    [Serializable]
    public class UnityEventVector2 : UnityEvent<Vector2>
    {
    }
}