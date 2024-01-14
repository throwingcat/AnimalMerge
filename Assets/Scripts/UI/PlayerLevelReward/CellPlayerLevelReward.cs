using Common;
using SheetData;
using UnityEngine;
using UnityEngine.UI;
using Violet;

public class CellPlayerLevelReward : MonoBehaviour, IScrollCell
{
    public Text Level;
    public Slider LevelSlider;
    public SimpleRewardView PremiumReward;
    public GameObject PremiumRewardLock;
    public GameObject ReceivePremiumReward;

    public GameObject ReceiveReward;
    public GameObject ReceiveRewardButton;
    public SimpleRewardView Reward;
    public RectTransform ScliderRectTransform;
    public PlayerLevel Sheet;

    public void UpdateCell(IScrollData data)
    {
        var cellData = data as Data;
        Set(cellData.Sheet)
            .SetLock(cellData.isLock)
            .SetPlayerPoint(cellData.PrevLevel, cellData.NextLevel, cellData.Level);
        UpdateRewardReceive();
    }

    private CellPlayerLevelReward Set(PlayerLevel sheet)
    {
        Sheet = sheet;
        Reward.Set(Sheet.Reward);
        PremiumReward.Set(Sheet.PremiumReward);
        Level.text = Sheet.level.ToString();
        return this;
    }

    private CellPlayerLevelReward SetLock(bool isLock)
    {
        PremiumRewardLock.SetActive(isLock);
        return this;
    }

    private CellPlayerLevelReward SetPlayerPoint(int prev, int next, int score)
    {
        //이전 점수 절반 ~ 현재 점수 까지가 0.5
        var from = 0;
        var to = 0;

        if (prev != 0)
        {
            var range = (int) (Mathf.Abs(prev - Sheet.level) * 0.5f);
            from = prev + range;
        }

        if (Sheet.level < next)
        {
            //현재 점수 ~ 다음 점수 절반 까지가 1 
            var range = (int) (Mathf.Abs(next - Sheet.level) * 0.5f);
            to = next - range;
            ScliderRectTransform.sizeDelta = new Vector2(ScliderRectTransform.sizeDelta.x, 250f);
        }
        //마지막 셀
        else
        {
            to = Sheet.level;
            ScliderRectTransform.sizeDelta = new Vector2(ScliderRectTransform.sizeDelta.x, 125f);
        }


        var t = Mathf.InverseLerp(from, to, score);
        LevelSlider.value = t;
        return this;
    }

    private void UpdateRewardReceive()
    {
        ReceiveReward.SetActive(false);
        ReceivePremiumReward.SetActive(false);

        var info = BattlePassInfo.Instance.SeasonPassRewardInfo;
        if (info.ContainsKey(Sheet.key))
            if (info[Sheet.key] != null)
            {
                if (info[Sheet.key].isReceivedPassReward)
                    ReceiveReward.SetActive(true);
                if (info[Sheet.key].isReceivedPremiumReward)
                    ReceivePremiumReward.SetActive(true);
            }

        var playerInfo = PlayerDataManager.Get<PlayerInfo>();
        if (Sheet.level <= playerInfo.attribute.Level)
        {
            var isEnable = ReceiveReward.activeSelf == false || ReceivePremiumReward.activeSelf == false;
            ReceiveRewardButton.SetActive(isEnable);
        }
        else
        {
            ReceiveRewardButton.SetActive(false);
        }
    }

    public void OnClickReceiveReward()
    {
        var playerInfo = PlayerDataManager.Get<PlayerInfo>();

        if (Sheet.level <= playerInfo.attribute.Level)
        {
            if (playerInfo.HasReward(Sheet.key) == false) return;

            var packet = new PacketBase();
            packet.PacketType = ePacketType.RECEIVE_PLAYER_LEVEL_REWARD;
            packet.hash["level_key"] = Sheet.key;
            // NetworkManager.Instance.Request(packet, res =>
            // {
            //     if (res.isSuccess()) UpdateRewardReceive();
            // });
        }
    }

    public class Data : IScrollData
    {
        public bool isLock;
        public int Level;
        public int NextLevel;
        public int PrevLevel;
        public PlayerLevel Sheet;
    }
}