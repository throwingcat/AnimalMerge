using System.Collections;
using System.Collections.Generic;
using Define;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Violet;

public class PopupGameResult : SUIPanel
{
    public GameObject Victory;
    public GameObject Defeat;
    public GameObject RankScoreRoot;
    public Text RankScoreText;
    public Text AddRankScoreText;

    public void SetResult(bool isWin, int beforeScore)
    {
        IgnoreBackPress = true;
        Victory.SetActive(false);
        Defeat.SetActive(false);
        RankScoreRoot.SetActive(false);

        StartCoroutine(Process(isWin, beforeScore));
    }

    private IEnumerator Process(bool isWin, int beforeScore)
    {
        Victory.SetActive(isWin);
        Defeat.SetActive(!isWin);

        yield return new WaitForSeconds(1f);

        RankScoreRoot.SetActive(true);

        int add = PlayerInfo.Instance.RankScore - beforeScore;
        AddRankScoreText.text = string.Format("{0}{1}", add >= 0 ? "+" : "", add); 
        var score = beforeScore;
        DOTween.To(() => score, x =>
        {
            score = x;
            RankScoreText.text = Utils.ParseComma(score);
            IgnoreBackPress = false;
        }, PlayerInfo.Instance.RankScore, 0.5f);
    }

    public void OnClickExit()
    {
        if (IgnoreBackPress) return;

        Hide();
        GameManager.Instance.ChangeGameState(eGAME_STATE.Lobby);
    }
}