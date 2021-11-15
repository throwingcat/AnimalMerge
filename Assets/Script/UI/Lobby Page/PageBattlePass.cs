using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Server;
using UnityEngine;
using UnityEngine.UI;
using Violet;

public class PageBattlePass : LobbyPageBase
{
    private Coroutine _coroutine;

    private string _currentSeason = "";
    private bool _isUpdate;

    public Text Point;
    public SUILoopScroll ScrollView;

    public GameObject SeasonIn;
    public GameObject SeasonOut;
    public Text SeasonRemainTime;
    
    public GameObject BattlePassToolTip;

    public override void OnShow()
    {
        base.OnShow();

        if (_coroutine == null)
            _coroutine = StartCoroutine(UpdateProcess());

        BattlePassToolTip.gameObject.SetActive(false);
        
        Refresh();
    }

    public void Refresh()
    {
        var season_enable = RefreshSeasonState();
        if (season_enable)
        {
            _currentSeason = BattlePassInfo.CurrentSeason.key;

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

            UpdatePoint();
        }
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
            //시즌 변경 확인
            if (_currentSeason.Equals(BattlePassInfo.CurrentSeason.key) == false)
            {
                var isDone = false;
                AnimalMergeServer.Instance.DownloadDB<DBBattlePassInfo>(() =>
                {
                    _currentSeason = BattlePassInfo.CurrentSeason.key;
                    Refresh();
                    isDone = true;
                });
                while (isDone == false)
                    yield return new WaitForSeconds(1f);
            }

            var eanble_season = RefreshSeasonState();
            if (eanble_season == false)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }

            var t = TimeSpan.FromSeconds(BattlePassInfo.Instance.SeasonReaminTime);
            var stringBuilder = new StringBuilder();

            //Day
            if (0 < t.Days)
            {
                stringBuilder.Append(string.Format("{0}{1} ", t.Days, "Day".ToLocalization()));
                stringBuilder.Append(string.Format("{0}{1} ", t.Hours, "Hour".ToLocalization()));
                stringBuilder.Append(string.Format("{0}{1} ", t.Minutes, "Minute".ToLocalization()));
                //stringBuilder.Append(string.Format("{0}{1}", t.Seconds, "Seconds".ToLocalization()));
            }
            else
            {
                //Hour
                if (0 < t.Hours)
                {
                    stringBuilder.Append(string.Format("{0}{1} ", t.Hours, "Hour".ToLocalization()));
                    stringBuilder.Append(string.Format("{0}{1} ", t.Minutes, "Minute".ToLocalization()));
                    //stringBuilder.Append(string.Format("{0}{1}", t.Seconds, "Seconds".ToLocalization()));
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

    public bool RefreshSeasonState()
    {
        var isResult = BattlePassInfo.CurrentSeason != null;

        if (SeasonIn.activeSelf != isResult)
            SeasonIn.SetActive(isResult);
        if (SeasonOut.activeSelf != !isResult)
            SeasonOut.SetActive(!isResult);

        return isResult;
    }

    public void OnClickToolTip()
    {
        BattlePassToolTip.SetActive(true);
    }

    public void OnCloseToolTip()
    {
        BattlePassToolTip.SetActive(false);
    }
}