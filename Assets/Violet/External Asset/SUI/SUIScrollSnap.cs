using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Violet
{
    [RequireComponent(typeof(ScrollRect))]
    public class SUIScrollSnap : MonoBehaviour, IBeginDragHandler, IEndDragHandler
    {
        public RectTransform snapTarget;

        // 스냅 강도(높을수록 빠르게 이동)
        public float snapPower = 7.5f;

        // 최소 스냅 인식거리(80pixel 넘게 드래그해야 스냅)
        public float dragThreshold = 80f;

        // 최소 스냅 인식시간(1초안에 dragThreshold를 넘어야 스냅)
        public float dragTime = 1f;
        private readonly List<Vector3> _childPosList = new List<Vector3>();
        private bool _isDraging, _isMove, _isVertical;
        private RectTransform _rectTrnScrollRect, _rectTrnContent;

        private ScrollRect _scrollRect;
        private Vector2 _startPos, _endPos, _resultPos;
        private float _startTime, _endTime, _maxContentPos;
        public Action<int> OnChangeTargetSlotNum = null;

        // 초기화.
        private void Awake()
        {
            _scrollRect = GetComponent<ScrollRect>();
            _rectTrnScrollRect = GetComponent<RectTransform>();
            _rectTrnContent = _scrollRect.content.GetComponent<RectTransform>();
            _isVertical = _scrollRect.vertical;
        }

        // 대상위치로 이동.
        private void Update()
        {
            // 드래그중엔 무시.
            if (_isDraging)
                return;

            // 멈췄으면 무시.
            if (!_isMove)
                return;

            // Content를 벗어났으면 무시.
            if (IsOutOfContent())
            {
                _isMove = false;
                return;
            }

            // 대상위치로 이동.
            _rectTrnContent.anchoredPosition =
                Vector3.Lerp(_rectTrnContent.anchoredPosition, _resultPos, snapPower * Time.deltaTime);
            if (Vector3.Distance(_rectTrnContent.anchoredPosition, _resultPos) < 0.01f)
                _isMove = false;
        }

        // 드래그 시작.
        public void OnBeginDrag(PointerEventData eventData)
        {
            _isDraging = true;
            _startTime = Time.realtimeSinceStartup;
            _startPos = _rectTrnContent.anchoredPosition;
        }

        // 드래그 종료.
        public void OnEndDrag(PointerEventData eventData)
        {
            _isMove = true;
            _isDraging = false;
            _endTime = Time.realtimeSinceStartup;
            _endPos = _rectTrnContent.anchoredPosition;

            SetResultPos();
        }

        // 특정 슬롯으로 이동
        public void OnMoveTargetSlot(int number)
        {
            if (_isMove && Vector3.Distance(_rectTrnContent.anchoredPosition, _resultPos) > 10f) return;

            _childPosList.Clear();
            foreach (RectTransform rectTrnChild in _rectTrnContent)
                _childPosList.Add(rectTrnChild.position);

            if (_childPosList.Count <= number) return;

            Vector2 childVec = snapTarget.position - _childPosList[number];
            var childDistance = _isVertical ? childVec.y : childVec.x;

            _isMove = true;
            _isDraging = false;

            _resultPos = _rectTrnContent.anchoredPosition + (_isVertical ? Vector2.up : Vector2.right) * childDistance;
            _maxContentPos = _isVertical
                ? _rectTrnContent.rect.height - _rectTrnScrollRect.rect.height
                : -(_rectTrnContent.rect.width - _rectTrnScrollRect.rect.width);

            if (childDistance != 0)
                _scrollRect.StopMovement();

            ChangeTargetSlotNumber(number);
        }

        // 타겟 슬롯 번호 변경 시
        private void ChangeTargetSlotNumber(int count)
        {
            if (OnChangeTargetSlotNum != null)
                OnChangeTargetSlotNum(count);
        }

        // 최종 이동위치 계산.
        private void SetResultPos()
        {
            // 하위요소 위치 준비.
            _childPosList.Clear();
            foreach (RectTransform rectTrnChild in _rectTrnContent)
                _childPosList.Add(rectTrnChild.position);

            // 데이터 준비.
            var durationTime = _endTime - _startTime;
            var distanceVec = _endPos - _startPos;
            var distance = _isVertical ? distanceVec.y : distanceVec.x;
            float resultPower = 0;

            // 제한보다 드래그했으면 최종값 계산.
            if (durationTime > 0 && durationTime < dragTime && Mathf.Abs(distance) > dragThreshold)
                resultPower = distance * (1f / durationTime);

            // 가장 가까운 요소 찾기.
            Vector2 snapPos = snapTarget.position;
            var minDistance = resultPower < 0 ? float.MinValue : float.MaxValue;
            var targetChild = Vector2.zero;
            var count = 0;
            var targetSlotCount = 0;
            foreach (Vector2 childPos in _childPosList)
            {
                var childVec = snapPos - childPos;
                var childDistance = _isVertical ? childVec.y : childVec.x;
                if (resultPower == 0 && Mathf.Abs(minDistance) > Mathf.Abs(childDistance) ||
                    resultPower > 0 && minDistance > childDistance && childDistance > 0 ||
                    resultPower < 0 && minDistance < childDistance && childDistance < 0)
                {
                    minDistance = childDistance;
                    targetChild = childPos;
                    targetSlotCount = count;
                }

                count++;
            }

            // 가까운 요소를 못찾으면 스냅 중지.
            if (minDistance == float.MinValue || minDistance == float.MaxValue)
            {
                _isMove = false;
                return;
            }

            _resultPos = _rectTrnContent.anchoredPosition + (_isVertical ? Vector2.up : Vector2.right) * minDistance;
            _maxContentPos = _isVertical
                ? _rectTrnContent.rect.height - _rectTrnScrollRect.rect.height
                : -(_rectTrnContent.rect.width - _rectTrnScrollRect.rect.width);

            if (minDistance != 0)
                _scrollRect.StopMovement();

            // 몇번째 슬롯을 타겟하고 있는지 알기위한 콜백
            ChangeTargetSlotNumber(targetSlotCount);
        }

        // Content를 벗어나는지 검사.
        private bool IsOutOfContent()
        {
            var targetPos = _rectTrnContent.anchoredPosition;
            return _isVertical && (targetPos.y < 0 || targetPos.y > _maxContentPos) ||
                   !_isVertical && (targetPos.x > 0 || targetPos.x < _maxContentPos);
        }
    }
}