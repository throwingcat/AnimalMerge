using System;
using Common;
using SheetData;
using UnityEngine;
using UnityEngine.UI;
using Violet;

public class PopupUnitInfo : SUIPanel
{
    private UnitInventory.Unit _unit;
    public Text Damage;
    public Text Description;
    public Text Group;
    public Text Name;
    public Text Price;
    public PartItemCard Unit;
    public GameObject UpgradeButton;
    public GameObject UpgradeComplete;
    public Text UpgradeDamage;

    protected override void OnReShow()
    {
        base.OnReShow();

        Set(UnitInventory.Instance.GetUnit(_unit.Key));
    }

    public void Set(UnitInventory.Unit unit)
    {
        _unit = unit;
        Unit.Set(unit);
        var sheet = unit.Key.ToTableData<Unit>();
        var master = sheet.master.ToTableData<Hero>();
        Name.text = sheet.name.ToLocalization();
        Description.text = string.Format("{0} 의 배경설명", sheet.name.ToLocalization());
        Group.text = master.name.ToLocalization();

        var current = Utils.GetUnitDamage(sheet.score, unit.Level);
        current = Math.Truncate(current * 10) / 10;
        Damage.text = string.Format("atk_value_format".ToLocalization(), current);

        if (unit.IsMaxLevel())
        {
            UpgradeComplete.SetActive(true);
            UpgradeDamage.gameObject.SetActive(false);
            UpgradeButton.SetActive(false);
        }
        else
        {
            UpgradeComplete.SetActive(false);
            UpgradeDamage.gameObject.SetActive(true);
            UpgradeButton.SetActive(true);

            var upgrade = Utils.GetUnitDamage(sheet.score, unit.Level + 1) - current;
            upgrade = Math.Truncate(upgrade * 10) / 10;
            UpgradeDamage.text = string.Format("atk_value_upgrade_format_color".ToLocalization(), upgrade);
            Price.text = "1,000";
        }
    }

    public void OnClickUpgrade()
    {
        var packet = new PacketBase();
        packet.PacketType = ePacketType.UNIT_LEVEL_UP;
        packet.hash.Add("unit_key", _unit.Key);
        NetworkManager.Instance.Request(packet, res =>
        {
            if (res.isSuccess())
            {
                var popup = UIManager.Instance.ShowPopup<PopupUnitUpgrade>();
                popup.Set(_unit);

                var index = 0;
                while (true)
                {
                    var panel = GetPanel(index);
                    if (panel == null) break;

                    if (panel is PanelLobby)
                    {
                        var lobby = panel as PanelLobby;
                        if (lobby.CurrentPage is LobbyPageCollection)
                            lobby.CurrentPage.Refresh();
                        break;
                    }

                    index++;
                }
            }
        });
    }

    public void OnClickBackground()
    {
        BackPress();
    }
}