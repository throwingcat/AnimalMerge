using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Violet;

public class PanelTitle : SUIPanel
{
    public Slider slider;
    public RectTransform loadingGauge;
    public RectTransform guide;
    protected override void OnShow()
    {
        base.OnShow();
        slider.value = 0f;
    }

    public void RefreshGauge(float t)
    {
        DOTween.To(() => slider.value, x =>
        {
            slider.value = x;
            
            var pos = Mathf.Lerp(0, loadingGauge.rect.width, x);
            guide.anchoredPosition= new Vector2(
                loadingGauge.rect.x + pos,
                guide.anchoredPosition.y);
            
        }, t, 0.33f);

        
    }
}