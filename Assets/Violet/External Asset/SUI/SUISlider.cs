using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Violet
{
    public class SUISlider : UIBehaviour
    {
        // 방향.
        [SerializeField] private Slider.Direction mDirection = Slider.Direction.LeftToRight;

        private float mHeight;
        // 체력바 등의 프로그래스바에 대한 처리.
        // TODO : 모션효과로 변경되는 처리.
        // TODO : 채우는방식과 밀리는방식을 지원.

        // 타겟 이미지.
        private RectTransform mTrnRectCached;

        // 값.
        private float mValue = 1;

        private float mWidth;

        private RectTransform mTrnRect
        {
            get
            {
                if (mTrnRectCached == null)
                    mTrnRectCached = GetComponent<RectTransform>();
                return mTrnRectCached;
            }
        }

        public Slider.Direction direction
        {
            get => mDirection;
            set => ChangeDirection(value);
        }

        public float value
        {
            get => mValue;
            set => ChangeValue(value);
        }

        // 사이즈 초기화.
        public void SetSize()
        {
            mWidth = mTrnRect.rect.width;
            mHeight = mTrnRect.rect.height;
        }

        // 방향 변경.
        private void ChangeDirection(Slider.Direction dir)
        {
            if (mDirection == dir)
                return;

            // 먼저 1인 상태로 크기를 키워놓고 피벗을 바꾼다음 원상복귀를 시킨다.
            var prevValue = mValue;
            ChangeValue(1);

            mDirection = dir;

            var prevPivot = mTrnRect.pivot;

            switch (mDirection)
            {
                case Slider.Direction.LeftToRight:
                    mTrnRect.pivot = new Vector2(0, mTrnRect.pivot.y);
                    mTrnRect.localPosition =
                        new Vector3(mTrnRect.localPosition.x + (0 - prevPivot.x) * mTrnRect.rect.width,
                            mTrnRect.localPosition.y, mTrnRect.localPosition.z);
                    //mTrnRect.anchorMin = new Vector2(0, 0.5f);
                    //mTrnRect.anchorMax = new Vector2(0, 0.5f);
                    break;
                case Slider.Direction.RightToLeft:
                    mTrnRect.pivot = new Vector2(1, mTrnRect.pivot.y);
                    mTrnRect.localPosition =
                        new Vector3(mTrnRect.localPosition.x + (1 - prevPivot.x) * mTrnRect.rect.width,
                            mTrnRect.localPosition.y, mTrnRect.localPosition.z);
                    //mTrnRect.anchorMin = new Vector2(1, 0.5f);
                    //mTrnRect.anchorMax = new Vector2(1, 0.5f);
                    break;
                case Slider.Direction.TopToBottom:
                    mTrnRect.pivot = new Vector2(mTrnRect.pivot.x, 1);
                    mTrnRect.localPosition = new Vector3(mTrnRect.localPosition.x,
                        mTrnRect.localPosition.y + (1 - prevPivot.y) * mTrnRect.rect.height, mTrnRect.localPosition.z);
                    //mTrnRect.anchorMin = new Vector2(0.5f, 1);
                    //mTrnRect.anchorMax = new Vector2(0.5f, 1);
                    break;
                case Slider.Direction.BottomToTop:
                    mTrnRect.pivot = new Vector2(mTrnRect.pivot.x, 0);
                    mTrnRect.localPosition = new Vector3(mTrnRect.localPosition.x,
                        mTrnRect.localPosition.y + (0 - prevPivot.y) * mTrnRect.rect.height, mTrnRect.localPosition.z);
                    //mTrnRect.anchorMin = new Vector2(0.5f, 0);
                    //mTrnRect.anchorMax = new Vector2(0.5f, 0);
                    break;
            }

            ChangeValue(prevValue);
        }

        // 값 변경.
        private void ChangeValue(float val)
        {
            if (mWidth == 0)
                SetSize();

            mValue = val < 0 ? 0 : val > 1 ? 1 : val;
            if (mDirection == Slider.Direction.LeftToRight || mDirection == Slider.Direction.RightToLeft)
                mTrnRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, mValue * mWidth);
            else
                mTrnRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, mValue * mHeight);
        }
    }
}