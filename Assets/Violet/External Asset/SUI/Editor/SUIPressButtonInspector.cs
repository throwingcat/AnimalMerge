using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Violet
{
    [CustomEditor(typeof(SUIPressButton), true)]
    [CanEditMultipleObjects]
    public class SUIPressButtonInspector : GraphicEditor
    {
        private SerializedProperty _pressType;
        private SerializedProperty _scaleTarget;
        private SerializedProperty _imgTarget;
        private SerializedProperty _sprTarget;
        private SerializedProperty _colorTarget;
        private SerializedProperty _onPressProp;
        private SerializedProperty _onReleaseProp;
        private SerializedProperty _onClickProp;

        private GUIContent _pressTypeContent;
        private GUIContent _spriteContent;
        private GUIContent _imageContent;
        private GUIContent _colorContent;
        private GUIContent _scaleContent;

        private SUIPressButton _targetButton;

        protected override void OnEnable()
        {
            base.OnEnable();

            _pressTypeContent = new GUIContent("Press Type");
            _spriteContent = new GUIContent("Sprite");
            _imageContent = new GUIContent("Target Image");
            _colorContent = new GUIContent("Color");
            _scaleContent = new GUIContent("Scale");

            _pressType = serializedObject.FindProperty("pressType");
            _scaleTarget = serializedObject.FindProperty("scaleTarget");
            _imgTarget = serializedObject.FindProperty("imgTarget");
            _sprTarget = serializedObject.FindProperty("sprTarget");
            _colorTarget = serializedObject.FindProperty("colorTarget");
            _onPressProp = serializedObject.FindProperty("onPress");
            _onReleaseProp = serializedObject.FindProperty("onRelease");
            _onClickProp = serializedObject.FindProperty("onClick");

            _targetButton = target as SUIPressButton;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_imgTarget, _imageContent);
            if (_targetButton.imgTarget == null)
                _targetButton.imgTarget = _targetButton.GetComponent<Image>();

            EditorGUILayout.PropertyField(_pressType, _pressTypeContent);
            ePRESS_TYPE btnType = (ePRESS_TYPE)_pressType.enumValueIndex;
            switch (btnType)
            {
                case ePRESS_TYPE.Color:
                    EditorGUILayout.PropertyField(_colorTarget, _colorContent);
                    break;
                case ePRESS_TYPE.Sprite:
                    EditorGUILayout.PropertyField(_sprTarget, _spriteContent);
                    if (_targetButton.sprTarget == null && _targetButton.imgTarget != null)
                        _targetButton.sprTarget = _targetButton.imgTarget.sprite;
                    break;
                case ePRESS_TYPE.Scale:
                    EditorGUILayout.PropertyField(_scaleTarget, _scaleContent);
                    break;
                default:
                    break;
            }

            EditorGUILayout.PropertyField(_onPressProp);
            EditorGUILayout.PropertyField(_onReleaseProp);
            EditorGUILayout.PropertyField(_onClickProp);

            serializedObject.ApplyModifiedProperties();
        }

    }
}