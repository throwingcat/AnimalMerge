using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Violet
{
    [RequireComponent(typeof(Text))]
    [RequireComponent(typeof(RectMask2D))]
    public sealed class SUIFlowText : BaseMeshEffect
    {
        // 시작점(0은 본래위치)
        public float startOffset;

        // 흐르는 속도.
        public float speed = 1f;

        // 크기 관계없이 강제로 흐를지 여부.
        public bool forceFlow;

        private readonly List<UIVertex> _list = new List<UIVertex>();
        private RectMask2D _mask;

        private float _offset;
        private RectTransform _rectTrn;
        private Text _targetText;

        public float Offset
        {
            get => _offset;
            set
            {
                if (_offset == value)
                    return;
                _offset = value;

                if (graphic != null)
                    graphic.SetVerticesDirty();
            }
        }

        protected override void Awake()
        {
            base.Awake();

            SetReference();
        }

        private void Update()
        {
            if (!Application.isPlaying)
                return;

            // Text가 RectTransform Width보다 길지 않으면 이동하지 않음.
            if (!forceFlow && _rectTrn.sizeDelta.x > _targetText.preferredWidth)
            {
                Offset = startOffset;
                return;
            }

            var targetWidth = _rectTrn.sizeDelta.x > _targetText.preferredWidth
                ? _rectTrn.sizeDelta.x
                : _targetText.preferredWidth;

            Offset = _offset - speed;
            if (_offset < -targetWidth)
                _offset = targetWidth;
            else if (_offset > targetWidth)
                _offset = -targetWidth;
        }

        protected override void OnEnable()
        {
            Offset = startOffset;
            base.OnEnable();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            if (Application.isPlaying)
                return;

            SetReference();

            _targetText.horizontalOverflow = HorizontalWrapMode.Overflow;
            _targetText.verticalOverflow = VerticalWrapMode.Overflow;

            Offset = startOffset;
            base.OnValidate();
        }
#endif

        private void SetReference()
        {
            if (_rectTrn == null)
                _rectTrn = GetComponent<RectTransform>();
            if (_targetText == null)
                _targetText = GetComponent<Text>();
            if (_mask == null)
                _mask = GetComponent<RectMask2D>();

            _mask.AddClippable(_targetText);
            _targetText.RecalculateClipping();
        }

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive())
                return;

            _list.Clear();
            vh.GetUIVertexStream(_list);

            ModifyVertices(_list);

            vh.Clear();
            vh.AddUIVertexTriangleStream(_list);
        }

        public void ModifyVertices(List<UIVertex> verts)
        {
            if (!IsActive())
                return;

            var lines = _targetText.text.Split('\n');
            Vector3 pos;
            var letterOffset = _targetText.fontSize / 100f;
            float alignmentFactor = 0;
            var glyphIdx = 0;

            switch (_targetText.alignment)
            {
                case TextAnchor.LowerLeft:
                case TextAnchor.MiddleLeft:
                case TextAnchor.UpperLeft:
                    alignmentFactor = 0f;
                    break;

                case TextAnchor.LowerCenter:
                case TextAnchor.MiddleCenter:
                case TextAnchor.UpperCenter:
                    alignmentFactor = 0.5f;
                    break;

                case TextAnchor.LowerRight:
                case TextAnchor.MiddleRight:
                case TextAnchor.UpperRight:
                    alignmentFactor = 1f;
                    break;
            }

            for (var lineIdx = 0; lineIdx < lines.Length; lineIdx++)
            {
                var line = lines[lineIdx];
                var lineOffset = (line.Length - 1) * letterOffset * alignmentFactor;

                for (var charIdx = 0; charIdx < line.Length; charIdx++)
                {
                    var idx1 = glyphIdx * 6 + 0;
                    var idx2 = glyphIdx * 6 + 1;
                    var idx3 = glyphIdx * 6 + 2;
                    var idx4 = glyphIdx * 6 + 3;
                    var idx5 = glyphIdx * 6 + 4;
                    var idx6 = glyphIdx * 6 + 5;

                    // Check for truncated text (doesn't generate verts for all characters)
                    if (idx6 > verts.Count - 1) return;

                    var vert1 = verts[idx1];
                    var vert2 = verts[idx2];
                    var vert3 = verts[idx3];
                    var vert4 = verts[idx4];
                    var vert5 = verts[idx5];
                    var vert6 = verts[idx6];

                    pos = Vector3.right * _offset;
                    vert1.position += pos;
                    vert2.position += pos;
                    vert3.position += pos;
                    vert4.position += pos;
                    vert5.position += pos;
                    vert6.position += pos;

                    verts[idx1] = vert1;
                    verts[idx2] = vert2;
                    verts[idx3] = vert3;
                    verts[idx4] = vert4;
                    verts[idx5] = vert5;
                    verts[idx6] = vert6;

                    glyphIdx++;
                }

                // Offset for carriage return character that still generates verts
                glyphIdx++;
            }
        }
    }
}