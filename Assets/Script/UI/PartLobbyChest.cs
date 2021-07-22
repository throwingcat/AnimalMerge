using System;
using System.Collections;
using System.Collections.Generic;
using SheetData;
using UnityEngine;
using UnityEngine.UI;

public class PartLobbyChest : MonoBehaviour
{
    public Image[] Texture;
    public Text NeedTime;
    public Text RemainTime;

    public GameObject Empty;
    public GameObject Ready;
    public GameObject Progress;
    public GameObject Complete;

    public enum eSTATE
    {
        Empty,
        Ready,
        Progress,
        Complete,
    }

    public eSTATE State = eSTATE.Empty;
    public int Index = 0;

    public Chest Sheet
    {
        get
        {
            if (Index < ChestInventory.Instance.Chests.Count)
            {
                if (ChestInventory.Instance.Chests[Index].inDate.IsNullOrEmpty() == false &&
                    ChestInventory.Instance.Chests[Index].Key.IsNullOrEmpty() == false)
                {
                    return ChestInventory.Instance.Chests[Index].Key.ToTableData<Chest>();
                }
            }
            return null;
        }
    }

    public void OnUpdate()
    {
        if(Sheet == null)
            SetEmpty();
        else
        {
            foreach (var t in Texture)
            {
                var sheet = ChestInventory.Instance.Chests[Index].Key.ToTableData<Chest>();
                t.sprite = sheet.texture.ToSprite();
            }

            var chest = ChestInventory.Instance.Chests[Index];
            
            //대기 상태
            if (chest.isProgress == false)
            {
                SetReady();
            }
            else
            {
                var remain = chest.StartTime.AddSeconds(Sheet.time) - GameManager.GetTime();
                
                //진행중
                if (0 < remain.TotalSeconds)
                {
                    SetProgress((long)remain.TotalSeconds);
                }
                //완료
                else
                {
                    SetComplete();
                }
            }
        }
    }

    public void SetEmpty()
    {
        State = eSTATE.Empty;
        SetActiveRoot(State);
    }

    public void SetReady()
    {
        if (State != eSTATE.Ready)
        {
            State = eSTATE.Ready;
            SetActiveRoot(State);
            NeedTime.text = Utils.ParseSeconds(Sheet.time);
        }
    }

    public void SetProgress(long remain_seconds)
    {
        if (State != eSTATE.Progress)
        {
            State = eSTATE.Progress;
            SetActiveRoot(State);
        }

        RemainTime.text = Utils.ParseSeconds(remain_seconds);
    }

    public void SetComplete()
    {
        if (State != eSTATE.Complete)
        {
            State = eSTATE.Complete;
            SetActiveRoot(State);
        }
    }

    private void SetActiveRoot(eSTATE state)
    {
        Empty.SetActive(state == eSTATE.Empty);
        Ready.SetActive(state == eSTATE.Ready);
        Progress.SetActive(state == eSTATE.Progress);
        Complete.SetActive(state == eSTATE.Complete);
    }
}