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
    public GameObject Lock;
    public Stage Stage;

    private Action<CellStage> _onClick;

    public enum eState
    {
        Lock,
        Unlock,
        Clear,
        None,
    }

    private eState _state = eState.None;

    public void Set(Stage stage, Action<CellStage> onClick)
    {
        Stage = stage;
        _onClick = onClick;
        Name.text = string.Format("스테이지 {0}", (stage.Index + 1));

        foreach (var reward in Rewards)
            reward.gameObject.SetActive(false);
        for (int i = 0; i < stage.Rewards.Count; i++)
        {
            Rewards[i].gameObject.SetActive(true);
            Rewards[i].Set(stage.Rewards[i]);
        }
    }

    public void SetState(eState state)
    {
        _state = state;
        Lock.SetActive(state == eState.Lock);
        Clear.SetActive(state == eState.Clear);
    }

    public void SetSelect(bool isSelect)
    {
        Select.SetActive(isSelect);
    }

    public void OnClick()
    {
        switch (_state)
        {
            case eState.Lock:
                PartSimpleNotice.Show("아직 진입할 수 없습니다");
                break;
            case eState.Unlock:
            case eState.Clear:
                _onClick?.Invoke(this);
                break;
            case eState.None:
                break;
        }
    }
}