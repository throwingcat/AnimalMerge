using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using SheetData;
using UnityEngine;
using UnityEngine.UI;

public class IngamePlayerPortrait : MonoBehaviour
{
    public Image HeroPortrait;
    public Text PlayerName;

    public void Set(string name, Hero hero)
    {
        HeroPortrait.sprite = hero.body.ToSprite(hero.atlas);
        PlayerName.text = name;
    }
}