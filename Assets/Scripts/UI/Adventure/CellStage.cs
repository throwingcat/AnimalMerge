using System;
using System.Collections;
using System.Collections.Generic;
using SheetData;
using UnityEngine;
using UnityEngine.UI;

public class CellStage : MonoBehaviour
{
    public RectTransform Root;
    public RectTransform Selected;
    public Text Name;
    public List<PartItemCard> Rewards;
    public GameObject Complete;
    public GameObject New;
    public Stage Stage;

    private Action<CellStage> _onClick;

    public void Set(Stage stage, Action<CellStage> onClick)
    {
        Stage = stage;
        _onClick = onClick;

        Name.text = string.Format("스테이지 {0}", (stage.Index + 1));

        bool isClear = PlayerTracker.Instance.Contains(stage.key);
        Complete.SetActive(isClear);
        New.SetActive(!isClear);
        SetSelect(false);
        
        //보상 설정
        foreach (var reward in Rewards)
            reward.gameObject.SetActive(false);
        for (int i = 0; i < stage.Rewards.Count; i++)
        {
            Rewards[i].gameObject.SetActive(true);
            Rewards[i].Set(stage.Rewards[i]);
        }
    }

    public void SetSelect(bool isSelected)
    {
        Selected.gameObject.SetActive(isSelected);

        if (isSelected)
        {
            //Root 위치 설정
            var pos = Root.anchoredPosition;
            pos.x = 597;
            Root.anchoredPosition = pos;
            
            //Selected 위치 설정
            pos = Selected.anchoredPosition;
            pos.x = 0;
            Selected.anchoredPosition = pos;
        }
        else
        {
            //Root 위치 설정
            var pos = Root.anchoredPosition;
            pos.x = 510;
            Root.anchoredPosition = pos;
            
            //Selected 위치 설정
            pos = Selected.anchoredPosition;
            pos.x = -200;
            Selected.anchoredPosition = pos;
        }
    }
    public void OnClick()
    {
        _onClick?.Invoke(this);
    }
}