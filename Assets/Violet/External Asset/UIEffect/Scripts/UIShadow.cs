using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace Coffee.UIExtensions
{
	/// <summary>
	///     UIEffect.
	/// </summary>
    [AddComponentMenu("UI/UIEffect/UIShadow", 100)]
    public class UIShadow : Shadow, IParameterTexture
#if UNITY_EDITOR
        , ISerializationCallbackReceiver
#endif
    {
        private static readonly List<UIShadow> tmpShadows = new List<UIShadow>();

        //################################
        // Private Members.
        //################################
        private static readonly List<UIVertex> s_Verts = new List<UIVertex>();

        //################################
        // Serialize Members.
        //################################
        [Tooltip("How far is the blurring shadow from the graphic.")]
        [FormerlySerializedAs("m_Blur")]
        [SerializeField]
        [Range(0, 1)]
        private float m_BlurFactor = 1;

        [Tooltip("Shadow effect style.")] [SerializeField]
        private ShadowStyle m_Style = ShadowStyle.Shadow;

#pragma warning disable 0414
        [HideInInspector] [Obsolete] [SerializeField]
        private List<AdditionalShadow> m_AdditionalShadows = new List<AdditionalShadow>();
#pragma warning restore 0414

        private int _graphicVertexCount;

        private UIEffect _uiEffect;


        //################################
        // Public Members.
        //################################
        /// <summary>
        ///     How far is the blurring shadow from the graphic.
        /// </summary>
        [Obsolete("Use blurFactor instead (UnityUpgradable) -> blurFactor")]
        public float blur
        {
            get => m_BlurFactor;
            set
            {
                m_BlurFactor = Mathf.Clamp(value, 0, 2);
                _SetDirty();
            }
        }

        /// <summary>
        ///     How far is the blurring shadow from the graphic.
        /// </summary>
        public float blurFactor
        {
            get => m_BlurFactor;
            set
            {
                m_BlurFactor = Mathf.Clamp(value, 0, 2);
                _SetDirty();
            }
        }

        /// <summary>
        ///     Shadow effect style.
        /// </summary>
        public ShadowStyle style
        {
            get => m_Style;
            set
            {
                m_Style = value;
                _SetDirty();
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        /// <summary>
        ///     Gets or sets the parameter index.
        /// </summary>
        public int parameterIndex { get; set; }

        /// <summary>
        ///     Gets the parameter texture.
        /// </summary>
        public ParameterTexture ptex { get; private set; }


        /// <summary>
        ///     Modifies the mesh.
        /// </summary>
        public override void ModifyMesh(VertexHelper vh)
        {
            if (!isActiveAndEnabled || vh.currentVertCount <= 0 || m_Style == ShadowStyle.None) return;

            vh.GetUIVertexStream(s_Verts);

            GetComponents(tmpShadows);

            foreach (var s in tmpShadows)
                if (s.isActiveAndEnabled)
                {
                    if (s == this)
                        foreach (var s2 in tmpShadows)
                            s2._graphicVertexCount = s_Verts.Count;
                    break;
                }

            tmpShadows.Clear();

            //================================
            // Append shadow vertices.
            //================================
            {
                _uiEffect = _uiEffect ?? GetComponent<UIEffect>();
                var start = s_Verts.Count - _graphicVertexCount;
                var end = s_Verts.Count;

                if (ptex != null && _uiEffect && _uiEffect.isActiveAndEnabled)
                {
                    ptex.SetData(this, 0, _uiEffect.effectFactor); // param.x : effect factor
                    ptex.SetData(this, 1, 255); // param.y : color factor
                    ptex.SetData(this, 2, m_BlurFactor); // param.z : blur factor
                }

                _ApplyShadow(s_Verts, effectColor, ref start, ref end, effectDistance, style, useGraphicAlpha);
            }

            vh.Clear();
            vh.AddUIVertexTriangleStream(s_Verts);

            s_Verts.Clear();
        }

        /// <summary>
        ///     Append shadow vertices.
        ///     * It is similar to Shadow component implementation.
        /// </summary>
        private void _ApplyShadow(List<UIVertex> verts, Color color, ref int start, ref int end, Vector2 effectDistance,
            ShadowStyle style, bool useGraphicAlpha)
        {
            if (style == ShadowStyle.None || color.a <= 0)
                return;

            // Append Shadow.
            _ApplyShadowZeroAlloc(s_Verts, color, ref start, ref end, effectDistance.x, effectDistance.y,
                useGraphicAlpha);

            // Append Shadow3.
            if (ShadowStyle.Shadow3 == style)
            {
                _ApplyShadowZeroAlloc(s_Verts, color, ref start, ref end, effectDistance.x, 0, useGraphicAlpha);
                _ApplyShadowZeroAlloc(s_Verts, color, ref start, ref end, 0, effectDistance.y, useGraphicAlpha);
            }

            // Append Outline.
            else if (ShadowStyle.Outline == style)
            {
                _ApplyShadowZeroAlloc(s_Verts, color, ref start, ref end, effectDistance.x, -effectDistance.y,
                    useGraphicAlpha);
                _ApplyShadowZeroAlloc(s_Verts, color, ref start, ref end, -effectDistance.x, effectDistance.y,
                    useGraphicAlpha);
                _ApplyShadowZeroAlloc(s_Verts, color, ref start, ref end, -effectDistance.x, -effectDistance.y,
                    useGraphicAlpha);
            }

            // Append Outline8.
            else if (ShadowStyle.Outline8 == style)
            {
                _ApplyShadowZeroAlloc(s_Verts, color, ref start, ref end, effectDistance.x, -effectDistance.y,
                    useGraphicAlpha);
                _ApplyShadowZeroAlloc(s_Verts, color, ref start, ref end, -effectDistance.x, effectDistance.y,
                    useGraphicAlpha);
                _ApplyShadowZeroAlloc(s_Verts, color, ref start, ref end, -effectDistance.x, -effectDistance.y,
                    useGraphicAlpha);
                _ApplyShadowZeroAlloc(s_Verts, color, ref start, ref end, -effectDistance.x, 0, useGraphicAlpha);
                _ApplyShadowZeroAlloc(s_Verts, color, ref start, ref end, 0, -effectDistance.y, useGraphicAlpha);
                _ApplyShadowZeroAlloc(s_Verts, color, ref start, ref end, effectDistance.x, 0, useGraphicAlpha);
                _ApplyShadowZeroAlloc(s_Verts, color, ref start, ref end, 0, effectDistance.y, useGraphicAlpha);
            }
        }

        /// <summary>
        ///     Append shadow vertices.
        ///     * It is similar to Shadow component implementation.
        /// </summary>
        private void _ApplyShadowZeroAlloc(List<UIVertex> verts, Color color, ref int start, ref int end, float x,
            float y, bool useGraphicAlpha)
        {
            // Check list capacity.
            var count = end - start;
            var neededCapacity = verts.Count + count;
            if (verts.Capacity < neededCapacity)
                verts.Capacity = neededCapacity;

            var normalizedIndex = ptex != null && _uiEffect && _uiEffect.isActiveAndEnabled
                ? ptex.GetNormalizedIndex(this)
                : -1;

            // Add 
            var vt = default(UIVertex);
            for (var i = 0; i < count; i++) verts.Add(vt);

            // Move
            for (var i = verts.Count - 1; count <= i; i--) verts[i] = verts[i - count];

            // Append shadow vertices to the front of list.
            // * The original vertex is pushed backward.
            for (var i = 0; i < count; ++i)
            {
                vt = verts[i + start + count];

                var v = vt.position;
                vt.position.Set(v.x + x, v.y + y, v.z);

                var vertColor = effectColor;
                vertColor.a = useGraphicAlpha ? color.a * vt.color.a / 255 : color.a;
                vt.color = vertColor;


                // Set UIEffect prameters
                if (0 <= normalizedIndex)
                    vt.uv0 = new Vector2(
                        vt.uv0.x,
                        normalizedIndex
                    );

                verts[i] = vt;
            }

            // Update next shadow offset.
            start = end;
            end = verts.Count;
        }

        /// <summary>
        ///     Mark the UIEffect as dirty.
        /// </summary>
        private void _SetDirty()
        {
            if (graphic)
                graphic.SetVerticesDirty();
        }

        /// <summary>
        ///     Additional shadow.
        /// </summary>
        [Obsolete]
        [Serializable]
        public class AdditionalShadow
        {
	        /// <summary>
	        ///     How far is the blurring shadow from the graphic.
	        /// </summary>
	        [FormerlySerializedAs("shadowBlur")] [Range(0, 1)]
            public float blur = 0.25f;

	        /// <summary>
	        ///     Shadow effect mode.
	        /// </summary>
	        [FormerlySerializedAs("shadowMode")] public ShadowStyle style = ShadowStyle.Shadow;

	        /// <summary>
	        ///     Color for the shadow effect.
	        /// </summary>
	        [FormerlySerializedAs("shadowColor")] public Color effectColor = Color.black;

	        /// <summary>
	        ///     How far is the shadow from the graphic.
	        /// </summary>
	        public Vector2 effectDistance = new Vector2(1f, -1f);

	        /// <summary>
	        ///     Should the shadow inherit the alpha from the graphic?
	        /// </summary>
	        public bool useGraphicAlpha = true;
        }

#if UNITY_EDITOR
        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            EditorApplication.delayCall += UpgradeIfNeeded;
        }


#pragma warning disable 0612
        private void UpgradeIfNeeded()
        {
            if (0 < m_AdditionalShadows.Count)
            {
                foreach (var s in m_AdditionalShadows)
                {
                    if (s.style == ShadowStyle.None) continue;

                    var shadow = gameObject.AddComponent<UIShadow>();
                    shadow.style = s.style;
                    shadow.effectDistance = s.effectDistance;
                    shadow.effectColor = s.effectColor;
                    shadow.useGraphicAlpha = s.useGraphicAlpha;
                    shadow.blurFactor = s.blur;
                }

                m_AdditionalShadows = null;
            }
        }
#pragma warning restore 0612
#endif
    }
}