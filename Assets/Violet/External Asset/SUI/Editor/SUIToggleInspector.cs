using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Violet
{
    [CustomEditor(typeof(SUIToggle), true)]
    [CanEditMultipleObjects]
    public class SUIToggleInspector : GraphicEditor
    {
        private SerializedProperty _pressType;
        private SerializedProperty _goActive;
        private SerializedProperty _goDeactive;
        private SerializedProperty _textTarget;
        private SerializedProperty _scaleTarget;
        private SerializedProperty _scaleRectTarget;
        private SerializedProperty _imgTarget;
        private SerializedProperty _sprTarget;
        private SerializedProperty _colorTarget;
        private SerializedProperty _onClickProp;

        private SerializedProperty _activeType;
        private SerializedProperty _imgActive;
        private SerializedProperty _sprActive;
        private SerializedProperty _colorActive;

        private GUIContent _activeTypeContent;
        private GUIContent _pressTypeContent;
        private GUIContent _goActiveContent;
        private GUIContent _goDeactiveContent;
        private GUIContent _textContent;
        private GUIContent _spriteContent;
        private GUIContent _imageContent;
        private GUIContent _colorContent;
        private GUIContent _scaleContent;
        private GUIContent _rectContent;
        private GUIContent _imgActiveContent;
        private GUIContent _sprActiveContent;
        private GUIContent _colorActiveContent;

        private SUIToggle _targetToggle;

        protected override void OnEnable()
        {
            base.OnEnable();

            _activeTypeContent = new GUIContent("Active Type");
            _pressTypeContent = new GUIContent("Press Type");
            _textContent = new GUIContent("Target Text");
            _goActiveContent = new GUIContent("Active Object");
            _goDeactiveContent = new GUIContent("Deactive Object");
            _spriteContent = new GUIContent("Sprite");
            _imageContent = new GUIContent("Target Image");
            _colorContent = new GUIContent("Color");
            _scaleContent = new GUIContent("Scale");
            _rectContent = new GUIContent("Target Rect");
            _imgActiveContent = new GUIContent("Active Image");
            _sprActiveContent = new GUIContent("Active Sprite");
            _colorActiveContent = new GUIContent("Active Color");

            _activeType = serializedObject.FindProperty("activeType");
            _pressType = serializedObject.FindProperty("pressType");
            _textTarget = serializedObject.FindProperty("text");
            _goActive = serializedObject.FindProperty("goActive");
            _goDeactive = serializedObject.FindProperty("goDeactive");
            _scaleTarget = serializedObject.FindProperty("scaleTarget");
            _imgTarget = serializedObject.FindProperty("imgTarget");
            _sprTarget = serializedObject.FindProperty("sprTarget");
            _scaleRectTarget = serializedObject.FindProperty("rectTarget");
            _colorTarget = serializedObject.FindProperty("colorTarget");
            _onClickProp = serializedObject.FindProperty("onValueChange");
            _imgActive = serializedObject.FindProperty("imgActive");
            _sprActive = serializedObject.FindProperty("sprActive");
            _colorActive = serializedObject.FindProperty("colorActive");

            _targetToggle = target as SUIToggle;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_activeType, _activeTypeContent);
            eTOGGLE_ACTIVE_TYPE activeType = (eTOGGLE_ACTIVE_TYPE)_activeType.enumValueIndex;
            switch (activeType)
            {
                case eTOGGLE_ACTIVE_TYPE.Color:
                    EditorGUILayout.PropertyField(_imgActive, _imgActiveContent);
                    EditorGUILayout.PropertyField(_colorActive, _colorActiveContent);
                    break;
                case eTOGGLE_ACTIVE_TYPE.Sprite:
                    EditorGUILayout.PropertyField(_imgActive, _imgActiveContent);
                    EditorGUILayout.PropertyField(_sprActive, _sprActiveContent);
                    break;
                case eTOGGLE_ACTIVE_TYPE.Check:
                    EditorGUILayout.PropertyField(_goActive, _goActiveContent);
                    break;
                case eTOGGLE_ACTIVE_TYPE.ToggleGameObject:
                    EditorGUILayout.PropertyField(_goActive, _goActiveContent);
                    EditorGUILayout.PropertyField(_goDeactive, _goDeactiveContent);
                    break;
            }

            if (_targetToggle.imgTarget == null)
                _targetToggle.imgTarget = _targetToggle.GetComponent<Image>();

            EditorGUILayout.PropertyField(_pressType, _pressTypeContent);

            ePRESS_TYPE btnType = (ePRESS_TYPE)_pressType.enumValueIndex;
            switch (btnType)
            {
                case ePRESS_TYPE.Color:
                    EditorGUILayout.PropertyField(_imgTarget, _imageContent);
                    EditorGUILayout.PropertyField(_colorTarget, _colorContent);
                    break;
                case ePRESS_TYPE.Sprite:
                    EditorGUILayout.PropertyField(_imgTarget, _imageContent);
                    EditorGUILayout.PropertyField(_sprTarget, _spriteContent);
                    if (_targetToggle.sprTarget == null && _targetToggle.imgTarget != null)
                        _targetToggle.sprTarget = _targetToggle.imgTarget.sprite;
                    break;
                case ePRESS_TYPE.Scale:
                    EditorGUILayout.PropertyField(_scaleTarget, _scaleContent);
                    EditorGUILayout.PropertyField(_scaleRectTarget, _rectContent);
                    break;
                default:
                    break;
            }

            EditorGUILayout.PropertyField(_onClickProp);
            EditorGUILayout.PropertyField(_textTarget, _textContent);

            serializedObject.ApplyModifiedProperties();
        }
    }
}