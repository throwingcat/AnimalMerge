using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Violet;

public class UIManager : MonoSingleton<UIManager>
{
    public enum eUILayer
    {
        None = 0,
        Panel,
        Popup,
        VFX,
    }
    
    public PartCurtain Curtain;

    public bool isPressed;
    private readonly Dictionary<string, SUIPanel> _cachedPanel = new Dictionary<string, SUIPanel>();

    public Dictionary<CanvasTag.eCanvasTag,Canvas> Canvases = new Dictionary<CanvasTag.eCanvasTag, Canvas>();
    public Dictionary<eUILayer, CanvasLayerTag> Layers = new Dictionary<eUILayer, CanvasLayerTag>();
    
    public Action OnPressBackgroundEvent;
    public Action OnReleaseBackgroundEvent;

    public LoadingScreen LoadingScreen;
    
    public void Update()
    {
        var mousePoint = Vector2.zero;
        var phase = TouchPhase.Began;
        if (Utils.GetTouchPhase(out phase))
        {
            if (phase == TouchPhase.Began) isPressed = true;
        }
        else
        {
            isPressed = false;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (1 < SUIPanel.StackCount)
            {
                SUIPanel.CurrentPanel.BackPress();
            }
        }
    }

    public Canvas GetCanvas(CanvasTag.eCanvasTag type)
    {
        if (Canvases == null || Canvases.Count == 0)
        {
            Canvases = new Dictionary<CanvasTag.eCanvasTag, Canvas>();
            var canvases = FindObjectsOfType<CanvasTag>();
            foreach(var canvas in canvases)
                Canvases.Add(canvas.Tag,canvas.Canvas);
        }

        return Canvases[type];
    }
    public CanvasLayerTag GetLayer(eUILayer type)
    {
        if (Layers == null || Layers.Count == 0)
        {
            Layers = new Dictionary<eUILayer, CanvasLayerTag>();
            var layers = FindObjectsOfType<CanvasLayerTag>();
            foreach (var layer in layers)
                Layers.Add(layer.layer, layer);
        }

        return Layers[type];
    }

    private T GetPanel<T>() where T : SUIPanel
    {
        var key = typeof(T).Name;
        return (T) GetPanel(key);
    }

    private SUIPanel GetPanel(string key)
    {
        if (_cachedPanel.ContainsKey(key) == false)
        {
            var isPopup = key.StartsWith("Popup");

            var path = string.Format("UI/{0}", key);

            var prefab = ResourceManager.Instance.LoadPrefab(path);
            var layer = isPopup == false ? GetLayer(eUILayer.Panel) : GetLayer(eUILayer.Popup);
            var panel = Instantiate(prefab, layer.CachedRectTransform);
        
            panel.SetActive(false);

            var component = panel.GetComponent<SUIPanel>();
            var rt = panel.GetComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero;
            _cachedPanel.Add(key, component);
        }

        return _cachedPanel[key];
    }

    public void Show(ePANEL_TYPE type)
    {
        var memberInfo = typeof(ePANEL_TYPE).GetMember(type.ToString()).FirstOrDefault();
        if (memberInfo != null)
        {
            var attribute = (SUIPanelAttribute)
                memberInfo.GetCustomAttributes(typeof(SUIPanelAttribute), false)
                    .FirstOrDefault();
            if (attribute != null)
            {
                var panel = GetPanel(attribute.PanelType.Name);
                if (attribute.isPopup)
                    panel.ShowPopup();
                else
                    panel.Show();
            }
        }
    }

    public T Show<T>() where T : SUIPanel
    {
        var panel = GetPanel<T>();
        panel.Show();
        return panel;
    }

    public T ShowPopup<T>(bool withCurtain = true) where T : SUIPanel
    {
        var panel = GetPanel<T>();
        panel.ShowPopup(withCurtain);

        return panel;
    }
    
    public void ShowCurtain(Transform transform)
    {
        Curtain.Show(transform);
    }

    // 커튼 숨김.
    public void HideCurtain()
    {
        Curtain.Hide();
    }

    public void Close()
    {
        SUIPanel.BackPressForce();
    }

    public void OnPressBackground()
    {
        OnPressBackgroundEvent?.Invoke();
    }

    public void OnReleaseBackground()
    {
        OnReleaseBackgroundEvent?.Invoke();
    }

    public void SetActiveLayer(eUILayer type, bool isActive)
    {
        var layer = GetLayer(type);

        var canvasGroup = layer.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            layer.gameObject.SetActive(isActive);
        }
        else
        {
            var from = isActive ? 0f : 1f;
            var to = isActive ? 1f : 0f;
            DOTween.To(() => from, value => { canvasGroup.alpha = value; }, to, 1f);
        }

        if (layer.CachedCanvasGroup != null)
            layer.CachedCanvasGroup.blocksRaycasts = isActive;
    }
}