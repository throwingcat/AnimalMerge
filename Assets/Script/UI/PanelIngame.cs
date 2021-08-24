using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using SheetData;
using UnityEngine;
using UnityEngine.UI;
using Violet;
using Violet.Audio;

public class PanelIngame : SUIPanel
{
    public DOTweenPlayer EnterPortraitTween;
    public DOTweenPlayer PlayPortraitTween;
    public IngamePlayerPortrait PlayerPortrait;
    public IngamePlayerPortrait EnemyPortrait;
    public RectTransform LayoutLeft;
    public RectTransform LayoutRight;
    public RectTransform LayoutTop;

    public GameObject WaitPlayer;
    public GameObject CountDownRoot;
    public List<GameObject> CountDown;
    private readonly List<IngameBadBlock> _badBlocks = new List<IngameBadBlock>();

    public Transform BadBlockParent;
    public GameObject BadBlockPrefab;
    public Image BadBlockTimer;

    public GameObject BadBlockTimerRoot;
    public Text BadBlockTimerText;

    public Text Score;
    public Transform VFXComboParent;
    public GameObject VFXComboPrefab;

    public Transform MyBadBlockVFXPoint;

    public GameObject SkillRoot;
    public GameObject SkillActivate;
    public Image SkillGauge;
    public Image SkillIcon;

    public Image NextBlock;
    public Image AfterNextBlock;

    public Slider GameOverTimerGauge;

    public GameObject PlayerSkillVFX;
    public Image PassiveSkillGauge;
    public Image PassiveSkillIcon;
    public GameObject PassiveSkillActivate;

    #region Enemy Screen

    private readonly List<IngameBadBlock> _enemyBadBlocks = new List<IngameBadBlock>();
    public Transform EnemyBadBlockParent;
    public Transform EnemyBadBlockVFXPoint;

    #endregion

    protected override void OnShow()
    {
        base.OnShow();
        
        EnterPortraitTween.gameObject.SetActive(true);
    }

    protected override void OnHide()
    {
        base.OnHide();
        
        EnterPortraitTween.SetEnable(false);
        PlayPortraitTween.SetEnable(false);
    }

    public void PlayEnterAnimation()
    {
        StartCoroutine(PlayEnterAnimationProcess());
    }

    private IEnumerator PlayEnterAnimationProcess()
    {
        yield return new WaitForSeconds(3f);
        EnterPortraitTween.SetEnable(true);
        yield return new WaitForSeconds(2f);
        PlayPortraitTween.SetEnable(true);

        LayoutLeft.DOAnchorPos(Vector2.zero, 1f);
        LayoutRight.DOAnchorPos(Vector2.zero, 1f);
        LayoutTop.DOAnchorPos(Vector2.zero, 1f);

        EnterPortraitTween.gameObject.SetActive(false);
    }

    public void RefreshScore(int before, int after)
    {
        var score = before;
        DOTween.To(() => score, x =>
        {
            score = x;
            Score.text = Utils.ParseComma(score);
        }, after, 0.5f);
    }

    public void RefreshBadBlock(List<Unit> blocks)
    {
        var key = "BadBlockPool";
        var pool = GameObjectPool.GetPool(key);
        if (pool == null)
            pool = GameObjectPool.CreatePool(key, () =>
            {
                var go = Instantiate(BadBlockPrefab);
                go.transform.SetParent(BadBlockParent);
                go.transform.LocalReset();
                go.gameObject.SetActive(false);

                return go;
            }, 1, BadBlockParent.gameObject, Define.Key.IngamePoolCategory);

        //이전에 사용한 블록 반납
        foreach (var block in _badBlocks)
        {
            if (block.gameObject)
                pool.Restore(block.gameObject);
        }

        _badBlocks.Clear();

        blocks.Sort((a, b) =>
        {
            if (a.score < b.score) return 1;
            if (b.score < a.score) return -1;
            return 0;
        });

        foreach (var block in blocks)
        {
            var b = pool.Get();
            b.SetActive(true);
            b.transform.LocalReset();
            b.transform.SetAsFirstSibling();

            var component = b.GetComponent<IngameBadBlock>();
            component.Set(block);

            _badBlocks.Add(component);
            if (6 <= _badBlocks.Count) break;
        }
    }

