using UnityEngine;

public class PartQuestDailyReward : MonoBehaviour
{
    public enum eState
    {
        Disable,
        Enable,
        Clear
    }

    private int _index;

    private PageQuest _owner;
    public GameObject Clear;

    public GameObject Disable;
    public GameObject Enable;

    public void Set(PageQuest owner, int index, eState state)
    {
        _owner = owner;
        _index = index;
        Disable.SetActive(state == eState.Disable);
        Enable.SetActive(state == eState.Enable);
        Clear.SetActive(state == eState.Clear);
    }

    public void OnClick()
    {
        _owner.ReceiveDailyReward(_index);
    }
}