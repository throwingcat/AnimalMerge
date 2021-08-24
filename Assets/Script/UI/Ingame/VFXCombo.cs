using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VFXCombo : MonoBehaviour
{
    public RectTransform RectTransform;
    public Text ComboCount;

    public void Set(int combo)
    {
        ComboCount.text = string.Format("x {0}", combo);
    }

    public void Play()
    {
        gameObject.SetActive(true);
        Invoke("Restore",2f);
    }

    private void Restore()
    {
        var pool = GameObjectPool.GetPool("VFXCombo");
        if(pool != null)
            pool.Restore(gameObject);
    }
}
