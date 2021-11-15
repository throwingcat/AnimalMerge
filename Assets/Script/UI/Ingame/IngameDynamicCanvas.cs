using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class IngameDynamicCanvas : MonoBehaviour
{
    public GameObject Camera;
    public GameObject Root;
    public PartComboPortrait PlayerPortrait;
    public PartComboPortrait EnemyPortrait;
    public GameObject PlayerSide;
    public GameObject EnemySide;
    
    public void Initialize()
    {
        Camera.SetActive(true);
        Root.SetActive(true);
        
        PlayerPortrait.gameObject.SetActive(true);
        EnemyPortrait.gameObject.SetActive(true);
        PlayerSide.SetActive(false);
        EnemySide.SetActive(false);
        
        PlayerPortrait.Enter();
        EnemyPortrait.Enter();
    }

    public void PlayComboPortrait(int combo, bool isPlayer)
    {
        if (combo < 3) return;
        if (isPlayer)
        {
            PlayerPortrait.Play(combo, () =>
            {
                //PlayerSide.SetActive(false);
            });
            //PlayerSide.SetActive(false);
            //PlayerSide.SetActive(true);
        }
        else
        {
            EnemyPortrait.Play(combo, () =>
            {
                //EnemySide.SetActive(false);
            });
            // EnemySide.SetActive(false);
            // EnemySide.SetActive(true);
        }
    }

    public void Exit()
    {
        PlayerPortrait.gameObject.SetActive(false);
        EnemyPortrait.gameObject.SetActive(false);
        PlayerSide.SetActive(false);
        EnemySide.SetActive(false);
        PlayerPortrait.Leave();
        EnemyPortrait.Leave();
        Camera.SetActive(false);
        Root.SetActive(false);
    }
}
