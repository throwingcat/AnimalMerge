using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CellQeustInfo : MonoBehaviour
{
    public GameObject RefreshRoot;
    public GameObject ReadyRoot;

    #region Refresh

    public Text RemainRefreshTime;
    private Coroutine _coroutine;

    #endregion

    #region Ready

    public Text Description;
    public Text Progress;
    public Text ExpReward;
    public Text CoinRewawrd;

    public GameObject ClearRoot;
    public GameObject ProgressRoot;

    #endregion

    private QuestInfo.QuestSlot _slot;
    private PageQuest _owner;

    public void Set(PageQuest owner, QuestInfo.QuestSlot slot)
    {
        _owner = owner;
        _slot = slot;
        //퀘스트 만료 상태 (갱신 대기)
        if (slot.isExpire)
        {
            RefreshRoot.SetActive(true);
            ReadyRoot.SetActive(false);

            if (_coroutine != null)
                StopCoroutine(_coroutine);
            _coroutine = StartCoroutine(UpdateProcess());
        }
        else
        {
            RefreshRoot.SetActive(false);
            ReadyRoot.SetActive(true);

            Description.text = slot.DescriptionText;
            Progress.text = slot.ProgressText;
            ExpReward.text = slot.Sheet.Point.ToString();
            CoinRewawrd.text = slot.Sheet.Coin.ToString();

            ClearRoot.SetActive(slot.isClear);
            ProgressRoot.SetActive(slot.isClear == false);
        }
    }

    private IEnumerator UpdateProcess()
    {
        while (_slot.isExpire)
        {
            var remain = (_slot.RefreshTime - GameManager.GetTime()).TotalSeconds;
            RemainRefreshTime.text = Utils.ParseSeconds((long) remain);
            yield return new WaitForSeconds(1f);
        }

        if (_slot.isExpire == false)
            Set(_owner, _slot);
    }

    public void OnClickRefresh()
    {
        _owner.QuestRefresh(_slot.Index);
    }

    public void OnClickComplete()
    {
        _owner.QuestComplete(_slot.Index);
    }
}