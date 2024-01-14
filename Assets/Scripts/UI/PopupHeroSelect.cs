using System.Collections.Generic;
using Common;
using SheetData;
using UnityEngine;
using UnityEngine.UI;
using Violet;

public class PopupHeroSelect : SUIPanel
{
    private readonly List<CellHeroSelect> _cells = new List<CellHeroSelect>();
    public List<PartItemCard> Blocks = new List<PartItemCard>();
    public CellHeroSelect CellPrefab;
    public GameObject CellRoot;

    public Text Name;
    public Image Portrait;

    public GameObject Selected;
    public GameObject Select;
    private Hero _currentHero;

    public System.Action onUpdateSelectedHero; 
    protected override void OnShow()
    {
        base.OnShow();

        var table = TableManager.Instance.GetTable<Hero>();

        var need = table.Count - _cells.Count;
        for (var i = 0; i < need; i++)
        {
            var cell = Instantiate(CellPrefab, CellRoot.transform);
            cell.transform.LocalReset();
            _cells.Add(cell);
        }

        foreach (var cell in _cells)
            cell.gameObject.SetActive(false);

        var index = 0;
        foreach (var row in table)
        {
            var hero = row.Value as Hero;
            _cells[index].SetInfo(hero);
            _cells[index].SetClickEvent(OnClickCell);
            _cells[index].gameObject.SetActive(true);
            index++;
        }

        OnClickCell(_cells[0]);
    }

    private void OnClickCell(CellHeroSelect selected)
    {
        foreach (var cell in _cells)
            cell.Select(false);
        selected.Select(true);

        UpdateHero(selected.Hero);
    }

    public void UpdateHero(Hero hero)
    {
        _currentHero = hero;
        
        Name.text = hero.name.ToLocalization();
        Portrait.sprite = hero.body.ToSprite(hero.atlas);

        var units = UnitInventory.Instance.GetGroup(hero.key);

        var index = 0;
        foreach (var unit in units)
        {
            Blocks[index].Set(unit);
            index++;
        }

        PlayerInfo playerInfo = PlayerDataManager.Get<PlayerInfo>();
        if (_currentHero.key == playerInfo.attribute.SelectHero)
        {
            Select.SetActive(false);
            Selected.SetActive(true);
        }
        else
        {
            Select.SetActive(true);
            Selected.SetActive(false);   
        }
    }

    public void OnClickSelect()
    {
        PlayerInfo playerInfo = PlayerDataManager.Get<PlayerInfo>();
        
        PacketBase packet = new PacketBase();
        packet.PacketType = ePacketType.CHANGE_HERO;
        packet.hash.Add("hero",_currentHero.key);
        // NetworkManager.Instance.Request(packet, (res) =>
        // {
        //     playerInfo.attribute.SelectHero = res.hash["hero"].ToString();
        //     Hero result = playerInfo.attribute.SelectHero.ToTableData<Hero>();
        //     UpdateHero(result);
        //     onUpdateSelectedHero?.Invoke();
        // });
    }
}