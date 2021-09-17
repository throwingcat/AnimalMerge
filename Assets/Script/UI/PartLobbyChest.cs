using System;
using System.Collections;
using System.Collections.Generic;
using Common;
using DG.Tweening;
using Packet;
using SheetData;
using UnityEngine;
using UnityEngine.UI;
using Violet;

public class PartLobbyChest : MonoBehaviour
{
    public Image[] Texture;
    public Text NeedTime;
    public Text RemainTime;

    public GameObject Empty;
    public GameObject Ready;
    public GameObject Progress;
    public GameObject Complete;

    public GameObject[] ReadyGrade;
    public GameObject[] ProgressGrade;

    public GameObject Root;

    public enum eSTATE
    {
        Empty,
        Ready,
        Progress,
        Complete,
    }

    public eSTATE State = eSTATE.Empty;
    public int Index = 0;

    public ChestInventory.ChestSlot ChestSlot
    {
        get
        {
            if (Index < ChestInventory.Instance.ChestSlots.Length)
            {
                if (ChestInventory.Instance.ChestSlots[Index].inDate.IsNullOrEmpty() == false &&
                    ChestInventory.Instance.ChestSlots[Index].Key.IsNullOrEmpty() == false)
                    return ChestInventory.Instance.ChestSlots[Index];
            }

            return null;
        }
    }

    public SheetData.Chest Sheet
    {
        get
        {
            if (ChestSlot != null)
                return ChestSlot.Key.ToTableData<Chest>();
            return null;
        }
    }

    public void OnUpdate()
    {
        if (ChestSlot == null)
            SetEmpty();
        else
        {
            foreach (var t in Texture)
            {
                t.sprite = Sheet.texture.ToSprite();
            }

            var chest = ChestInventory.Instance.ChestSlots[Index];

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
                    SetProgress((long) remain.TotalSeconds);
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

        int grade = ChestSlot.Grade;
        for (int i = 0; i < ReadyGrade.Length; i++)
        {
            ReadyGrade[i].SetActive(i < grade);
        }
    }

    public void SetProgress(long remain_seconds)
    {
        if (State != eSTATE.Progress)
        {
            State = eSTATE.Progress;
            SetActiveRoot(State);
            int grade = ChestSlot.Grade;
            for (int i = 0; i < ProgressGrade.Length; i++)
            {
                ProgressGrade[i].SetActive(i < grade);
            }
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

    public void OnPress()
    {
        Root.transform.DOScale(-0.05f, 0.2f).SetRelative(true).SetEase(Ease.OutElastic).Play();
    }

    public void OnRelease()
    {
        Root.transform.DOScale(1f, 0.2f).SetRelative(false).SetEase(Ease.OutElastic).Play();
    }

    public void OnClick()
    {
        if (Sheet != null)
        {
            switch (State)
            {
                case eSTATE.Empty:
                    break;
                case eSTATE.Ready:
                {
                    var popup = UIManager.Instance.ShowPopup<PopupChestUnlock>();
                    popup.Set(ChestSlot);
                }
                    break;
                case eSTATE.Progress:
                    break;
                case eSTATE.Complete:
                {
                    var packet = new PacketBase();
                    packet.PacketType = ePACKET_TYPE.CHEST_COMPLETE;
                    packet.hash = new Dictionary<string, object>();
                    packet.hash.Add("inDate", ChestSlot.inDate);
                    packet.hash.Add("chest_key", ChestSlot.Key);
                    
                    NetworkManager.Instance.Request(packet, (res) =>
                    {
                        var packet = res as PacketReward;
                        var popup = UIManager.Instance.ShowPopup<PopupChestOpen>();
                        popup.Set(res.hash["chest_key"].ToString(), packet.Rewards);
                    });
                }
                    break;
            }
        }
    }
}