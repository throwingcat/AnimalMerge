using UnityEngine;

public class CanvasLayerTag : MonoBehaviour
{
    [Header("레이어")] public UIManager.eUILayer layer = UIManager.eUILayer.None;

    private RectTransform _cachedRectTransform;

    private CanvasGroup _canvasGroup;

    public RectTransform CachedRectTransform
    {
        get
        {
            if (_cachedRectTransform == null)
                _cachedRectTransform = GetComponent<RectTransform>();
            return _cachedRectTransform;
        }
    }

    public CanvasGroup CachedCanvasGroup
    {
        get
        {
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();
            return _canvasGroup;
        }
    }
}