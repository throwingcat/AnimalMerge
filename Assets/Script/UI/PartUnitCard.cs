using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartUnitCard : MonoBehaviour
{
    public CanvasGroup CardImageGroup;
    public CanvasGroup CardDescriptionGroup;
    public GameObject Root;
    public Image Frame;
    public Image Icon;
    public Text Level;
    public Text Exp;
    public Text Group;
    public Text Name;
    public SlicedFilledImage ExpGauge;

    public void SetTexutre(string texutre)
    {
        Icon.sprite = texutre.ToSprite();
    }

    public void SetName(string name)
    {
        if (Name != null)
            Name.text = name;
    }

    public void SetGroup(string group)
    {
        if (Group != null)
            Group.text = group;
    }

    public void SetLevel(int level)
    {
        if (Level != null)
        {
            if (Level.gameObject.activeSelf == false)
                Level.gameObject.SetActive(true);
            Level.text = string.Format("{0} {1}", "Level".ToLocalization(), level);
        }
    }

    public void SetExp(int current, int max)
    {
        if (Exp != null)
        {
            if (Exp.gameObject.activeSelf == false)
                Exp.gameObject.SetActive(true);
            Exp.text = string.Format("{0}/{1}", current, max);
        }

        if (ExpGauge != null)
        {
            if (ExpGauge.gameObject.activeSelf == false)
                ExpGauge.gameObject.SetActive(true);
            ExpGauge.fillAmount = current / (float) max;
        }
    }
}