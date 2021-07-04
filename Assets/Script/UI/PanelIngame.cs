using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Violet;
using Violet.Audio;

public class PanelIngame : MonoBehaviour
{
    public GameObject VFXComboPrefab;
    public Transform VFXComboParent;

    public void PlayCombo(Vector3 worldPosition, int combo)
    {
        string path = string.Format("Sound/Combo Sound/Default/combo_{0}",combo);
        AudioManager.Instance.Play(path);

        var pool = GameObjectPool.GetPool("VFXCombo");
        if (pool == null)
        {
            pool = GameObjectPool.CreatePool("VFXCombo", () =>
            {
                var go = Instantiate(VFXComboPrefab, VFXComboParent);
                go.transform.LocalReset();
                go.gameObject.SetActive(false);
                
                return go;
            },1,VFXComboParent.gameObject);
        }

        var vfx = pool.Get();
        var vfxCombo = vfx.GetComponent<VFXCombo>();
        vfxCombo.Set(combo);
        Utils.WorldToCanvas(ref vfxCombo.RectTransform, Camera.main, worldPosition, GameCore.Instance.Canvas.GetComponent<RectTransform>());
        vfxCombo.Play();
    }
}
