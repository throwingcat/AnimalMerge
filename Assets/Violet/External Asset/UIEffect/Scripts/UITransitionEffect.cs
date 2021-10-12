using UnityEngine;
using UnityEngine.UI;

namespace Coffee.UIExtensions
{
	/// <summary>
	///     Transition effect.
	/// </summary>
	[AddComponentMenu("UI/UIEffect/UITransitionEffect", 5)]
    public class UITransitionEffect : UIEffectBase
    {
	    /// <summary>
	    ///     Effect mode.
	    /// </summary>
	    public enum EffectMode
        {
            Fade = 1,
            Cutoff = 2,
            Dissolve = 3
        }

        //################################
        // Constant or Static Members.
        //################################
        public const string shaderName = "UI/Hidden/UI-Effect-Transition";
        private static readonly ParameterTexture _ptex = new ParameterTexture(8, 128, "_ParamTex");


        //################################
        // Serialize Members.
        //################################
        [Tooltip("Effect mode.")] [SerializeField]
        private EffectMode m_EffectMode = EffectMode.Cutoff;

        [Tooltip("Effect factor between 0(hidden) and 1(shown).")] [SerializeField] [Range(0, 1)]
        private float m_EffectFactor = 1;

        [Tooltip("Transition texture (single channel texture).")] [SerializeField]
        private Texture m_TransitionTexture;

        [Tooltip("The area for effect.")] [SerializeField]
        private EffectArea m_EffectArea = EffectArea.RectTransform;

        [Tooltip("Keep effect aspect ratio.")] [SerializeField]
        private bool m_KeepAspectRatio;

        [Tooltip("Dissolve edge width.")] [SerializeField] [Range(0, 1)]
        private float m_DissolveWidth = 0.5f;

        [Tooltip("Dissolve edge softness.")] [SerializeField] [Range(0, 1)]
        private float m_DissolveSoftness = 0.5f;

        [Tooltip("Dissolve edge color.")] [SerializeField] [ColorUsage(false)]
        private Color m_DissolveColor = new Color(0.0f, 0.25f, 1.0f);

        [Tooltip("Disable graphic's raycast target on hidden.")] [SerializeField]
        private bool m_PassRayOnHidden;

        [Header("Effect Player")] [SerializeField]
        private EffectPlayer m_Player;

        //################################
        // Private Members.
        //################################
        private MaterialCache _materialCache;

        //################################
        // Public Members.
        //################################
        /// <summary>
        ///     Effect factor between 0(no effect) and 1(complete effect).
        /// </summary>
        public float effectFactor
        {
            get => m_EffectFactor;
            set
            {
                value = Mathf.Clamp(value, 0, 1);
                if (!Mathf.Approximately(m_EffectFactor, value))
                {
                    m_EffectFactor = value;
                    SetDirty();
                }
            }
        }

        /// <summary>
        ///     Transition texture.
        /// </summary>
        public Texture transitionTexture
        {
            get => m_TransitionTexture;
            set
            {
                if (m_TransitionTexture != value)
                {
                    m_TransitionTexture = value;
                    if (graphic) ModifyMaterial();
                }
            }
        }

        /// <summary>
        ///     Effect mode.
        /// </summary>
        public EffectMode effectMode => m_EffectMode;

        /// <summary>
        ///     Keep aspect ratio.
        /// </summary>
        public bool keepAspectRatio
        {
            get => m_KeepAspectRatio;
            set
            {
                if (m_KeepAspectRatio != value)
                {
                    m_KeepAspectRatio = value;
                    targetGraphic.SetVerticesDirty();
                }
            }
        }

        /// <summary>
        ///     Gets the parameter texture.
        /// </summary>
        public override ParameterTexture ptex => _ptex;

        /// <summary>
        ///     Dissolve edge width.
        /// </summary>
        public float dissolveWidth
        {
            get => m_DissolveWidth;
            set
            {
                value = Mathf.Clamp(value, 0, 1);
                if (!Mathf.Approximately(m_DissolveWidth, value))
                {
                    m_DissolveWidth = value;
                    SetDirty();
                }
            }
        }

        /// <summary>
        ///     Dissolve edge softness.
        /// </summary>
        public float dissolveSoftness
        {
            get => m_DissolveSoftness;
            set
            {
                value = Mathf.Clamp(value, 0, 1);
                if (!Mathf.Approximately(m_DissolveSoftness, value))
                {
                    m_DissolveSoftness = value;
                    SetDirty();
                }
            }
        }

        /// <summary>
        ///     Dissolve edge color.
        /// </summary>
        public Color dissolveColor
        {
            get => m_DissolveColor;
            set
            {
                if (m_DissolveColor != value)
                {
                    m_DissolveColor = value;
                    SetDirty();
                }
            }
        }

        /// <summary>
        ///     Duration for showing/hiding.
        /// </summary>
        public float duration
        {
            get => _player.duration;
            set => _player.duration = Mathf.Max(value, 0.1f);
        }

        /// <summary>
        ///     Disable graphic's raycast target on hidden.
        /// </summary>
        public bool passRayOnHidden
        {
            get => m_PassRayOnHidden;
            set => m_PassRayOnHidden = value;
        }