    public void RefreshEnemyBadBlock(int damage)
    {
        var blocks = new List<Unit>();

        var current = damage;
        foreach (var bad in Unit.BadBlocks)
        {
            var count = current / bad.score;

            if (0 < count)
                for (var i = 0; i < count; i++)
                    blocks.Add(bad);

            current = current % bad.score;
        }

        var key = "EnemyBadBlockPool";
        var pool = GameObjectPool.GetPool(key);
        if (pool == null)
            pool = GameObjectPool.CreatePool(key, () =>
            {
                var go = Instantiate(BadBlockPrefab);
                go.transform.SetParent(EnemyBadBlockParent);
                go.transform.LocalReset();
                go.gameObject.SetActive(false);

                return go;
            }, 1, EnemyBadBlockParent.gameObject, Define.Key.IngamePoolCategory);

        //이전에 사용한 블록 반납
        foreach (var block in _enemyBadBlocks)
            pool.Restore(block.gameObject);
        _enemyBadBlocks.Clear();


        blocks.Sort((a, b) =>
        {
            if (a.score < b.score) return 1;
            if (b.score < a.score) return -1;
            return 0;
        });

        foreach (var block in blocks)
        {
            var b = pool.Get();
            b.SetActive(true);
            b.transform.LocalReset();
            b.transform.SetAsFirstSibling();

            var component = b.GetComponent<IngameBadBlock>();
            component.Set(block);

            _enemyBadBlocks.Add(component);
            if (6 <= _enemyBadBlocks.Count) break;
        }
    }

    public void PlayCombo(RectTransform canvas, Vector3 worldPosition, int combo)
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
            }, 1, VFXComboParent.gameObject, Define.Key.IngamePoolCategory);

        var vfx = pool.Get();
        var vfxCombo = vfx.GetComponent<VFXCombo>();
        vfxCombo.Set(combo);
        Utils.WorldToCanvas(ref vfxCombo.RectTransform, Camera.main, worldPosition, canvas);
        vfxCombo.Play();
    }

    public void SetActiveBadBlockTimer(bool isActive)
    {
        if (BadBlockTimerRoot.activeSelf != isActive)
            BadBlockTimerRoot.SetActive(isActive);
    }

    public void UpdateBadBlockTimer(float remain, float max)
    {
        BadBlockTimer.fillAmount = remain / max;
        BadBlockTimerText.text = ((int) remain + 1).ToString();
    }

    public void RefreshSkillGauge(float t)
    {
        SkillGauge.fillAmount = t;
        SkillIcon.color = t <= 1f ? Color.gray : Color.white;
        SkillActivate.SetActive(t >= 1f);
    }

    public void RefreshPassiveSkillGauge(float t)
    {
        PassiveSkillGauge.fillAmount = t;
        PassiveSkillIcon.fillAmount = t;
        PassiveSkillIcon.color = t <= 1f ? Color.gray : Color.white;
        PassiveSkillActivate.SetActive(t >= 1f);
    }

    public Action OnClickSkillEvent;

    public void OnClickSkill()
    {
        OnClickSkillEvent?.Invoke();
    }

    public void RefreshWaitBlocks(string next, string afterNext)
    {
        NextBlock.gameObject.SetActive(false);
        AfterNextBlock.gameObject.SetActive(false);

        NextBlock.sprite = next.ToSprite();
        AfterNextBlock.sprite = afterNext.ToSprite();

        NextBlock.gameObject.SetActive(true);
        AfterNextBlock.gameObject.SetActive(true);
    }

    public void SetActiveGameOverTimer(bool isActive)
    {
        if (GameOverTimerGauge.gameObject.activeSelf != isActive)
            GameOverTimerGauge.gameObject.SetActive(isActive);
    }

    public void SetGameOverTimer(float t)
    {
        GameOverTimerGauge.value = 1f - t;
    }

    public void Clear()
    {
        _badBlocks.Clear();
        _enemyBadBlocks.Clear();
    }

    public void SetCountDown(int index)
    {
        for (int i = 0; i < CountDown.Count; i++)
            CountDown[i].SetActive(i == index);
    }

    public void SetActiveCountDown(bool isActive)
    {
        CountDownRoot.SetActive(isActive);
    }

    public void SetActiveWaitPlayer(bool isActive)
    {
        WaitPlayer.SetActive(isActive);
    }
}