using System;
using UnityEngine;

public class FXController : MonoBehaviour
{
    public float Duration;

    /// <summary>
    ///     애니메이션 사용하는 VFX에서 사용함
    /// </summary>
    public Animation CachedAnimation;

    /// <summary>
    ///     UI VFX에서 사용함
    /// </summary>
    public RectTransform CachedRectTransform;

    public UITracker UITracker;

    protected float _delta;
    private Action<FXController> _onFinish;

    public Action<FXController> OnRestore;

    public void Reset()
    {
        var particle = transform.GetComponentsInChildren<ParticleSystem>();

        var maxDuration = 0f;
        for (var i = 0; i < particle.Length; i++)
        {
            var d = particle[i].main.duration + particle[i].main.startDelayMultiplier;
            if (maxDuration < d)
                maxDuration = d;
        }

        Duration = maxDuration;
    }

    private void Update()
    {
        var delta = Time.deltaTime;
        _delta += delta;
        VFXUpdate(delta);

        if (_delta >= Duration)
            Stop();
    }

    public void SetParnet(Transform parent)
    {
        transform.SetParent(parent);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

    public virtual void Play(Action<FXController> onFinish)
    {
        _onFinish = onFinish;
        _delta = 0f;


        gameObject.SetActive(true);
    }

    public virtual void Play(Vector3 pos, bool isLocal = false)
    {
        if (isLocal)
            transform.localPosition = pos;
        else
            transform.position = pos;

        Play(onFinish: null);
    }

    public virtual void Play(int damage, Action<FXController> onFinish = null)
    {
    }

    public virtual void Play(Vector3 from, Vector3 to, Action<FXController> onFinish = null)
    {
    }

    public virtual void Play(string image)
    {
    }

    public virtual void PlayGetGold(int value)
    {
    }

    public void Stop()
    {
        gameObject.SetActive(false);

        OnRestore?.Invoke(this);
        _onFinish?.Invoke(this);
    }

    protected virtual void VFXUpdate(float delta)
    {
    }
}