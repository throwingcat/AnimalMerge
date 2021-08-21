using System;
using System.Collections;
using System.Collections.Generic;
using BackEnd;
using Define;
using Server;
using UnityEngine;
using UnityEngine.UI;
using Violet;

public class LobbyPageMain : LobbyPageBase
{
    public Text RankScore;

    public PartLobbyChest[] Chests;
    private float _chest_update_delta = 0f;
    private float _chest_update_delay = 1f;

    public override void OnShow()
    {
        base.OnShow();

        RankScore.text = PlayerInfo.Instance.RankScore.ToString();
        foreach (var chest in Chests)
            chest.OnUpdate();
    }

    public override void OnUpdate(float delta)
    {
        base.OnUpdate(delta);

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
                    chest.StartTime = new DateTime();
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

        if (Input.GetKeyDown(KeyCode.T))
        {
            Inventory.Instance.Update("coin", 1);
        }
    }

    #region 게임 시작

    public void OnClickGameStart()
    {
        Backend.Match.OnMatchInGameStart -= OnMatchInGameStart;
        Backend.Match.OnMatchInGameStart += OnMatchInGameStart;

        NetworkManager.Instance.OnMatchingStart();

        if (SUIPanel.CurrentPanel is PanelLobby)
        {
            PanelLobby panel = SUIPanel.CurrentPanel as PanelLobby;
            panel.Matching.SetActive(true);
        }
    }

    private void OnMatchInGameStart()
    {
        GameManager.EnterBattle(false);
    }

    public void OnClickAdventure()
    {
        UIManager.Instance.Show<PanelAdventure>();
    }

    #endregion
}