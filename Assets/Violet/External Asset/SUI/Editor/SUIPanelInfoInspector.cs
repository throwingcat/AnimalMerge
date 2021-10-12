using UnityEditor;
using UnityEngine;

namespace Violet
{
    [CustomEditor(typeof(SUIPanelInfo), true)]
    public class SUIPanelInfoInspector : Editor
    {
        int _stackCount;
        GUIStyle _style;

        private void OnEnable()
        {
            _stackCount = 0;

            _style = new GUIStyle();
            _style.richText = true;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            _stackCount = SUIPanel.StackCount;
            EditorGUILayout.LabelField("Stack Count", _stackCount.ToString());

            EditorGUI.indentLevel++;
            for (int i = 0; i < SUIPanel.StackCount; i++)
            {
                SUIPanel targetPanel = SUIPanel.GetPanel(i);
                string showName = (targetPanel.IsShow ? "<color=green>" : "<color=#555555>") + targetPanel.name + "</color>";
                EditorGUILayout.LabelField(targetPanel.IsPopup ? "Popup" : "Panel", showName, _style);
            }
            EditorGUI.indentLevel--;

            EditorGUILayout.LabelField("IgnoreBackPress", SUIPanel.IgnoreBackPress.ToString());

            serializedObject.ApplyModifiedProperties();
        }

        public override bool RequiresConstantRepaint()
        {
            return SUIPanel.StackCount != _stackCount;
        }

    }
}