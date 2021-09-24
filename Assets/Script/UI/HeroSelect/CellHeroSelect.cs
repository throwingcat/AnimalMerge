using System;
using SheetData;
using UnityEngine;
using UnityEngine.UI;

public class CellHeroSelect : MonoBehaviour
{
    private const string UNKNOWN_FACE = "portrait_unknown_face";
    private Action<CellHeroSelect> _onClick;
    public Image Face;

    public Hero Hero;
    public Text Name;
    public GameObject Selected;

    public void SetInfo(Hero hero)
    {
        Hero = hero;
        if (Hero.isUnlock)
        {
            Face.sprite = hero.face.ToSprite(hero.atlas);
            Name.text = hero.name.ToLocalization();
        }
        else
        {
            Face.sprite = UNKNOWN_FACE.ToSprite();
            Name.text = "???";
        }
    }

    public void SetClickEvent(Action<CellHeroSelect> onClick)
    {
        _onClick = onClick;
    }

    public void Select(bool isSelect)
    {
        Selected.SetActive(isSelect);
    }

    public void OnClick()
    {
        if (Hero.isUnlock)
            _onClick?.Invoke(this);
        else
            PartSimpleNotice.Show("not_open_hero");
    }
}