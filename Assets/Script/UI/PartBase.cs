using DG.Tweening;
using UnityEngine;
using Violet;

public class PartBase : MonoBehaviour
{
    [SerializeField] [Header("트윈 애니메이션")] protected DOTweenAnimation[] _tween;

    public bool isActivate;

    [ContextMenu("트윈 갱신")]
    protected void SetTween()
    {
        _tween = transform.GetComponentsInChildren<DOTweenAnimation>();
    }

    public virtual void Initialize()
    {
    }

    public virtual void OnShow()
    {
        isActivate = true;
        SUIPanel.ToggleTween(_tween, true);
        gameObject.SetActive(true);
    }

    public virtual void OnHide()
    {
        isActivate = false;
        SUIPanel.ToggleTween(_tween, false);

        var delay = 0f;
        for (var i = 0; i < _tween.Length; i++)
        {
            var value = _tween[i].duration + _tween[i].delay;
            if (value > delay)
                delay = value;
        }

        Invoke(nameof(HideForce), delay);
    }

    public void HideForce()
    {
        gameObject.SetActive(false);
    }

    public virtual void OnHideOnlyTween()
    {
        SUIPanel.ToggleTween(_tween, false);
    }
}