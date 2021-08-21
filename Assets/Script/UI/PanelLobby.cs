using System.Collections.Generic;
using Define;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Violet;

public class PanelLobby : SUIPanel
{
    public LobbyPageBase PrevPage;
    public LobbyPageBase CurrentPage;
    public GameObject Matching;
    public List<LobbyPageBase> Page = new List<LobbyPageBase>();

    public ScrollRect PageScroll;
    
    protected override void OnShow()
    {
        base.OnShow();

        Matching.SetActive(false);

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
        float destination = CurrentPage.Index == 0 ? 0f : (float) CurrentPage.Index / (Page.Count - 1);
        DOTween.To(() => PageScroll.horizontalNormalizedPosition,
            x => PageScroll.horizontalNormalizedPosition = x,
            destination, 0.3f).SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                PrevPage.Root.SetActive(false);
            })
            .Play();
    }

    public void OnClickPageButton(int page)
    {
        MovePage(page);
    }

    #region 매칭 취소

    public void OnClickMatchCancel()
    {
        OnMatchingCancel();
    }

    public void OnMatchingCancel()
    {
        NetworkManager.Instance.DisconnectGameRoom();
        NetworkManager.Instance.DisconnectIngameServer();
        NetworkManager.Instance.DisconnectMatchServer();

        Matching.SetActive(false);
    }

    public void MatchMakingAI()
    {
        OnMatchingCancel();
        GameManager.EnterBattle(true);
    }
    #endregion
}