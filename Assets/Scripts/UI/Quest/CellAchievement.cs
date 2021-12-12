using System.Collections.Generic;
using SheetData;
using UnityEngine;
using UnityEngine.UI;
using Violet;

public class CellAchievement : MonoBehaviour, IScrollCell
{
    public Data CellData;
    public GameObject Complete;
    public Text Description;
    public Text Progress;
    public List<SimpleRewardView> Rewards = new List<SimpleRewardView>();

    public void UpdateCell(IScrollData data)
    {
        CellData = data as Data;

        Description.text = CellData.Achievement.DescriptionText;
        Progress.text = CellData.Achievement.ProgressText;
        Complete.SetActive(false);

        foreach (var r in Rewards)
            r.gameObject.SetActive(false);

        var index = 0;
        foreach (var reward in CellData.Achievement.Rewards)
        {
            Rewards[index].Set(reward);
            index++;
        }
    }

    public class Data : IScrollData
    {
        public Achievement Achievement;
    }
}