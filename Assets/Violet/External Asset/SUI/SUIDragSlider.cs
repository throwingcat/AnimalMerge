using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Violet
{
    [RequireComponent(typeof(Slider))]
    public class SUIDragSlider : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        // 드래그가 끝났을때 한번만 이벤트를 전달하는것이 목표.

        [SerializeField] public UnityEventFloat onChangeValue;

        private Slider mSlider;

        // 초기화.
        private void Awake()
        {
            mSlider = GetComponent<Slider>();
        }

        // 포인터 다운.
        public void OnPointerDown(PointerEventData eventData)
        {
        }

        // 포인터 업.
        public void OnPointerUp(PointerEventData eventData)
        {
            if (onChangeValue != null)
                onChangeValue.Invoke(mSlider.value);
        }
    }

    [Serializable]
    public class UnityEventFloat : UnityEvent<float>
    {
    }
}