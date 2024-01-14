using System.Collections.Generic;
using Common;
using DG.Tweening;
using SheetData;
using UnityEngine;
using UnityEngine.UI;

public class PartLobbyChest : MonoBehaviour
{
    public enum eSTATE
    {
        Empty,
        Ready,
        Progress,
        Complete
    }

    public GameObject Complete;

    public GameObject Empty;
    public int Index;
    public Text NeedTime;
    public GameObject Progress;
    public GameObject[] ProgressGrade;
    public GameObject Ready;

    public GameObject[] ReadyGrade;
    public Text RemainTime;

    public GameObject Root;

    public eSTATE State = eSTATE.Empty;
    public Image[] Texture;

    public ChestInventory.ChestSlot ChestSlot
    {
        get
        {
            if (Index < ChestInventory.Instance.ChestSlots.Length)
                if (ChestInventory.Instance.ChestSlots[Index].key.IsNullOrEmpty() == false)
                    return ChestInventory.Instance.ChestSlots[Index];

            return null;
        }
    }

    public Chest Sheet
    {
        get
        {
            if (ChestSlot != null)
                return ChestSlot.key.ToTableData<Chest>();
            return null;
        }
    }

    public void OnUpdate()
    {
        if (ChestSlot == null)
        {
            SetEmpty();
        }
        else
        {
            foreach (var t in Texture) t.sprite = Sheet.texture.ToSprite();

            var chest = ChestInventory.Instance.ChestSlots[Index];

            //대기 상태
            if (chest.isProgress == false)
            {
                SetReady();
            }
            else
            {
                var remain = chest.startTime.AddSeconds(Sheet.time) - GameManager.GetTime();

                //진행중
                if (0 < remain.TotalSeconds)
                    SetProgress((long) remain.TotalSeconds);
                //완료
                else
                    SetComplete();
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

        var grade = ChestSlot.grade;
        for (var i = 0; i < ReadyGrade.Length; i++) ReadyGrade[i].SetActive(i < grade);
    }

    public void SetProgress(long remain_seconds)
    {
        if (State != eSTATE.Progress)
        {
            State = eSTATE.Progress;
            SetActiveRoot(State);
            var grade = ChestSlot.grade;
            for (var i = 0; i < ProgressGrade.Length; i++) ProgressGrade[i].SetActive(i < grade);
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
                    packet.PacketType = ePacketType.CHEST_COMPLETE;
                    packet.hash = new Dictionary<string, object>();
                    packet.hash.Add("chest_key", ChestSlot.key);

                    // NetworkManager.Instance.Request(packet, res =>
                    // {
                    //     var packet = res as PacketReward;
                    //     var popup = UIManager.Instance.ShowPopup<PopupChestOpen>();
                    //     popup.Set(res.hash["chest_key"].ToString(), packet.Rewards);
                    // });
                }
                    break;
            }
    }
}