using System.Collections;
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
        Jewel
    }

    private Coroutine _coroutine;
    private int _currentValue = -1;
    private int _prevValue = -1;
    public bool isActive;
    public Slider Slider;
    public SlicedFilledImage SliderFilledImage;

    public eType Type;

    public Text Value;

    public PlayerInfo PlayerInfo = null;
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
            if (PlayerInfo == null)
            {
                if (PlayerDataManager.Instance != null)
                    PlayerInfo = PlayerDataManager.Get<PlayerInfo>();
            }
            if (PlayerInfo != null)
            {
                switch (Type)
                {
                    case eType.PlayerLevel:
                            _currentValue = PlayerInfo.elements.Level;
                        break;
                    case eType.PlayerExp:
                            if (PlayerInfo.isMaxLevel())
                            {
                                _currentValue = 100;
                            }
                            else
                            {
                                var sheet = PlayerInfo.GetLevelSheet();
                                if (sheet != null)
                                {
                                    var max = sheet.exp;
                                    _currentValue =
                                        (int) (Mathf.InverseLerp(0, max, PlayerInfo.elements.Exp) * 100);
                                }
                            }

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
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    public void UpdateValue(int from)
    {
        switch (Type)
        {
            case eType.PlayerLevel:
                Value.text = _currentValue.ToString();
                break;
            case eType.PlayerExp:
            {
                Value.text = string.Format("{0}%", _currentValue);
                if (Slider != null)
                    Slider.value = _currentValue / 100f;
                if (SliderFilledImage != null)
                    SliderFilledImage.fillAmount = _currentValue / 100f;
            }
                break;
            case eType.Coin:
            case eType.Jewel:
            {
                DOTween.To(() => from, x =>
                {
                    if (Value != null)
                        Value.text = Utils.ParseComma(x);
                }, _currentValue, 0.33f);
            }
                break;
        }
    }
}