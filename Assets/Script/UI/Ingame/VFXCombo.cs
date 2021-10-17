using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VFXCombo : MonoBehaviour
{
    public RectTransform RectTransform;
    public TextMeshProUGUI ComboCount;
    public Image ComboText;
    private float _duration = 0f;
    private float _delta = 0f;
    private Coroutine _coroutine;

    private ulong _restoreID = 0;

    public void Set(int combo)
    {
        ComboCount.text = string.Format("x {0}", combo);
    }

    public void Play(float duration)
    {
        gameObject.SetActive(false);
        _duration = duration;
        _delta = 0f;
        gameObject.SetActive(true);

        GameManager.DelayInvokeCancel(_restoreID);
        _restoreID = GameManager.DelayInvoke(() => { Restore(); }, 2f);

        if (_coroutine != null)
            StopCoroutine(_coroutine);
        _coroutine = null;
        _coroutine = StartCoroutine(PlayProcess());
    }

    private IEnumerator PlayProcess()
    {
        while (_delta < _duration)
        {
            _delta += Time.deltaTime;
            ComboText.fillAmount = (_duration - _delta) / _duration;
            yield return null;
        }
        _coroutine = null;
    }

    private void Restore()
    {
        return;
        var pool = GameObjectPool.GetPool("VFXCombo");
        if (pool != null)
            pool.Restore(gameObject);
    }
}