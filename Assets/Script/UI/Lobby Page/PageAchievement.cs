using System.Collections;
using System.Collections.Generic;
using SheetData;
using UnityEngine;
using Violet;

public class PageAchievement : MonoBehaviour
{
    public SUILoopScroll ScrollView;

    public void Show()
    {
        gameObject.SetActive(true);
        List<CellAchievement.Data> datas = new List<CellAchievement.Data>();

        var sheet = TableManager.Instance.GetTable<SheetData.Achievement>();
        foreach (var row in sheet)
        {
            datas.Add(new CellAchievement.Data()
            {
                Achievement = row.Value as Achievement,
            });
        }

        ScrollView.SetData(datas);
        ScrollView.UpdateAll();
        ScrollView.Move(0);
    }

    public void Exit()
    {
        gameObject.SetActive(false);
    }
}