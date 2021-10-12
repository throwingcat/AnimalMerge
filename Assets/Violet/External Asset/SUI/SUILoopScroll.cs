using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Violet
{
    [RequireComponent(typeof(ScrollRect))]
    public class SUILoopScroll : MonoBehaviour
    {
        private float _origianlContentSize;

        // 드래그 가능여부.
        public bool isEnableDrag { get; private set; }

        // 현재위치.
        public float pos
        {
            get => mIsVertical ? mContentRect.anchoredPosition.y : mContentRect.anchoredPosition.x;
            set => mContentRect.anchoredPosition = mIsVertical ? Vector2.up * value : Vector2.right * value;
        }

        // 초기화.
        public void Init()
        {
            if (mIsInit)
                return;
            mIsInit = true;

            // cellPrefab 검증.
            if (cellPrefab == null)
            {
                Debug.LogError("CellPrefab is null.");
                return;
            }

            // ISUIScrollItem 상속여부 검증.
            if (cellPrefab.GetComponent<IScrollCell>() == null)
            {
                Debug.LogError("CellPrefab is not ISUIScrollItem.");
                return;
            }

            // 변수 초기화 및 리스너 등록.
            mRect = GetComponent<RectTransform>();
            mScrollRect = GetComponent<ScrollRect>();
            mContentRect = mScrollRect.content;
            mScrollRect.onValueChanged.AddListener(OnScrollPosChanged);
            mIsVertical = mScrollRect.vertical;
            mCellSize = mIsVertical ? cellPrefab.sizeDelta.y : cellPrefab.sizeDelta.x;
            mIsGrid = gridCount > 1;

            if (isAutoGridCount)
            {
                var contentsSize = 0f;
                if (mIsVertical)
                {
                    mContentRect.sizeDelta = new Vector2(mScrollRect.GetComponent<RectTransform>().rect.width,
                        mContentRect.sizeDelta.y);
                    contentsSize = mContentRect.rect.width;
                }
                else
                {
                    mContentRect.sizeDelta = new Vector2(mContentRect.sizeDelta.x,
                        mScrollRect.GetComponent<RectTransform>().rect.height);
                    contentsSize = mContentRect.rect.height;
                }

                var count = 0;
                while (0 < contentsSize)
                {
                    contentsSize -= (mIsVertical ? cellPrefab.sizeDelta.x : cellPrefab.sizeDelta.y) +
                                    (mIsVertical ? spacing.x : spacing.y);
                    if (contentsSize < 0) break;
                    count++;
                }

                gridCount = count;
            }

            mCacheSpacing = mIsVertical ? spacing.y : spacing.x;
            mCachePadding = mIsVertical ? padding.top : padding.left;
            mCachePaddingBoth = mIsVertical ? padding.top + padding.bottom : padding.left + padding.right;
            mOriginRectSize = mIsVertical ? mRect.rect.height : mRect.rect.width;

            if (mIsGrid)
            {
                mCacheGridCellOtherSize =
                    mIsVertical ? cellPrefab.sizeDelta.x + spacing.x : cellPrefab.sizeDelta.y + spacing.y;
                mCacheGridFirstCellOtherPos =
                    -(((gridCount - 1) * mCacheGridCellOtherSize - (mIsVertical ? spacing.x : spacing.y)) * 0.5f);
            }

            CreateCells();
        }

        // 데이터 설정.
        public void SetData<T>(List<T> list) where T : IScrollData
        {
            // 초기화.
            Init();

            // List 검증.
            if (list == null)
            {
                Debug.LogError("List is null.");
                return;
            }

            // 데이터를 인터페이스목록으로 변환.
            mDataList.Clear();
            for (var i = 0; i < list.Count; i++)
                mDataList.Add(list[i]);

            // 표시될 Row수 계산.
            var cellRow = mIsGrid ? (mDataList.Count - 1) / gridCount + 1 : mDataList.Count;

            // Content 크기 변경.
            var contentSize = cellRow * mCellSize + mCachePaddingBoth + mCacheSpacing * (cellRow - 1);
            if (contentSize <= 0)
                contentSize = 1;
            mContentRect.SetSizeWithCurrentAnchors(
                mIsVertical ? RectTransform.Axis.Vertical : RectTransform.Axis.Horizontal, contentSize);
            _origianlContentSize = contentSize;
            // 데이터가 더 적다면 일부 셀 숨김.
            for (var i = 0; i < mCellCount; i++)
                mCellRectList[i].gameObject.SetActive(i < list.Count);

            // 데이터가 한화면에 보이고 cancelDragIfFits가 설정되어있으면 드래그금지.
            isEnableDrag = !(cancelDragIfFits && mOriginRectSize >= contentSize);
            mScrollRect.enabled = isEnableDrag;

            if (isCenter)
            {
                var curSize = mRect.sizeDelta;

                if (mIsVertical)
                    curSize.y = mCellSize;
                else
                    curSize.x = mCellSize;

                mRect.sizeDelta = curSize;
            }
        }

        // 정보 갱신.
        public void UpdateAll()
        {
            // 초기화되지 않았으면 무시.
            if (!mIsInit)
            {
                Debug.LogError("Not Init.");
                return;
            }

            // 모든셀이 업데이트되도록 호출.
            UpdateCells(true);
        }

        // 스크롤시 콜백.
        private void OnScrollPosChanged(Vector2 pos)
        {
            UpdateCells(true);
        }

        // 셀 생성.
        private void CreateCells()
        {
            // 생성할 셀개수 계산.
            mCellCount = ((int) (mOriginRectSize / mCellSize) + 2) * (mIsGrid ? gridCount : 1);

            // 셀목록에 추가.
            mCellList.Clear();
            mCellRectList.Clear();
            for (var i = 0; i < mCellCount; i++)
            {
                mCellRectList.Add(Instantiate(cellPrefab, mContentRect));
                mCellRectList[i].localScale = Vector3.one;
                mCellRectList[i].anchorMin = mContentRect.anchorMin;
                mCellRectList[i].anchorMax = mContentRect.anchorMax;
                mCellRectList[i].pivot = mContentRect.pivot;
                mCellList.Add(mCellRectList[i].GetComponent<IScrollCell>());
            }
        }

        // 모든셀 업데이트.
        private void UpdateCells(bool forceUpdate)
        {
            // 현재 스크롤위치 관련정보 얻기.
            var gap = 0.0f;
            if (isCenter)
                gap = (mOriginRectSize - mCellSize) * 0.5f;

            var currentContentPos = (mIsVertical ? mContentRect.anchoredPosition.y : mContentRect.anchoredPosition.x) +
                                    gap + (mIsVertical ? -mCachePadding : mCachePadding);
            if (!mIsVertical)
                currentContentPos = -currentContentPos;
            var topCellDataIndex = (int) (currentContentPos / (mCellSize + mCacheSpacing)) * (mIsGrid ? gridCount : 1);
            var topCellIndex = topCellDataIndex % mCellCount;

            // 최상단이 그대로면 업데이트 안함.
            if (mPrevTopData == topCellDataIndex && !forceUpdate)
                return;

            // 모든 셀 업데이트.
            for (var i = 0; i < mCellCount; i++)
                UpdateCell(i, topCellDataIndex, topCellIndex, forceUpdate);

            // 직전 최상단값 갱신.
            mPrevTopData = topCellDataIndex;
        }

        // 셀 업데이트.
        private void UpdateCell(int cellIndex, int topDataIndex, int topCellIndex, bool forceUpdate)
        {
            // 해당 셀이 표시할 데이터인덱스 계산.
            var targetDataIndex = cellIndex - topCellIndex;
            if (targetDataIndex < 0)
                targetDataIndex = targetDataIndex + mCellCount;
            targetDataIndex += topDataIndex;

            // 범위밖 셀은 위치만 갱신.
            if (targetDataIndex >= mDataList.Count)
            {
                if (!mIsVertical)
                    return;
            
                var tempDataIndex = targetDataIndex - mCellCount;
                var tempPos = (mIsGrid ? tempDataIndex / gridCount : tempDataIndex) * (mCellSize + mCacheSpacing) +
                              mCachePadding;
                var tempOtherPos = mIsGrid
                    ? mCacheGridFirstCellOtherPos + tempDataIndex % gridCount * mCacheGridCellOtherSize
                    : 0;
                var tempVec = new Vector2(mIsVertical ? tempOtherPos : tempPos, mIsVertical ? -tempPos : -tempOtherPos);
                mCellRectList[cellIndex].anchoredPosition = tempVec;
                return;
            }

            // 셀이 표시될 위치 계산.
            var targetPos = (mIsGrid ? targetDataIndex / gridCount : targetDataIndex) * (mCellSize + mCacheSpacing) +
                            mCachePadding;
            float targetOtherPos = 0;

            // isGrid면 반대방향의 위치도 계산.
            if (mIsGrid)
            {
                var otherSpace = (mIsVertical ? spacing.x : spacing.y) * 0.5f;
                targetOtherPos = mCacheGridFirstCellOtherPos + targetDataIndex % gridCount * mCacheGridCellOtherSize -
                                 otherSpace;
            }

            //이미 그 위치에 있으면 무시.
            var curPos = mIsVertical
                ? -mCellRectList[cellIndex].anchoredPosition.y
                : -mCellRectList[cellIndex].anchoredPosition.x;
            if (curPos == targetPos)
            {
                if (mIsGrid)
                {
                    var curOtherPos = mIsVertical
                        ? -mCellRectList[cellIndex].anchoredPosition.x
                        : -mCellRectList[cellIndex].anchoredPosition.y;
                    if (curOtherPos == targetOtherPos)
                    {
                        if (forceUpdate)
                            mCellList[cellIndex].UpdateCell(mDataList[targetDataIndex]);
                        return;
                    }
                }
                else
                {
                    if (forceUpdate)
                        mCellList[cellIndex].UpdateCell(mDataList[targetDataIndex]);
                    return;
                }
            }

            var pos = new Vector2(mIsVertical ? targetOtherPos : targetPos, mIsVertical ? -targetPos : -targetOtherPos);
            mCellRectList[cellIndex].anchoredPosition = pos;

            // 해당셀의 정보갱신.
            mCellList[cellIndex].UpdateCell(mDataList[targetDataIndex]);
        }

        // 목록 위치이동.
        public void Move(int cellDataIndex, bool isAnim = false, float duration = 0.5f)
        {
            mScrollRect.StopMovement();

            if (cellDataIndex < 0)
                cellDataIndex = 0;

            // 표시될 Row위치 계산.
            var targetRow = mIsGrid ? cellDataIndex / gridCount : cellDataIndex;
            var pos = targetRow * (mCellSize + mCacheSpacing);
            var contentSize = mIsVertical ? mContentRect.rect.height : mContentRect.rect.width;
            var scrollRectSize = mIsVertical ? mRect.rect.height : mRect.rect.width;

            if (pos > contentSize - scrollRectSize)
                pos = contentSize - scrollRectSize;
            if (pos < 0)
                pos = 0;

            mContentRect.DOKill();

            if (isAnim)
            {
                if (mIsVertical)
                    mContentRect.DOAnchorPosY(pos, duration).SetEase(Ease.InOutCubic);
                else
                    mContentRect.DOAnchorPosX(-pos, duration).SetEase(Ease.InOutCubic);
            }
            else
            {
                mContentRect.anchoredPosition = mIsVertical ? Vector2.up * pos : Vector2.left * pos;
            }

            UpdateAll();
        }

        public RectTransform GetCellRect(int index)
        {
            if (mCellRectList.Count <= index) return null;
            return mCellRectList[index];
        }

        public void SetContentBoundsPadding(float size)
        {
            if (mContentRect == null) return;
            var axis = mIsVertical ? RectTransform.Axis.Vertical : RectTransform.Axis.Horizontal;

            _origianlContentSize =
                axis == RectTransform.Axis.Vertical ? mContentRect.rect.height : mContentRect.rect.width;
            mContentRect.SetSizeWithCurrentAnchors(axis, _origianlContentSize + size);
        }

        public void ResetContentBoundsPadding()
        {
            if (mContentRect == null) return;

            var axis = mIsVertical ? RectTransform.Axis.Vertical : RectTransform.Axis.Horizontal;
            mContentRect.SetSizeWithCurrentAnchors(axis, _origianlContentSize);
        }

        public void SetScrollRectSize(int cellcount)
        {
            var axis = mIsVertical ? RectTransform.Axis.Vertical : RectTransform.Axis.Horizontal;
            var prevSize = mIsVertical ? mRect.sizeDelta.y : mRect.sizeDelta.x;
            var targetSize = mCachePaddingBoth + mCellSize * cellcount + mCacheSpacing * (cellcount - 1);
            mRect.SetSizeWithCurrentAnchors(axis, prevSize < targetSize ? prevSize : targetSize);
        }

        #region Expose Property

        // 셀 프리팹.
        [SerializeField] private RectTransform cellPrefab;

        // 스크롤범위의 패딩
        [SerializeField] private RectOffset padding;

        // 셀 사이의 간격
        [SerializeField] private Vector2 spacing = Vector2.zero;

        // 범위안에 아이템이 전부 표시될때 드래그 불가여부.
        [SerializeField] private bool cancelDragIfFits;

        // 그리드의 Row당 개수(그리드가 아닌경우 무시됨)
        [SerializeField] private int gridCount;

        [SerializeField] private bool isAutoGridCount;

        // 셀 중앙정렬 여부(사용하는 곳이 없어 미표시)
        [HideInInspector] public bool isCenter;

        public Vector2 CellSize => cellPrefab.rect.size;

        #endregion

        #region Private Property

        // 초기화여부.
        private bool mIsInit;

        // 세로방향 여부.
        private bool mIsVertical;

        // ScrollRect의 RectTransform.
        private RectTransform mRect;

        // ScrollRect.
        private ScrollRect mScrollRect;

        // Content의 RectTransform.
        private RectTransform mContentRect;

        // 방향에 따른 셀사이즈.
        private float mCellSize;

        // 그리드형태인지 여부.
        private bool mIsGrid;

        // 생성한 셀의 개수.
        private int mCellCount;

        // 방향에 따른 셀사이 간격 캐싱.
        private float mCacheSpacing;

        // 방향에 따른 Content의 Padding 캐싱.
        private float mCachePadding;

        // 방향에 따른 Content의 양방향 Padding 캐싱.
        private float mCachePaddingBoth;

        // 셀의 다른방향의 길이 캐싱.
        private float mCacheGridCellOtherSize;

        // Grid의 첫셀의 다른방향 위치 캐싱.
        private float mCacheGridFirstCellOtherPos;

        // 직전 최상위 데이터의 인덱스.
        private int mPrevTopData;

        // 초기화 당시 렉트 사이즈.
        private float mOriginRectSize;

        // 셀 목록.
        private readonly List<IScrollCell> mCellList = new List<IScrollCell>();
        private readonly List<RectTransform> mCellRectList = new List<RectTransform>();

        // 데이터 목록.
        private readonly List<IScrollData> mDataList = new List<IScrollData>();

        public int DataLength => mDataList.Count;

        #endregion
    }

    public interface IScrollCell
    {
        void UpdateCell(IScrollData data);
    }

    public interface IScrollData
    {
    }
}