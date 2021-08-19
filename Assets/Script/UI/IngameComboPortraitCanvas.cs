using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngameComboPortraitCanvas : MonoBehaviour
{
    public GameObject Camera;
    public GameObject Root;
    public PartComboPortrait PlayerComboPortrait;
    public PartComboPortrait EnemyComboPortrait;
    public GameObject PlayerSide;
    public GameObject EnemySide;

    public void Initialize()
    {
        Camera.SetActive(true);
        Root.SetActive(true);
        
        PlayerComboPortrait.gameObject.SetActive(true);
        EnemyComboPortrait.gameObject.SetActive(true);
        PlayerSide.SetActive(false);
        EnemySide.SetActive(false);
        
        PlayerComboPortrait.Enter();
        EnemyComboPortrait.Enter();
    } 
    
    public void PlayComboPortrait(int combo, bool isPlayer)
    {
        if (combo < 3) return;
        if (isPlayer)
        {
            PlayerComboPortrait.Play(combo, () =>
            {
                PlayerSide.SetActive(false);
            });
            PlayerSide.SetActive(false);
            PlayerSide.SetActive(true);
        }
        else
        {
            EnemyComboPortrait.Play(combo, () =>
            {
                EnemySide.SetActive(false);
            });
            EnemySide.SetActive(false);
            EnemySide.SetActive(true);
        }
    }

    public void Exit()
    {
        PlayerComboPortrait.gameObject.SetActive(false);
        EnemyComboPortrait.gameObject.SetActive(false);
        PlayerSide.SetActive(false);
        EnemySide.SetActive(false);
        PlayerComboPortrait.Leave();
        EnemyComboPortrait.Leave();
        Camera.SetActive(false);
        Root.SetActive(false);
    }
}
