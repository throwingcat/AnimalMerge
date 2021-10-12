using UnityEngine;
using UnityEngine.UI;

namespace Coffee.UIExtensions
{
    [RequireComponent(typeof(Graphic))]
    [DisallowMultipleComponent]
    [AddComponentMenu("UI/UIEffect/UIFlip", 102)]
    public class UIFlip : BaseMeshEffect
    {
        //################################
        // Serialize Members.
        //################################

        [Tooltip("Flip horizontally.")] [SerializeField]
        private bool m_Horizontal;

        [Tooltip("Flip vertically.")] [SerializeField]
        private bool m_Veritical;

        //################################
        // Public Members.
        //################################
        /// <summary>
        ///     Gets or sets a value indicating whether this <see cref="Coffee.UIExtensions.UIFlip" /> should be flipped
        ///     horizontally.
        /// </summary>
        /// <value><c>true</c> if be flipped horizontally; otherwise, <c>false</c>.</value>
        public bool horizontal
        {
            get => m_Horizontal;
            set => m_Horizontal = value;
        }

        /// <summary>
        ///     Gets or sets a value indicating whether this <see cref="Coffee.UIExtensions.UIFlip" /> should be flipped
        ///     vertically.
        /// </summary>
        /// <value><c>true</c> if be flipped horizontally; otherwise, <c>false</c>.</value>
        public bool vertical
        {
            get => m_Veritical;
            set => m_Veritical = value;
        }

        /// <summary>
        ///     Call used to modify mesh.
        /// </summary>
        /// <param name="vh">VertexHelper.</param>
        public override void ModifyMesh(VertexHelper vh)
        {
            var rt = graphic.rectTransform;
            var vt = default(UIVertex);
            Vector3 pos;
            var center = rt.rect.center;
            for (var i = 0; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref vt, i);
                pos = vt.position;
                vt.position = new Vector3(
                    m_Horizontal ? -pos.x : pos.x,
                    m_Veritical ? -pos.y : pos.y
//					m_Horizontal ? (pos.x + (center.x - pos.x) * 2) : pos.x,
//					m_Veritical ? (pos.y + (center.y - pos.y) * 2) : pos.y
                );
                vh.SetUIVertex(vt, i);
            }
        }
    }
}