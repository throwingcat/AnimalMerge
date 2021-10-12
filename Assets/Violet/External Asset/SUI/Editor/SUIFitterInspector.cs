using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

[CustomEditor(typeof(SUIFitter), true)]
[CanEditMultipleObjects]
public class SUIFitterInspector : GraphicEditor
{
    private SerializedProperty _targetText;
    //private SerializedProperty _offset;

    private GUIContent _offsetContent;
    private GUIContent _targetTextContent;
    private GUIContent _buttonContent;

    private SUIFitter _fitter;

    protected override void OnEnable()
    {
        base.OnEnable();

        _offsetContent = new GUIContent("Offset");
        _targetTextContent = new GUIContent("Target Text");
        _buttonContent = new GUIContent("Change Now");

        _targetText = serializedObject.FindProperty("targetText");
        //_offset = serializedObject.FindProperty("offset");

        _fitter = target as SUIFitter;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(_targetText, _targetTextContent);
        if (EditorGUI.EndChangeCheck())
        {
            _fitter.targetText = _targetText.objectReferenceValue as Text;
            _fitter.ChangeRect();
        }

        Vector2 targetOffset = EditorGUILayout.Vector2Field(_offsetContent, _fitter.offset);
        if (targetOffset != _fitter.offset)
        {
            _fitter.offset = targetOffset;
            _fitter.ChangeRect();
        }

        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(EditorGUIUtility.labelWidth);
        if (GUILayout.Button(_buttonContent, EditorStyles.miniButton))
            _fitter.ChangeRect();
        EditorGUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();
    }
}