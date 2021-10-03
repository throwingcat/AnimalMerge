using System.Collections.Generic;
using SheetData;
using Violet;

public class PageBattlePass : LobbyPageBase
{
    public SUILoopScroll ScrollView;

    public override void OnShow()
    {
        base.OnShow();

        var sheet = TableManager.Instance.GetTable<BattlePass>();

        var list = new List<BattlePass>();
        foreach (var row in sheet)
        {
            var pass = row.Value as BattlePass;
            list.Add(pass);
        }

        list.Sort((a, b) =>
        {
            if (a.score < b.score) return -1;
            if (a.score > b.score) return 1;
            return 0;
        });

        var items = new List<CellBattlePass.Data>();
        for (var i = 0; i < list.Count; i++)
            items.Add(new CellBattlePass.Data
            {
                isLock = false,
                Pass = list[i],
                PlayerScore = PlayerInfo.Instance.RankScore,
                NextScore = i == list.Count - 1 ? 0 : list[i + 1].score,
                PrevScore = i == 0 ? 0 : list[i - 1].score
            });
        items.Sort((a, b) =>
        {
            if (a.Pass.score < b.Pass.score) return 1;
            if (a.Pass.score > b.Pass.score) return -1;
            return 0;
        });
        ScrollView.SetData(items);
        ScrollView.UpdateAll();
        ScrollView.Move(ScrollView.DataLength - 1);
    }
}