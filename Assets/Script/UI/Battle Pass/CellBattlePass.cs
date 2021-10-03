using System.Collections;
using System.Collections.Generic;
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

    private  CellBattlePass SetPass(BattlePass pass)
    {
        BattlePass = pass;
        PassReward.Set(pass.PassReward);
        for (int i = 0; i < PremiumRewards.Count; i++)
            PremiumRewards[i].Set(pass.PremiumRewards[i]);
        Score.text = pass.score.ToString();
        return this;
    }

    private  CellBattlePass SetLock(bool isLock)
    {
        PremiumPassLock.SetActive(isLock);
        return this;
    }

    private  CellBattlePass SetPlayerScore(int prev, int next, int score)
    {
        //이전 점수 절반 ~ 현재 점수 까지가 0.5
        int from = 0;
        int to = 0;
        
        if (prev != 0)
        {
            int range = (int) (Mathf.Abs(prev - BattlePass.score) * 0.5f);
            from = prev + range;
        }

        if (BattlePass.score < next)
        {
            //현재 점수 ~ 다음 점수 절반 까지가 1 
            int range = (int) (Mathf.Abs(next - BattlePass.score) * 0.5f);
            to = next - range;
            ScliderRectTransform.sizeDelta = new Vector2(ScliderRectTransform.sizeDelta.x,250f);
        }
        //마지막 셀
        else
        {
            to = BattlePass.score;
            ScliderRectTransform.sizeDelta = new Vector2(ScliderRectTransform.sizeDelta.x,125f);
        }


        var t = Mathf.InverseLerp(from, to, score);
        ScoreSlider.value = t;
        return this;
    }

    public class Data : IScrollData
    {
        public BattlePass Pass;
        public int PrevScore;
        public int NextScore;
        public int PlayerScore;
        public bool isLock;
    }
    
    public void UpdateCell(IScrollData data)
    {
        var celldata = data as Data;

        SetPass(celldata.Pass)
            .SetLock(celldata.isLock)
            .SetPlayerScore(celldata.PrevScore, celldata.NextScore, celldata.PlayerScore);
    }
}