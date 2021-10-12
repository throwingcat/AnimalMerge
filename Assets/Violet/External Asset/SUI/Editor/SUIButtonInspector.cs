using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Violet
{
    [CustomEditor(typeof(SUIButton), true)]
    [CanEditMultipleObjects]
    public class SUIButtonInspector : GraphicEditor
    {
        private SerializedProperty _pressType;
        private SerializedProperty _soundType;
        private SerializedProperty _soundExcuteType;
        private SerializedProperty _scaleTarget;
        private SerializedProperty _imgTarget;
        private SerializedProperty _sprTarget;
        private SerializedProperty _colorTarget;
        private SerializedProperty _onClickProp;

        private GUIContent _pressTypeContent;
        private GUIContent _soundTypeContent;
        private GUIContent _soundExcuteTypeContent;
        private GUIContent _spriteContent;
        private GUIContent _imageContent;
        private GUIContent _colorContent;
        private GUIContent _scaleContent;

        private SUIButton _targetButton;

        protected override void OnEnable()
        {
            base.OnEnable();

            _pressTypeContent = new GUIContent("Press Type");
            _soundTypeContent = new GUIContent("Sound Type");
            _soundExcuteTypeContent = new GUIContent("Sound Excute");
            _spriteContent = new GUIContent("Sprite");
            _imageContent = new GUIContent("Target Image");
            _colorContent = new GUIContent("Color");
            _scaleContent = new GUIContent("Scale(Relative)");

            _pressType = serializedObject.FindProperty("pressType");
            _soundType = serializedObject.FindProperty("soundType");
            _soundExcuteType = serializedObject.FindProperty("soundExcuteType");
            _scaleTarget = serializedObject.FindProperty("scaleTarget");
            _imgTarget = serializedObject.FindProperty("imgTarget");
            _sprTarget = serializedObject.FindProperty("sprTarget");
            _colorTarget = serializedObject.FindProperty("colorTarget");
            _onClickProp = serializedObject.FindProperty("onClick");

            _targetButton = target as SUIButton;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_imgTarget, _imageContent);
            if (_targetButton.imgTarget == null)
                _targetButton.imgTarget = _targetButton.GetComponent<Image>();

            EditorGUILayout.PropertyField(_pressType, _pressTypeContent);
            ePRESS_TYPE btnType = (ePRESS_TYPE) _pressType.enumValueIndex;
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
                    EditorGUILayout.Slider(_scaleTarget, -0.5f, 0.5f, _scaleContent);
                    break;
                default:
                    break;
            }

            EditorGUILayout.PropertyField(_soundType, _soundTypeContent);
            _targetButton.soundType = (eSOUND_TYPE) _soundType.enumValueIndex;

            EditorGUILayout.PropertyField(_soundExcuteType, _soundExcuteTypeContent);
            _targetButton.soundExcuteType = (eSOUND_EXCUTE_TYPE) _soundType.enumValueIndex;

            EditorGUILayout.PropertyField(_onClickProp);

            serializedObject.ApplyModifiedProperties();
        }
    }
}