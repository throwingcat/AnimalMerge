using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PartSimpleValueUpdate : MonoBehaviour
{
    public enum eType
    {
        PlayerLevel,
        PlayerExp,
        Coin,
        Jewel,
    }

    public eType Type;
    private int _prevValue = -1;
    private int _currentValue = -1;

    public Text Value;
    public Slider Slider;
    public bool isActive = false;
    private Coroutine _coroutine;

    private void OnEnable()
    {
        isActive = true;
        if (_coroutine != null)
            StopCoroutine(_coroutine);
        _coroutine = StartCoroutine(UpdateProcess());
    }

    private void OnDisable()
    {
        isActive = false;
        if (_coroutine != null)
            StopCoroutine(_coroutine);
        _coroutine = null;
    }

    private IEnumerator UpdateProcess()
    {
        while (isActive)
        {
            switch (Type)
            {
                case eType.PlayerLevel:
                    if (PlayerInfo.Instance != null)
                        _currentValue = PlayerInfo.Instance.Level;
                    break;
                case eType.PlayerExp:
                    break;
                case eType.Coin:
                    if (Inventory.Instance != null)
                        _currentValue = Inventory.Instance.GetAmount("Coin");
                    break;
                case eType.Jewel:
                    if (Inventory.Instance != null)
                        _currentValue = Inventory.Instance.GetAmount("Jewel");
                    break;
            }

            if (_prevValue != _currentValue)
            {
                UpdateValue(_prevValue);
                _prevValue = _currentValue;
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    public void UpdateValue(int from)
    {
        DOTween.To(() => from, (x) =>
        {
            if (Slider != null)
            {
                if (Type == eType.PlayerExp)
                    Slider.value = x / 100f;
            }

            if (Value != null)
            {
                switch (Type)
                {
                    case eType.PlayerLevel:
                        Value.text = x.ToString();
                        break;
                    case eType.PlayerExp:
                        Value.text = string.Format("{0]%", x);
                        break;
                    case eType.Coin:
                    case eType.Jewel:
                        Value.text = Utils.ParseComma(x);
                        break;
                }
            }
        }, _currentValue, 0.33f);
    }
}