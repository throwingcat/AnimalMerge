using UnityEngine;

namespace Violet
{
    public class SUICanvasFitter : MonoBehaviour
    {
        public bool horizontal, vertical;

        private RectTransform _rectTrn;

#if UNITY_EDITOR

        private void Update()
        {
            ChangeSize();
        }

#endif

        private void OnEnable()
        {
            ChangeSize();
        }

        private void ChangeSize()
        {
            if (_rectTrn == null)
                _rectTrn = GetComponent<RectTransform>();

            if (horizontal)
                _rectTrn.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, SceneDirector.TargetWidth);
            if (vertical)
                _rectTrn.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, SceneDirector.TargetHeight);
        }
    }
}