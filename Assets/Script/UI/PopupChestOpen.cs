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

    public GameObject RewardsRoot;
    public List<PartUnitCard> Rewards = new List<PartUnitCard>();

    private bool _isInitialize = false;
    private Action _inputEvent = null;

    private List<ItemInfo> _rewards = new List<ItemInfo>();

    protected override void OnShow()
    {
        base.OnShow();

        Chest.gameObject.SetActive(false);
        RemainRewardCount.gameObject.gameObject.SetActive(false);

        GetCurrentReward.gameObject.SetActive(false);
        RewardsRoot.SetActive(false);
        foreach (var reward in Rewards)
            reward.gameObject.SetActive(false);

        StartCoroutine(Process());
    }

    protected override void OnHide()
    {
        base.OnHide();

        _isInitialize = false;
    }

    public void Set(string chest_key, List<ItemInfo> Rewards)
    {
        _rewards = Rewards;

        Chest.sprite = chest_key.ToTableData<Chest>().texture.ToSprite();
        _isInitialize = true;
    }

    private void OnEnable()
    {
        _isInitialize = true;
        StartCoroutine(Process());
    }

    public IEnumerator Process()
    {
        while (_isInitialize == false)
            yield return null;

        //최초 설정값 
        Chest.rectTransform.anchoredPosition = new Vector2(0, -60);
        Chest.color = Color.white;

        //상자 등장
        Chest.gameObject.SetActive(true);
        Chest.DOFade(0f, 0.3f).From().Play();
        Chest.transform.DOLocalMoveY(-90f, 0.5f).From(true).SetEase(Ease.OutQuad).Play();

        yield return new WaitForSeconds(0.5f);

        //입력 대기
        bool isDone = false;
        _inputEvent = () => isDone = true;

        while (isDone == false)
            yield return null;
        isDone = false;
        _inputEvent = null;

        //상자 아래로 이동
        Chest.transform.DOLocalMoveY(-120f, 0.5f).SetRelative(true).SetEase(Ease.OutQuad).Play();
        yield return new WaitForSeconds(0.5f);

        if (_rewards.Count == 0)
        {
            _rewards.Add(new ItemInfo());
            _rewards.Add(new ItemInfo());
            _rewards.Add(new ItemInfo());
            _rewards.Add(new ItemInfo());
            _rewards.Add(new ItemInfo());
        }

        int open_index = 0;
        while (open_index < _rewards.Count)
        {
            open_index++;

            Chest.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0), 0.2f, 10, 1f);

            GetCurrentReward.gameObject.SetActive(true);
            GetCurrentReward.CardImageGroup.gameObject.SetActive(true);
            GetCurrentReward.CardDescriptionGroup.gameObject.SetActive(false);
            GetCurrentReward.CardImageGroup.alpha = 0f;
            GetCurrentReward.CardImageGroup.DOFade(1, 0.3f).Play();
            GetCurrentReward.CardImageGroup.transform.localPosition = new Vector3(150, -141, 0f);
            GetCurrentReward.CardImageGroup.transform.DOLocalMoveY(0, 0.3f).SetEase(Ease.OutQuad).Play();

            yield return new WaitForSeconds(0.5f);

            GetCurrentReward.CardImageGroup.transform.DOLocalMoveX(0, 0.3f).SetEase(Ease.OutQuad).Play();
            GetCurrentReward.CardDescriptionGroup.alpha = 0f;
            GetCurrentReward.CardDescriptionGroup.DOFade(1f, 0.2f).SetEase(Ease.OutQuad).Play();
            GetCurrentReward.CardDescriptionGroup.transform.DOLocalMoveY(-0.1f, 0.2f).From().SetEase(Ease.OutQuad)
                .Play();
            yield return new WaitForSeconds(0.5f);
            
            _inputEvent = () => isDone = true;

            while (isDone == false)
                yield return null;
            isDone = false;
            _inputEvent = null;
        }
    }

    public void OnClickTouchArea()
    {
        _inputEvent?.Invoke();
    }
}