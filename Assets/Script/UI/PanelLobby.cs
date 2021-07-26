using System;
using System.Collections;
using BackEnd;
using Define;
using Packet;
using Server;
using UnityEngine;
using UnityEngine.UI;
using Violet;

public class PanelLobby : SUIPanel
{
    public GameObject Matching;
    public Text RankScore;

    public PartLobbyChest[] Chests;
    private float _chest_update_delta = 0f;
    private float _chest_update_delay = 0.5f;
    protected override void OnShow()
    {
        base.OnShow();

        Matching.SetActive(false);
        
        RankScore.text = PlayerInfo.Instance.RankScore.ToString();
        foreach (var chest in Chests)
            chest.OnUpdate();
    }

    private void Update()
    {
        _chest_update_delta += Time.deltaTime;
        if (_chest_update_delay <= _chest_update_delta)
        {
            foreach (var chest in Chests)
                chest.OnUpdate();
            _chest_update_delta = 0f;
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            AnimalMergeServer.Instance.BattleWinProcess(() =>
            {
                foreach (var chest in Chests)
                    chest.OnUpdate();
            });
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            foreach (var chest in ChestInventory.Instance.ChestSlots)
            {
                if (chest.inDate.IsNullOrEmpty() == false && chest.Key.IsNullOrEmpty() == false)
                {
                    chest.StartTime  = new DateTime();
                    chest.isChanged = true;
                    AnimalMergeServer.Instance.UpdateDB<DBChestInventory>(() =>
                    {
                        foreach (var chest in Chests)
                            chest.OnUpdate();
                    });
                    break;
                }
            }
        }
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