using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Violet;

public class PageBattlePass : LobbyPageBase
{
    private Coroutine _coroutine;
    private bool _isUpdate;

    public Text Point;
    public SUILoopScroll ScrollView;
    public Text SeasonRemainTime;

    public override void OnShow()
    {
        base.OnShow();

        var list = BattlePassInfo.Instance.SeasonPassItems;

        var items = new List<CellBattlePass.Data>();
        for (var i = 0; i < list.Count; i++)
            items.Add(new CellBattlePass.Data
            {
                isLock = BattlePassInfo.Instance.isPurchasePremiumPass == false,
                Pass = list[i],
                PlayerPoint = BattlePassInfo.Instance.Point,
                NextPoint = i == list.Count - 1 ? 0 : list[i + 1].point,
                PrevPoint = i == 0 ? 0 : list[i - 1].point
            });
        items.Sort((a, b) =>
        {
            if (a.Pass.point < b.Pass.point) return 1;
            if (a.Pass.point > b.Pass.point) return -1;
            return 0;
        });

        ScrollView.SetData(items);
        ScrollView.Move(ScrollView.DataLength - 2);

        if (_coroutine != null)
            StopCoroutine(_coroutine);
        _coroutine = null;
        _coroutine = StartCoroutine(UpdateProcess());

        UpdatePoint();
    }

    public override void OnHide()
    {
        base.OnHide();

        _isUpdate = false;

        if (_coroutine != null)
            StopCoroutine(_coroutine);
        _coroutine = null;
    }

    public IEnumerator UpdateProcess()
    {
        _isUpdate = true;
        while (_isUpdate)
        {
            if (BattlePassInfo.Instance.isActiveSeason == false) break;

            var t = TimeSpan.FromSeconds(BattlePassInfo.Instance.SeasonReaminTime);
            var stringBuilder = new StringBuilder();

            //Day
            if (0 < t.Days)
            {
                stringBuilder.Append(string.Format("{0}{1} ", t.Days, "Day".ToLocalization()));
                stringBuilder.Append(string.Format("{0}{1} ", t.Hours, "Hour".ToLocalization()));
                stringBuilder.Append(string.Format("{0}{1} ", t.Minutes, "Minute".ToLocalization()));
                stringBuilder.Append(string.Format("{0}{1}", t.Seconds, "Seconds".ToLocalization()));
            }
            else
            {
                //Hour
                if (0 < t.Hours)
                {
                    stringBuilder.Append(string.Format("{0}{1} ", t.Hours, "Hour".ToLocalization()));
                    stringBuilder.Append(string.Format("{0}{1} ", t.Minutes, "Minute".ToLocalization()));
                    stringBuilder.Append(string.Format("{0}{1}", t.Seconds, "Seconds".ToLocalization()));
                }
                //Min
                else
                {
                    stringBuilder.Append(string.Format("{0}{1} ", t.Minutes, "Minute".ToLocalization()));
                    stringBuilder.Append(string.Format("{0}{1}", t.Seconds, "Seconds".ToLocalization()));
                }
            }

            SeasonRemainTime.text = stringBuilder.ToString();
            yield return new WaitForSeconds(1f);
        }
    }

    public void UpdatePoint()
    {
        Point.text = string.Format("패스 포인트 : {0}", BattlePassInfo.Instance.Point);
    }
}