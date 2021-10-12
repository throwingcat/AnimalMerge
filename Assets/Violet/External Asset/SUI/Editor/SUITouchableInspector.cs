using UnityEditor;
using UnityEditor.UI;

namespace Violet
{
    [CustomEditor(typeof(SUITouchable), true)]
    [CanEditMultipleObjects]
    public class SUITouchableInspector : GraphicEditor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            RaycastControlsGUI();
            serializedObject.ApplyModifiedProperties();
        }

    }
}