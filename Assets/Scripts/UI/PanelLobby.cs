using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Violet;

public class PanelLobby : SUIPanel
{
    public LobbyPageBase CurrentPage;
    public GameObject Matching;
    public List<LobbyPageBase> Page = new List<LobbyPageBase>();

    public ScrollRect PageScroll;
    public LobbyPageBase PrevPage;

    protected override void OnShow()
    {
        base.OnShow();

        Matching.SetActive(false);
        var pages = GetComponentsInChildren<LobbyPageBase>();
        Page = new List<LobbyPageBase>(pages);
        Page.Sort((a, b) =>
        {
            if (a.Index < b.Index) return -1;
            if (a.Index > b.Index) return 1;
            return 0;
        });
        foreach (var page in Page)
            page.Root.SetActive(false);

        CurrentPage = Page[2];
        CurrentPage.Root.SetActive(true);
        CurrentPage.OnShow();

        RefreshScroll();

        //스테이지 최초 클리어
        if (GameManager.Instance.isUnlockHero)
        {
            GameManager.Instance.isUnlockHero = false;
            UIManager.Instance.ShowPopup<PopupHeroUnlock>();
        }
    }

    private void Update()
    {
        if (CurrentPage != null) CurrentPage.OnUpdate(Time.deltaTime);
    }

    private void MovePage(int index)
    {
        if (CurrentPage.Index == index) return;

        PrevPage = CurrentPage;

        CurrentPage = Page[index];
        CurrentPage.Root.SetActive(true);
        CurrentPage.OnShow();
        RefreshScroll();
    }

    public void RefreshScroll()
    {
        var destination = CurrentPage.Index == 0 ? 0f : (float) CurrentPage.Index / (Page.Count - 1);
        DOTween.To(() => PageScroll.horizontalNormalizedPosition,
                x => PageScroll.horizontalNormalizedPosition = x,
                destination, 0.3f).SetEase(Ease.OutBack)
            .OnComplete(() => { PrevPage.Root.SetActive(false); })
            .Play();
    }

    public void OnClickPageButton(int page)
    {
        PartSimpleNotice.Show("공사중 입니다!");
        return;
        MovePage(page);
    }

    public void OnClickPlayerLevelReward()
    {
        PartSimpleNotice.Show("공사중 입니다!");
        return;
        UIManager.Instance.Show<PanelPlayerLevelReward>();
    }
    #region 매칭 취소

    public void OnClickMatchCancel()
    {
        OnMatchingCancel();
    }

    public void OnMatchingCancel()
    {
        Matching.SetActive(false);
    }

    public void MatchMakingAI()
    {
        OnMatchingCancel();
        GameManager.EnterBattle(true);
    }

    #endregion
    
}