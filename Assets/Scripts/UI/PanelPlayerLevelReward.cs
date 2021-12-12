using System.Collections.Generic;
using Violet;

public class PanelPlayerLevelReward : SUIPanel
{
    public SUILoopScroll ScrollView;

    public PlayerInfo PlayerInfo => PlayerDataManager.Get<PlayerInfo>();
    protected override void OnShow()
    {
        base.OnShow();
        
        Refresh();
    }

    public void Refresh()
    {
        var playerInfo = PlayerDataManager.Get<PlayerInfo>();

        var list = playerInfo.LevelRewardInfos;
        var items = new List<CellPlayerLevelReward.Data>();
        for (var i = 0; i < list.Count; i++)
            items.Add(new CellPlayerLevelReward.Data
            {
                isLock = playerInfo.elements.isPurchasePremium == false,
                Sheet = list[i].Sheet,
                Level = playerInfo.elements.Level,
                NextLevel = i == list.Count - 1 ? 0 : list[i + 1].Sheet.level,
                PrevLevel = i == 0 ? 0 : list[i - 1].Sheet.level
            });
        items.Sort((a, b) =>
        {
            if (a.Sheet.level < b.Sheet.level) return 1;
            if (a.Sheet.level > b.Sheet.level) return -1;
            return 0;
        });

        ScrollView.SetData(items);

        var index = items.Count - 1;
        for (var i = 0; i < items.Count; i++)
            if (items[i].Sheet.level == playerInfo.elements.Level)
            {
                index = i;
                break;
            }

        ScrollView.Move(index);
    }
}