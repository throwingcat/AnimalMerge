using System;
using System.Collections;
using System.Collections.Generic;
using SheetData;
using UnityEngine;
using UnityEngine.UI;

public class CellStage : MonoBehaviour
{
    public Text Name;
    public List<PartItemCard> Rewards;
    public GameObject Clear;
    public GameObject Select;
    
    public Stage Stage;

    private Action<CellStage> _onClick;
    public void Set(Stage stage,Action<CellStage> onClick)
    {
        Stage = stage;
        _onClick = onClick;
        Name.text = string.Format("스테이지 {0}", (stage.Index + 1));
        
        foreach(var reward in Rewards)
            reward.gameObject.SetActive(false);
        for (int i = 0; i < stage.Rewards.Count; i++)
        {
            Rewards[i].gameObject.SetActive(true);
            Rewards[i].Set(stage.Rewards[i]);
        }
    }

    public void SetClear(bool isClear)
    {
        Clear.SetActive(isClear);
    }
    public void SetSelect(bool isSelect)
    {
        Select.SetActive(isSelect);        
    }
    
    public void OnClick()
    {
        _onClick?.Invoke(this);
    }
}
