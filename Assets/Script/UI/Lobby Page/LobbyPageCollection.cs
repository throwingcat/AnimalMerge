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

    private List<UnitGroup> _groups = new List<UnitGroup>();
    private UnitGroup _currentGroup;

    private bool _isFirstGroup = false;
    private bool _isLastGroup = false;

    public override void OnShow()
    {
        base.OnShow();

        _groups.Clear();

        var sheet = TableManager.Instance.GetTable<UnitGroup>();
        foreach (var row in sheet)
        {
            var group = row.Value as UnitGroup;
            _groups.Add(group);
        }

        _groups.Sort((a, b) =>
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
        var index = _currentGroup.index + direction;
        index = Mathf.Clamp(index, 0, _groups.Count);

        SetGroup(index);
    }

    private void SetGroup(int index)
    {
        if (index < 0 || _groups.Count <= index) return;

        _currentGroup = _groups[index];

        if (index == 0)
            _isFirstGroup = true;
        else
            _isFirstGroup = false;
        if (index == _groups.Count - 1)
            _isLastGroup = true;
        else
            _isLastGroup = false;

        EnableChangeGroupButtonLeft.SetActive(!_isFirstGroup);
        EnableChangeGroupButtonRight.SetActive(!_isLastGroup);

        RefreshGroupList();
    }

    private void RefreshGroupList()
    {
        var group = UnitInventory.Instance.GetGroup(_currentGroup.key);

        int index = 0;
        foreach (var unit in group)
        {
            PartItemCards[index].Set(unit);
            PartItemCards[index].SetClickEvent(OnClickUnit);
            index++;
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