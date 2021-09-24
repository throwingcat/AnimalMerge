using System.Collections.Generic;
using SheetData;
using UnityEngine;
using UnityEngine.UI;
using Violet;

public class LobbyPageCollection : LobbyPageBase
{
    public Text Group;
    public Image GroupTexture;
    public PartItemCard[] PartItemCards;

    public GameObject EnableChangeGroupButtonLeft;
    public GameObject EnableChangeGroupButtonRight;

    public GameObject Lock;
    public Text LockMessage;
    
    private List<Hero> _heroes = new List<Hero>();
    private Hero _currentHero;

    private bool _isFirstGroup = false;
    private bool _isLastGroup = false;

    public override void OnShow()
    {
        base.OnShow();

        _heroes.Clear();

        var sheet = TableManager.Instance.GetTable<Hero>();
        foreach (var row in sheet)
        {
            var group = row.Value as Hero;
            _heroes.Add(group);
        }

        _heroes.Sort((a, b) =>
        {
            if (a.index < b.index) return -1;
            if (a.index > b.index) return 1;
            return 0;
        });

        SetGroup(0);
    }

    public void OnClickPrevGroup()
    {
        MoveGroupIndex(-1);
    }

    public void OnClickNextGroup()
    {
        MoveGroupIndex(1);
    }

    private void MoveGroupIndex(int direction)
    {
        var index = _currentHero.index + direction;
        index = Mathf.Clamp(index, 0, _heroes.Count);

        SetGroup(index);
    }

    private void SetGroup(int index)
    {
        if (index < 0 || _heroes.Count <= index) return;

        _currentHero = _heroes[index];

        if (index == 0)
            _isFirstGroup = true;
        else
            _isFirstGroup = false;
        if (index == _heroes.Count - 1)
            _isLastGroup = true;
        else
            _isLastGroup = false;

        EnableChangeGroupButtonLeft.SetActive(!_isFirstGroup);
        EnableChangeGroupButtonRight.SetActive(!_isLastGroup);

        RefreshGroupList();
    }

    private void RefreshGroupList()
    {
        var group = UnitInventory.Instance.GetGroup(_currentHero.key);

        int index = 0;
        foreach (var unit in group)
        {
            PartItemCards[index].Set(unit);
            PartItemCards[index].SetClickEvent(OnClickUnit);
            index++;
        }

        if (_currentHero.isUnlock == false)
        {
            var stage = _currentHero.unlock_condition.ToTableData<Stage>();
            var chapter = stage.Chapter.ToTableData<Chapter>();
            Lock.SetActive(true);
            LockMessage.text = string.Format("HeroUnlockStageFormat".ToLocalization(), chapter.name.ToLocalization());
        }
        else
        {
            Lock.SetActive(false);
        }
    }

    public override void Refresh()
    {
        RefreshGroupList();
    }

    public void OnClickUnit(UnitInventory.Unit unit)
    {
        var popup = UIManager.Instance.ShowPopup<PopupUnitInfo>();
        popup.Set(unit);
    }
}