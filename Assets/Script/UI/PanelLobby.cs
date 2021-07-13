using System.Collections;
using BackEnd;
using BackEnd.Tcp;
using Define;
using UnityEngine;
using UnityEngine.UI;
using Violet;

public class PanelLobby : SUIPanel
{
    public GameObject Matching;
    public Text MatchingState;

    protected override void OnShow()
    {
        base.OnShow();
        
        Matching.SetActive(false);
        
    }

    public void OnMatchingCancel()
    {
        Backend.Match.LeaveMatchMakingServer();
        Backend.Match.OnLeaveMatchMakingServer -= OnLeaveMatchMakingServer;
        Backend.Match.OnLeaveMatchMakingServer += OnLeaveMatchMakingServer;

        Backend.Match.Poll();
    }

    private void OnLeaveMatchMakingServer(LeaveChannelEventArgs args)
    {
        Matching.SetActive(false);
    }

    public void Update()
    {
        if (Matching.activeSelf)
        {
            MatchingState.text = NetworkManager.Instance.MatchingStep.ToString();
        }
    }
    public void OnClickGameStart()
    {
        Backend.Match.OnMatchInGameStart -= OnMatchInGameStart;
        Backend.Match.OnMatchInGameStart += OnMatchInGameStart;
        
        NetworkManager.Instance.OnMatchingStart();
        Matching.SetActive(true);
    }

    public void OnClickMatchCancel()
    {
        OnMatchingCancel();
    }

    #region 게임 시작
    private void OnMatchInGameStart()
    {
        GameManager.Instance.ChangeGameState(eGAME_STATE.Battle);
    }
    #endregion
}