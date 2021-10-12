using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MToggle : MonoBehaviour
{
    public GameObject Enable;
    public GameObject Disable;

    void Reset()
    {
        Enable = transform.Find("Enable").gameObject;
        Disable = transform.Find("Disable").gameObject;
    }

    public void SetEnable(bool isEanble)
    {
        Enable.SetActive(isEanble);
        Disable.SetActive(!isEanble);
    }
}