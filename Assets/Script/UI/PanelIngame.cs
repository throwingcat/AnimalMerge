using System.Collections.Generic;
using DG.Tweening;
using SheetData;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;
using Violet;
using Violet.Audio;

public class PanelIngame : MonoBehaviour
{
    public Transform VFXComboParent;
    public GameObject VFXComboPrefab;

    public Transform BadBlockParent;
    public GameObject BadBlockPrefab;

    public Text Score;
    private List<IngameBadBlock> _badBlocks = new List<IngameBadBlock>();

    public void RefreshScore(int before,int after)
    {
        int score = before;
        DOTween.To(() => score, x =>
        {
            score = x;
            Score.text = Utils.ParseComma(score);
        }, after, 0.5f);
    }
    public void RefreshBadBlock(List<Unit> blocks)
    {
        string key = "BadBlockPool";
        var pool = GameObjectPool.GetPool(key);
        if (pool == null)
        {
            pool = GameObjectPool.CreatePool(key, () =>
            {
                var go = Instantiate(BadBlockPrefab);
                go.transform.SetParent(BadBlockParent);
                go.transform.LocalReset();
                go.gameObject.SetActive(false);

                return go;
            },1,BadBlockParent.gameObject);
        }

        //이전에 사용한 블록 반납
        foreach (var block in _badBlocks)
            pool.Restore(block.gameObject);
        _badBlocks.Clear();

        blocks.Sort((a, b) =>
        {
            if (a.score < b.score) return -1;
            if (b.score < a.score) return 1;
            return 0;
        });
        
        foreach (var block in blocks)
        {
            var b = pool.Get();
            b.SetActive(true);
            b.transform.LocalReset();
            b.transform.SetAsLastSibling();
            
            var component = b.GetComponent<IngameBadBlock>();
            component.Set(block);
            
            _badBlocks.Add(component);
        }
        
    }
    public void PlayCombo(Vector3 worldPosition, int combo)
    {
        var path = string.Format("Sound/Combo Sound/Default/combo_{0}", Mathf.Clamp(combo, 1, 11));
        AudioManager.Instance.Play(path);

        var pool = GameObjectPool.GetPool("VFXCombo");
        if (pool == null)
            pool = GameObjectPool.CreatePool("VFXCombo", () =>
            {
                var go = Instantiate(VFXComboPrefab, VFXComboParent);
                go.transform.LocalReset();
                go.gameObject.SetActive(false);

                return go;
            }, 1, VFXComboParent.gameObject);

        var vfx = pool.Get();
        var vfxCombo = vfx.GetComponent<VFXCombo>();
        vfxCombo.Set(combo);
        Utils.WorldToCanvas(ref vfxCombo.RectTransform, Camera.main, worldPosition,
            GameCore.Instance.Canvas.GetComponent<RectTransform>());
        vfxCombo.Play();
    }
}