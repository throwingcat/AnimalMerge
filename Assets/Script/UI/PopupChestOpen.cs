using System;
using System.Collections;
using System.Collections.Generic;
using Define;
using DG.Tweening;
using SheetData;
using UnityEngine;
using UnityEngine.UI;
using Violet;

public class PopupChestOpen : SUIPanel
{
    public Image Chest;
    public Text RemainRewardCount;

    public PartItemCard GetCurrentReward;

    public GameObject RewardsRoot;
    public List<PartItemCard> Rewards = new List<PartItemCard>();

    private bool _isInitialize = false;
    private Action _inputEvent = null;

    private List<ItemInfo> _rewards = new List<ItemInfo>();

    protected override void OnShow()
    {
        base.OnShow();

        IgnoreBackPress = true;
        
        //최초 설정값 
        Chest.rectTransform.anchoredPosition = new Vector2(0, -60);
        Chest.color = Color.white;
        Chest.gameObject.SetActive(false);
        RemainRewardCount.gameObject.gameObject.SetActive(false);

        GetCurrentReward.gameObject.SetActive(false);
        
        RewardsRoot.SetActive(false);
        foreach (var reward in Rewards)
            reward.Root.SetActive(false);

        StartCoroutine(Process());
    }

    protected override void OnHide()
    {
        base.OnHide();

        _isInitialize = false;
        _inputEvent = null;
        IgnoreBackPress = false;
    }

    public void Set(string chest_key, List<ItemInfo> Rewards)
    {
        _rewards = Rewards;

        var sheet = chest_key.ToTableData<Chest>();
        if (sheet != null)
            Chest.sprite = sheet.texture.ToSprite();
        _isInitialize = true;
    }

    public IEnumerator Process()
    {
        while (_isInitialize == false)
            yield return null;

        //상자 등장
        Chest.gameObject.SetActive(true);
        Chest.transform.localPosition =new Vector3(0,-200,0);
        Chest.DOFade(0f, 0.3f).From().Play();
        Chest.transform.DOLocalMoveY(-70f, 0.5f).From(true).SetEase(Ease.OutQuad).Play();
        yield return new WaitForSeconds(0.5f);

        //입력 대기
        bool isDone = false;
        _inputEvent = () => isDone = true;
        yield return new WaitUntil(() => isDone);
        
        //상자 아래로 이동
        Chest.transform.DOLocalMoveY(-240f, 0.5f).SetRelative(true).SetEase(Ease.OutQuad).Play();

        RemainRewardCount.gameObject.SetActive(true);
        RemainRewardCount.text = _rewards.Count.ToString();

        isDone = false;
        _inputEvent = null;
        _inputEvent = () => isDone = true;
        yield return new Extention.WaitForSecondsOrEvent(0.5f, () => isDone);

        List<Tweener> used_tweener = new List<Tweener>();
        int open_index = 0;
        while (open_index < _rewards.Count)
        {
            foreach (var t in used_tweener)
                t.Kill();
            used_tweener.Clear();
            
            var reward = _rewards[open_index];

            GetCurrentReward.Set(reward);
            Rewards[open_index].Set(reward);
            Rewards[open_index].SetAmount(reward.Amount);
            open_index++;

            int remain = _rewards.Count - open_index;
            RemainRewardCount.text = remain.ToString();
            used_tweener.Add( Chest.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0), 0.2f, 10, 1f));

            GetCurrentReward.CardImageGroup.transform.localPosition = Vector3.zero;
            GetCurrentReward.CardDescriptionGroup.transform.localPosition = Vector3.zero;

            GetCurrentReward.gameObject.SetActive(true);
            GetCurrentReward.CardImageGroup.gameObject.SetActive(true);
            GetCurrentReward.CardDescriptionGroup.gameObject.SetActive(false);
            GetCurrentReward.CardImageGroup.alpha = 0f;
            used_tweener.Add(GetCurrentReward.CardImageGroup.DOFade(1, 0.3f).Play());
            GetCurrentReward.CardImageGroup.transform.localPosition = new Vector3(150, -141, 0f);
            used_tweener.Add(GetCurrentReward.CardImageGroup.transform.DOLocalMoveY(0, 0.3f).SetEase(Ease.OutQuad).Play());

            GetCurrentReward.Amount.gameObject.SetActive(false);

            
            //경험치 설정
            int prev_exp = 0;
            int current_exp = 0;
            int max_exp = 0;
            if (reward.Type == eItemType.Card)
            {
                var unit = UnitInventory.Instance.GetUnit(reward.Key);
                if (unit != null)
                {
                    current_exp = unit.Exp;
                    prev_exp = current_exp - reward.Amount;

                    if (unit.IsMaxLevel())
                    {
                        max_exp = 0;
                    }
                    else
                    {
                        max_exp = unit.GetCurrentLevelUpExp();
                    }
                    GetCurrentReward.SetExp(prev_exp, max_exp);
                }
            }
            
            yield return new WaitForSeconds(0.3f);
            isDone = false;
            _inputEvent = null;
            _inputEvent = () => isDone = true;
            if (isDone)
                continue;
            yield return new WaitForSeconds(0.1f);

            GetCurrentReward.CardImageGroup.transform.localPosition = new Vector3(150, 0, 0);
            used_tweener.Add(GetCurrentReward.CardImageGroup.transform.DOLocalMoveX(0, 0.3f).SetEase(Ease.OutQuad).Play());

            GetCurrentReward.CardDescriptionGroup.gameObject.SetActive(true);
            GetCurrentReward.CardDescriptionGroup.alpha = 0f;
            used_tweener.Add(GetCurrentReward.CardDescriptionGroup.DOFade(1f, 0.2f).SetEase(Ease.OutQuad).Play());
            used_tweener.Add(GetCurrentReward.CardDescriptionGroup.transform.DOLocalMoveY(-0.1f, 0.2f).From().SetEase(Ease.OutQuad).Play());

            GetCurrentReward.SetAmount(reward.Amount);
            
            isDone = false;
            _inputEvent = null;
            _inputEvent = () => isDone = true;
            yield return new Extention.WaitForSecondsOrEvent(0.2f, () => isDone);
            _inputEvent = null;

            
            
            int offset = current_exp - prev_exp;
            float sec = Mathf.Clamp(offset / 50f, 0.25f, 1.5f);
            used_tweener.Add(DOTween.To(() => prev_exp, x =>
            {
                GetCurrentReward.SetExp(x, max_exp);
            }, current_exp, sec));

            isDone = false;
            _inputEvent = () => isDone = true;
            yield return new WaitUntil(() => isDone);
            _inputEvent = null;
        }

        GetCurrentReward.gameObject.SetActive(false);
        RemainRewardCount.gameObject.SetActive(false);
        Chest.transform.DOLocalMoveY(500f, 0.5f).SetRelative(true);

        yield return new WaitForSeconds(0.3f);
        RewardsRoot.SetActive(true);
        for (int i = 0; i < _rewards.Count; i++)
        {
            Rewards[i].Root.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(0.5f);
        
        IgnoreBackPress = false;
        
        _inputEvent = null;
        _inputEvent = () => { BackPress(); };
    }

    public void OnClickTouchArea()
    {
        _inputEvent?.Invoke();
    }
}