        /// <summary>
        ///     Update mode for showing/hiding.
        /// </summary>
        public AnimatorUpdateMode updateMode
        {
            get => _player.updateMode;
            set => _player.updateMode = value;
        }

        private EffectPlayer _player => m_Player ?? (m_Player = new EffectPlayer());

        //################################
        // Protected Members.
        //################################

        /// <summary>
        ///     This function is called when the object becomes enabled and active.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
            _player.OnEnable();
            _player.loop = false;
        }

        /// <summary>
        ///     This function is called when the behaviour becomes disabled () or inactive.
        /// </summary>
        protected override void OnDisable()
        {
            MaterialCache.Unregister(_materialCache);
            _materialCache = null;
            base.OnDisable();
            _player.OnDisable();
        }

        /// <summary>
        ///     Show transition.
        /// </summary>
        public void Show()
        {
            _player.loop = false;
            _player.Play(f => effectFactor = f);
        }

        /// <summary>
        ///     Hide transition.
        /// </summary>
        public void Hide()
        {
            _player.loop = false;
            _player.Play(f => effectFactor = 1 - f);
        }

        /// <summary>
        ///     Modifies the material.
        /// </summary>
        public override void ModifyMaterial()
        {
            var hash = (m_TransitionTexture ? (uint) m_TransitionTexture.GetInstanceID() : 0) + ((ulong) 2 << 32) +
                       ((ulong) m_EffectMode << 36);
            if (_materialCache != null && (_materialCache.hash != hash || !isActiveAndEnabled || !m_EffectMaterial))
            {
                MaterialCache.Unregister(_materialCache);
                _materialCache = null;
            }

            if (!isActiveAndEnabled || !m_EffectMaterial)
            {
                graphic.material = null;
            }
            else if (!m_TransitionTexture)
            {
                graphic.material = m_EffectMaterial;
            }
            else if (_materialCache != null && _materialCache.hash == hash)
            {
                graphic.material = _materialCache.material;
            }
            else
            {
                _materialCache = MaterialCache.Register(hash, m_TransitionTexture, () =>
                {
                    var mat = new Material(m_EffectMaterial);
                    mat.name += "_" + m_TransitionTexture.name;
                    mat.SetTexture("_TransitionTexture", m_TransitionTexture);
                    return mat;
                });
                graphic.material = _materialCache.material;
            }
        }

        /// <summary>
        ///     Modifies the mesh.
        /// </summary>
        public override void ModifyMesh(VertexHelper vh)
        {
            if (!isActiveAndEnabled) return;

            // rect.
            var tex = transitionTexture;
            var aspectRatio = m_KeepAspectRatio && tex ? (float) tex.width / tex.height : -1;
            var rect = m_EffectArea.GetEffectArea(vh, graphic, aspectRatio);

            // Set prameters to vertex.
            var normalizedIndex = ptex.GetNormalizedIndex(this);
            var vertex = default(UIVertex);
            var effectEachCharacter = graphic is Text && m_EffectArea == EffectArea.Character;
            float x, y;
            var count = vh.currentVertCount;
            for (var i = 0; i < count; i++)
            {
                vh.PopulateUIVertex(ref vertex, i);

                if (effectEachCharacter)
                {
                    x = splitedCharacterPosition[i % 4].x;
                    y = splitedCharacterPosition[i % 4].y;
                }
                else
                {
                    x = Mathf.Clamp01(vertex.position.x / rect.width + 0.5f);
                    y = Mathf.Clamp01(vertex.position.y / rect.height + 0.5f);
                }

                vertex.uv0 = new Vector2(
                    Packer.ToFloat(vertex.uv0.x, vertex.uv0.y),
                    Packer.ToFloat(x, y, normalizedIndex)
                );
                vh.SetUIVertex(vertex, i);
            }
        }

        protected override void SetDirty()
        {
            ptex.RegisterMaterial(targetGraphic.material);
            ptex.SetData(this, 0, m_EffectFactor); // param1.x : effect factor
            if (m_EffectMode == EffectMode.Dissolve)
            {
                ptex.SetData(this, 1, m_DissolveWidth); // param1.y : width
                ptex.SetData(this, 2, m_DissolveSoftness); // param1.z : softness
                ptex.SetData(this, 4, m_DissolveColor.r); // param2.x : red
                ptex.SetData(this, 5, m_DissolveColor.g); // param2.y : green
                ptex.SetData(this, 6, m_DissolveColor.b); // param2.z : blue
            }

            // Disable graphic's raycastTarget on hidden.
            if (m_PassRayOnHidden) graphic.raycastTarget = 0 < m_EffectFactor;
        }

#if UNITY_EDITOR
	    /// <summary>
	    ///     Gets the material.
	    /// </summary>
	    /// <returns>The material.</returns>
	    protected override Material GetMaterial()
        {
            return MaterialResolver.GetOrGenerateMaterialVariant(Shader.Find(shaderName), m_EffectMode);
        }
#endif
    }
}