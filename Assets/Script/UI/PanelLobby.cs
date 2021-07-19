using System.Collections;
using BackEnd;
using Define;
using UnityEngine;
using UnityEngine.UI;
using Violet;

public class PanelLobby : SUIPanel
{
    public GameObject Matching;
    public Text RankScore;
    protected override void OnShow()
    {
        base.OnShow();

        var hash = new Hashtable();
        AnimalMergeServer.Instance.RefreshPlayerInfo();

        Matching.SetActive(false);
        RankScore.text = PlayerInfo.Instance.RankScore.ToString();
    }

    public void OnMatchingCancel()
    {
        NetworkManager.Instance.DisconnectGameRoom();
        NetworkManager.Instance.DisconnectIngameServer();
        NetworkManager.Instance.DisconnectMatchServer();

        Matching.SetActive(false);
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