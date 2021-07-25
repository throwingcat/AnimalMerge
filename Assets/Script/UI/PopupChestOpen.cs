using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using SheetData;
using UnityEngine;
using UnityEngine.UI;
using Violet;

public class PopupChestOpen : SUIPanel
{
    public Image Chest;
    public Text RemainRewardCount;

    public PartUnitCard GetCurrentReward;
    public Text GetRewardAmount;

    public GameObject RewardsRoot;
    public List<PartUnitCard> Rewards = new List<PartUnitCard>();

    private bool _isInitialize = false;
    private Action _inputEvent = null;

    private List<ItemInfo> _rewards = new List<ItemInfo>();

    protected override void OnShow()
    {
        base.OnShow();

        //최초 설정값 
        Chest.rectTransform.anchoredPosition = new Vector2(0, -60);
        Chest.color = Color.white;
        Chest.gameObject.SetActive(false);
        RemainRewardCount.gameObject.gameObject.SetActive(false);

        GetCurrentReward.gameObject.SetActive(false);
        GetRewardAmount.gameObject.SetActive(false);
        RewardsRoot.SetActive(false);
        foreach (var reward in Rewards)
            reward.Root.SetActive(false);

        StartCoroutine(Process());
    }

    protected override void OnHide()
    {
        base.OnHide();

        _isInitialize = false;
    }

    private void OnEnable()
    {
        OnShow();
        _isInitialize = true;
    }

    public void Set(string chest_key, List<ItemInfo> Rewards)
    {
        _rewards = Rewards;

        Chest.sprite = chest_key.ToTableData<Chest>().texture.ToSprite();
        _isInitialize = true;
    }

    public IEnumerator Process()
    {
        while (_isInitialize == false)
            yield return null;

        //상자 등장
        Chest.gameObject.SetActive(true);
        Chest.DOFade(0f, 0.3f).From().Play();

        Chest.transform.DOLocalMoveY(-90f, 0.5f).From(true).SetEase(Ease.OutQuad).Play();
        yield return new WaitForSeconds(0.5f);

        //입력 대기
        bool isDone = false;
        _inputEvent = () => isDone = true;
        yield return new WaitUntil(() => isDone);

        if (_rewards.Count == 0)
        {
            _rewards.Add(new ItemInfo()
            {
                Amount = 5,
            });
            _rewards.Add(new ItemInfo()
            {
                Amount = 20
            });
            _rewards.Add(new ItemInfo()
            {
                Amount = 100,
            });
            _rewards.Add(new ItemInfo()
            {
                Amount = 1000,
            });
            _rewards.Add(new ItemInfo()
            {
                Amount = 1,
            });
        }

        //상자 아래로 이동
        Chest.transform.DOLocalMoveY(-120f, 0.5f).SetRelative(true).SetEase(Ease.OutQuad).Play();

        RemainRewardCount.gameObject.SetActive(true);
        RemainRewardCount.text = _rewards.Count.ToString();

        isDone = false;
        _inputEvent = null;
        _inputEvent = () => isDone = true;
        yield return new Extention.WaitForSecondsOrEvent(0.5f, () => isDone);

        int open_index = 0;
        while (open_index < _rewards.Count)
        {
            DOTween.KillAll();

            var itemInfo = _rewards[open_index];

            open_index++;

            int remain = _rewards.Count - open_index;
            RemainRewardCount.text = remain.ToString();
            Chest.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0), 0.2f, 10, 1f);

            GetCurrentReward.CardImageGroup.transform.localPosition = Vector3.zero;
            GetCurrentReward.CardDescriptionGroup.transform.localPosition = Vector3.zero;

            GetCurrentReward.gameObject.SetActive(true);
            GetCurrentReward.CardImageGroup.gameObject.SetActive(true);
            GetCurrentReward.CardDescriptionGroup.gameObject.SetActive(false);
            GetCurrentReward.CardImageGroup.alpha = 0f;
            GetCurrentReward.CardImageGroup.DOFade(1, 0.3f).Play();
            GetCurrentReward.CardImageGroup.transform.localPosition = new Vector3(150, -141, 0f);
            GetCurrentReward.CardImageGroup.transform.DOLocalMoveY(0, 0.3f).SetEase(Ease.OutQuad).Play();

            GetRewardAmount.gameObject.SetActive(false);

            yield return new WaitForSeconds(0.3f);
            isDone = false;
            _inputEvent = null;
            _inputEvent = () => isDone = true;
            if (isDone)
                continue;
            yield return new WaitForSeconds(0.1f);

            GetCurrentReward.CardImageGroup.transform.localPosition = new Vector3(150, 0, 0);
            GetCurrentReward.CardImageGroup.transform.DOLocalMoveX(0, 0.3f).SetEase(Ease.OutQuad).Play();

            GetCurrentReward.CardDescriptionGroup.gameObject.SetActive(true);
            GetCurrentReward.CardDescriptionGroup.alpha = 0f;
            GetCurrentReward.CardDescriptionGroup.DOFade(1f, 0.2f).SetEase(Ease.OutQuad).Play();
            GetCurrentReward.CardDescriptionGroup.transform.DOLocalMoveY(-0.1f, 0.2f).From().SetEase(Ease.OutQuad)
                .Play();

            int prev_exp = 0;
            int current_exp = prev_exp + itemInfo.Amount;
            GetCurrentReward.SetExp(prev_exp, 20);

            isDone = false;
            _inputEvent = null;
            _inputEvent = () => isDone = true;
            yield return new Extention.WaitForSecondsOrEvent(0.2f, () => isDone);
            _inputEvent = null;

            GetRewardAmount.gameObject.SetActive(true);
            GetRewardAmount.text = string.Format("X{0}", itemInfo.Amount);

            int offset = current_exp - prev_exp;
            float sec = Mathf.Clamp(offset / 50f, 0.25f, 1.5f);
            DOTween.To(() => prev_exp, x =>
            {
                GetCurrentReward.SetExp(x, 10);
            }, current_exp, sec);

            isDone = false;
            _inputEvent = () => isDone = true;
            yield return new WaitUntil(() => isDone);
            _inputEvent = null;
        }

        GetCurrentReward.gameObject.SetActive(false);
        RemainRewardCount.gameObject.SetActive(false);
        Chest.transform.DOLocalMoveY(300f, 0.5f).SetRelative(true);

        yield return new WaitForSeconds(0.3f);
        RewardsRoot.SetActive(true);
        for (int i = 0; i < _rewards.Count; i++)
        {
            Rewards[i].Root.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void OnClickTouchArea()
    {
        _inputEvent?.Invoke();
    }
}