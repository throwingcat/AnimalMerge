using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngameComboPortraitCanvas : MonoBehaviour
{
    public GameObject Camera;
    public GameObject Root;
    public PartComboPortrait PlayerComboPortrait;
    public PartComboPortrait EnemyComboPortrait;

    private float _elapsed = 0f;
    private float _duration = 0f;
private bool isEnable = false;
    public void Initialize()
    {
        PlayerComboPortrait.gameObject.SetActive(false);
        EnemyComboPortrait.gameObject.SetActive(false);
    } 
    public void PlayComboPortrait(int combo, bool isPlayer)
    {
        if (combo < 3) return;
        if(isPlayer)
            PlayerComboPortrait.Play(combo);
        else
            EnemyComboPortrait.Play(combo);

        isEnable = true;
        _elapsed = 0f;
        _duration = 3f;
        
        Camera.SetActive(true);
        Root.SetActive(true);
    }

    public void Update()
    {
        if (isEnable)
        {
            if (_duration <= _elapsed)
            {
                isEnable = false;
                Camera.SetActive(false);
                Root.SetActive(false);
            }
        }
        _elapsed += Time.deltaTime;
    }

    public void Exit()
    {
        PlayerComboPortrait.gameObject.SetActive(false);
        EnemyComboPortrait.gameObject.SetActive(false);
        Camera.SetActive(false);
        Root.SetActive(false);
    }
}
