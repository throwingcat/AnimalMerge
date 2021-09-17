using System.Collections.Generic;
using Common;
using Define;
using Packet;
using UnityEngine;

public class LobbyPageQuest : LobbyPageBase
{
    public SlicedFilledImage PointSlider;
    public List<CellQeustInfo> Cells = new List<CellQeustInfo>();
    public List<PartQuestDailyReward> DailyRewards = new List<PartQuestDailyReward>();

    public override void OnShow()
    {
        base.OnShow();

        RefreshQuestList();
    }

    public void RefreshDailyReward()
    {
        for (int i = 0; i < DailyRewards.Count; i++)
        {
            PartQuestDailyReward.eState state = PartQuestDailyReward.eState.Disable;
            if (QuestInfo.Instance.ReceiveReward[i] == false)
            {
                if (QuestInfo.Instance.RewardPoint[i] <= QuestInfo.Instance.QuestPoint)
                    state = PartQuestDailyReward.eState.Enable;
            }
            else
            {
                state = PartQuestDailyReward.eState.Clear;
            }

            DailyRewards[i].Set(this, i, state);
        }
    }

    public void RefreshQuestList()
    {
        var index = 0;
        foreach (var slot in QuestInfo.Instance.QuestSlots)
        {
            if (index < Cells.Count)
                Cells[index].Set(this, slot);
            index++;
        }

        var maxPoint = QuestInfo.Instance.RewardPoint[QuestInfo.Instance.RewardPoint.Count - 1];
        PointSlider.fillAmount = QuestInfo.Instance.QuestPoint / (float) maxPoint;

        RefreshDailyReward();
    }

    public void QuestRefresh(int index)
    {
        PacketBase packet = new PacketBase();
        packet.PacketType = ePACKET_TYPE.QUEST_REFRESH;
        packet.hash.Add("slot_index", index);
        NetworkManager.Instance.Request(packet,
            (res) => { Cells[index].Set(this, QuestInfo.Instance.QuestSlots[index]); });
    }

    public void QuestComplete(int index)
    {
        PacketBase packet = new PacketBase();
        packet.PacketType = ePACKET_TYPE.QUEST_COMPLETE;
        packet.hash.Add("slot_index", index);
        NetworkManager.Instance.Request(packet,
            (res) =>
            {
                var p = res as PacketReward;

                if (0 < p.Rewards.Count)
                {
                    var popup = UIManager.Instance.ShowPopup<PopupGetReward>();
                    popup.Set(p.Rewards);
                }

                RefreshQuestList();
            });
    }

    public void ReceiveDailyReward(int index)
    {
        //포인트 미달성
        if (QuestInfo.Instance.QuestPoint < QuestInfo.Instance.RewardPoint[index]) return;
        //이미 받음
        if (QuestInfo.Instance.ReceiveReward[index]) return;
        
        PacketBase packet = new PacketBase();
        packet.PacketType = ePACKET_TYPE.DAILY_QUEST_REWARD;
        packet.hash.Add("index", index);
        NetworkManager.Instance.Request(packet,
            (res) =>
            {
                var p = res as PacketReward;

                if (0 < p.Rewards.Count)
                {
                    var popup = UIManager.Instance.ShowPopup<PopupGetReward>();
                    popup.Set(p.Rewards);
                }

                RefreshDailyReward();
            });
    }
}