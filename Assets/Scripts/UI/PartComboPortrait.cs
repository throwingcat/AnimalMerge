using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using Violet;

public class PartComboPortrait : MonoBehaviour
{
    public RectTransform RectTransform;
    public GameObject Root;

    public GameObject PlayingObject;
    
    public DOTweenAnimation[] Tweens;

    private bool isPlaying = false;
    private float Elapsed = 0f;
    private float Duration = 2f;

    private System.Action _onFinish;

    public void Enter()
    {
    }

    public void Leave()
    {
    }

    public void Play(int combo, System.Action onFinish)
    {
        if (combo < 3) return;

        _onFinish = onFinish;

        foreach (var t in Tweens)
        {
            t.DORewind();
            t.DOPlayForward();
        }

        Root.SetActive(false);

        combo = Mathf.Clamp(combo, 3, 3);
        string key = string.Format("Combo_Portrait_{0}", combo);
        var pool = GameObjectPool.GetPool(key);
        if (pool == null)
        {
            pool = GameObjectPool.CreatePool(key, () =>
            {
                var prefab = ResourceManager.Instance.LoadPrefab(string.Format("UI/ComboPortrait/{0}", key));
                if (prefab != null)
                {
                    var go = Instantiate(prefab);
                    go.name = key;
                    go.transform.LocalReset();
                    go.gameObject.SetActive(false);

                    return go;
                }

                return null;
            }, 1, category: Define.Key.UIVFXPoolCategory);
        }

        if (PlayingObject != null)
        {
            var prev_pool = GameObjectPool.GetPool(PlayingObject.name);
            prev_pool.Restore(PlayingObject);
        }

        PlayingObject = pool.Get();
        var rt = PlayingObject.GetComponent<RectTransform>();
        rt.SetParent(Root.transform);
        rt.LocalReset();
        PlayingObject.gameObject.SetActive(true);

        isPlaying = true;
        Elapsed = 0f;

        Root.SetActive(true);
    }

    public void Update()
    {
        if (isPlaying)
        {
            if (Duration <= Elapsed)
            {
                Stop();
            }

            Elapsed += Time.deltaTime;
        }
    }

    public void Stop()
    {
        Elapsed = 0f;
        isPlaying = false;
        foreach (var t in Tweens)
            t.DOPlayBackwards();
        GameManager.DelayInvoke(Exit, 0.5f);
    }

    private void Exit()
    {
        if (isPlaying == false)
        {
            Root.SetActive(false);
            _onFinish?.Invoke();
        }
    }
}