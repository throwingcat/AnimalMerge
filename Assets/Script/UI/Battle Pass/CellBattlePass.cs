using System.Collections;
using System.Collections.Generic;
using Common;
using Packet;
using SheetData;
using UnityEngine;
using UnityEngine.UI;
using Violet;

public class CellBattlePass : MonoBehaviour , IScrollCell
{
    public SimpleRewardView PassReward;
    public List<SimpleRewardView> PremiumRewards;
    public GameObject PremiumPassLock;
    public Text Score;
    public Slider ScoreSlider;
    public RectTransform ScliderRectTransform;
    public BattlePass BattlePass;
    
    public GameObject ReceivePassReward;
    public GameObject ReceivePremiumPassReward;
    public GameObject ReceiveRewardButton;
    
    public class Data : IScrollData
    {
        public BattlePass Pass;
        public int PrevPoint;
        public int NextPoint;
        public int PlayerPoint;
        public bool isLock;
    }
    
    private  CellBattlePass SetPass(BattlePass pass)
    {
        BattlePass = pass;
        PassReward.Set(pass.PassReward);
        for (int i = 0; i < PremiumRewards.Count; i++)
            PremiumRewards[i].Set(pass.PremiumRewards[i]);
        Score.text = pass.point.ToString();
        return this;
    }

    private  CellBattlePass SetLock(bool isLock)
    {
        PremiumPassLock.SetActive(isLock);
        return this;
    }

    private  CellBattlePass SetPlayerPoint(int prev, int next, int score)
    {
        //이전 점수 절반 ~ 현재 점수 까지가 0.5
        int from = 0;
        int to = 0;
        
        if (prev != 0)
        {
            int range = (int) (Mathf.Abs(prev - BattlePass.point) * 0.5f);
            from = prev + range;
        }

        if (BattlePass.point < next)
        {
            //현재 점수 ~ 다음 점수 절반 까지가 1 
            int range = (int) (Mathf.Abs(next - BattlePass.point) * 0.5f);
            to = next - range;
            ScliderRectTransform.sizeDelta = new Vector2(ScliderRectTransform.sizeDelta.x,250f);
        }
        //마지막 셀
        else
        {
            to = BattlePass.point;
            ScliderRectTransform.sizeDelta = new Vector2(ScliderRectTransform.sizeDelta.x,125f);
        }


        var t = Mathf.InverseLerp(from, to, score);
        ScoreSlider.value = t;
        return this;
    }

    private void UpdateRewardReceive()
    {
        ReceivePassReward.SetActive(false);
        ReceivePremiumPassReward.SetActive(false);
        
        var info = BattlePassInfo.Instance.SeasonPassRewardInfo;
        if (info.ContainsKey(BattlePass.key))
        {
            if (info[BattlePass.key] != null)
            {
                if(info[BattlePass.key].isReceivedPassReward)
                    ReceivePassReward.SetActive(true);
                if(info[BattlePass.key].isReceivedPremiumReward)
                    ReceivePremiumPassReward.SetActive(true);
            }
        }

        if (BattlePass.point <= BattlePassInfo.Instance.Point)
        {
            bool isEnable = ReceivePassReward.activeSelf == false || ReceivePremiumPassReward.activeSelf == false;
            ReceiveRewardButton.SetActive(isEnable);
        }
        else
        {
            ReceiveRewardButton.SetActive(false);    
        }
    }
    
    public void UpdateCell(IScrollData data)
    {
        var cellData = data as Data;

        SetPass(cellData.Pass)
            .SetLock(cellData.isLock)
            .SetPlayerPoint(cellData.PrevPoint, cellData.NextPoint, cellData.PlayerPoint);
        UpdateRewardReceive();
    }

    public void OnClickReceiveReward()
    {
        if (BattlePass.point <= BattlePassInfo.Instance.Point)
        {
            if (BattlePassInfo.Instance.HasReward(BattlePass.key) == false) return;
            
            var packet = new PacketBase();
            packet.PacketType = ePACKET_TYPE.RECEIVE_PASS_REWARD;
            packet.hash["pass_key"] = BattlePass.key;
            NetworkManager.Instance.Request(packet, (res) =>
            {
                if (res.isSuccess())
                {
                    UpdateRewardReceive();
                }
            });
        }
    }
}