using System.Collections.Generic;
using Violet;

public class PanelPlayerLevelReward : SUIPanel
{
    public SUILoopScroll ScrollView;

    protected override void OnShow()
    {
        base.OnShow();
        
        Refresh();
    }

    public void Refresh()
    {
        var list = PlayerInfo.Instance.GetRewardInfos();
        var items = new List<CellPlayerLevelReward.Data>();
        for (var i = 0; i < list.Count; i++)
            items.Add(new CellPlayerLevelReward.Data
            {
                isLock = PlayerInfo.Instance.isPurchasePremium == false,
                Sheet = list[i].Sheet,
                Level = PlayerInfo.Instance.Level,
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
            if (items[i].Sheet.level == PlayerInfo.Instance.Level)
            {
                index = i;
                break;
            }

        ScrollView.Move(index);
    }
}