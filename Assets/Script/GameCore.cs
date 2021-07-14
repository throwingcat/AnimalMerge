using System;
using System.Collections;
using System.Collections.Generic;
using BackEnd;
using Define;
using DG.Tweening;
using SheetData;
using UnityEngine;
using UnityEngine.U2D;
using Violet;
using Violet.Audio;
using Random = UnityEngine.Random;

public class GameCore : MonoSingleton<GameCore>
{
    public bool isGameFinish;
    public bool isPauseBadBlockTimer = false;
    public bool isPauseGameOverTimer = false;
    public int SpawnLevel = 1;

    public void Initialize()
    {
        isGameOver = false;
        isGameFinish = false;
        PlayerScreen.SetActive(true);
        EnemyScreen.gameObject.SetActive(true);
        SetEnableDeadline(false);
        AttackBadBlockValue = 0;
        MyBadBlockValue = 0;
        Score = 0;
        SkillGaugeValue = 0;
        
        SyncManager.Instance.OnSyncCapture = OnCaptureSyncPacket;
        SyncManager.Instance.OnSyncReceive = OnReceiveSyncPacket;

        AudioManager.Instance.ChangeBGMVolume(0.3f);
        AudioManager.Instance.ChangeSFXVolume(0.3f);
        AudioManager.Instance.Play("Sound/bgm", eAUDIO_TYPE.BGM);

        Backend.Match.OnMatchRelay -= SyncManager.Instance.OnReceiveMatchRelay;
        Backend.Match.OnMatchRelay += SyncManager.Instance.OnReceiveMatchRelay;

        //방해 블록 초기화
        _badBlockSheet.Clear();
        
        var table = TableManager.Instance.GetTable<Unit>();
        foreach (var sheet in table)
        {
            var unit = sheet.Value as Unit;
            if (unit != null)
                if (unit.group == "Rat")
                    _badBlockSheet.Add(unit);
        }

        _badBlockSheet.Sort((a, b) =>
        {
            if (a.score < b.score) return 1;
            if (b.score < a.score) return -1;
            return 0;
        });
        MAX_BADBLOCK_VALUE = _badBlockSheet[0].score * 5;

        panelIngame = UIManager.Instance.Show<PanelIngame>();
        panelIngame.RefreshScore(0, 0);
        panelIngame.RefreshSkillGauge(0f);
        RefreshBadBlockUI();
    }

    public void OnUpdate(float delta)
    {
        if (isGameOver == false)
        {
            if (_unit == null && _unitSpawnDelayDelta <= 0f)
                _unit = SpawnUnit();
            else
                _unitSpawnDelayDelta -= delta;

            if (Input.GetKeyDown(KeyCode.Space))
                OnReceiveBadBlock(10);
            if (Input.GetKeyDown(KeyCode.A))
            {
                ChargeSkillGauge(SKILL_CHARGE_VALUE);
                UseSkill();
            }

            InputUpdate();

            MergeUpdate(delta);

            BadBlockUpdate(delta);

            ComboUpdate(delta);

            GameOverUpdate(delta);
        }

        #region Sync

        if (_syncCaptureDelta <= 0f)
        {
            _syncCaptureDelta = EnvironmentValue.SYNC_CAPTURE_DELAY;
            SyncManager.Instance.Capture();
        }
        else
        {
            _syncCaptureDelta -= delta;
        }

        #endregion
    }

    private UnitBase SpawnUnit(string key = "")
    {
        if (key.IsNullOrEmpty())
        {
            var pick = 0;
            switch (SpawnLevel)
            {
                case 1:
                    pick = 1;
                    break;
                case 2:
                    pick = Utils.RandomPick(new List<double> {50, 50}) + 1;
                    break;
                case 3:
                    pick = Utils.RandomPick(new List<double> {34, 33, 33}) + 1;
                    break;
                default:
                    pick = 1;
                    break;
            }

            key = string.Format("cat{0}", Random.Range(1, pick));
        }

        var pool = GameObjectPool.GetPool(key);
        if (pool == null)
            pool = GameObjectPool.CreatePool(key, () =>
            {
                var go = Instantiate(UnitPrefab, UnitParent);
                go.gameObject.SetActive(false);
                return go.gameObject;
            }, 1, UnitParent.gameObject , Key.IngamePoolCategory);

        var unit = pool.Get();
        unit.transform.SetAsLastSibling();
        unit.SetActive(true);

        var component = unit.GetComponent<UnitBase>();

        component.OnSpawn(key)
            .SetPosition(UnitSpawnPosition)
            .SetRotation(Vector3.zero);

        return component;
    }

    private UnitBase SpawnUnit(string key, Vector3 pos)
    {
        var component = SpawnUnit(key);
        component.SetPosition(pos);
        component.Drop();
        return component;
    }

    private void RemoveUnit(UnitBase unit)
    {
        if (unit.eUnitType == eUnitType.Nomral)
            for (var i = 0; i < UnitsInField.Count; i++)
                if (UnitsInField[i].GUID == unit.GUID)
                {
                    UnitsInField.RemoveAt(i);
                    break;
                }

        if (unit.eUnitType == eUnitType.Bad)
            for (var i = 0; i < BadUnits.Count; i++)
                if (BadUnits[i].GUID == unit.GUID)
                {
                    BadUnits.RemoveAt(i);
                    break;
                }

        var pool = GameObjectPool.GetPool(unit.Sheet.key);
        if (pool != null)
            pool.Restore(unit.gameObject);
    }

    private void MergeUpdate(float delta)
    {
        if (0 < _mergeDelayDelta)
        {
            _mergeDelayDelta -= delta;
            return;
        }

        /*for (var i = 0; i < UnitsInField.Count; i++)
        {
            var unit = UnitsInField[i];

            if (unit.eUnitType == eUnitType.Nomral)
            {
                if (IgnoreUnitGUID.Contains(unit.GUID))
                    continue;
                foreach (var friend in UnitsInField)
                {
                    if (IgnoreUnitGUID.Contains(friend.GUID)) continue;
                    if (friend.eUnitType == eUnitType.Bad) continue;
                    if (unit.GUID == friend.GUID) continue;
                    if (unit.Sheet.key != friend.Sheet.key) continue;

                    var distance = Vector3.Distance(unit.transform.localPosition, friend.transform.localPosition);
                    if (distance <= unit.transform.localScale.x * 2 + 10)
                    {
                        isMergeProcess = true;
                        StartCoroutine(MergeProcess(unit, friend));

                        _mergeDelayDelta = EnvironmentValue.MERGE_DELAY;
                        _unitSpawnDelayDelta = EnvironmentValue.UNIT_SPAWN_DELAY;

                        break;
                    }
                }

                if (isMergeProcess)
                    break;
            }
        }
        */
        isMergeProcess = false;
    }

    public void CollisionEnter(UnitBase a, UnitBase b)
    {
        if (IgnoreUnitGUID.Contains(a.GUID) ||
            IgnoreUnitGUID.Contains(b.GUID)) return;

        if (a.Sheet.key == b.Sheet.key)
        {
            StartCoroutine(MergeProcess(a, b));
            _mergeDelayDelta = EnvironmentValue.MERGE_DELAY;
            _unitSpawnDelayDelta = EnvironmentValue.UNIT_SPAWN_DELAY;
        }
    }

    private IEnumerator MergeProcess(UnitBase a, UnitBase b)
    {
        var cached_guid_a = a.GUID;
        var cached_guid_b = b.GUID;
        var cached_pos_a = a.transform.position;
        var cached_pos_b = b.transform.position;

        IgnoreUnitGUID.Add(cached_guid_a);
        IgnoreUnitGUID.Add(cached_guid_b);

        a.PlayMerge();
        b.PlayMerge();

        a.transform.DOScale(a.transform.localScale * 0.2f, 0.3f).SetRelative().SetEase(Ease.OutElastic).Play();
        b.transform.DOScale(a.transform.localScale * 0.2f, 0.3f).SetRelative().SetEase(Ease.OutElastic).Play();
        yield return new WaitForSeconds(0.35f);

        a.transform.position = cached_pos_a;
        b.transform.DOMove(cached_pos_a, 0.15f).SetEase(Ease.OutCubic).Play();
        b.transform.DOLocalRotate(a.transform.localRotation.eulerAngles, 0.15f).SetEase(Ease.OutCubic).Play();
        yield return new WaitForSeconds(0.15f);

        //콤보 출력
        Combo++;
        OnMergeEvent(a, b, Combo);

        RemoveUnit(a);
        RemoveUnit(b);

        var pos = new Vector3(
            (a.transform.localPosition.x + b.transform.localPosition.x) * 0.5f,
            (a.transform.localPosition.y + b.transform.localPosition.y) * 0.5f,
            a.transform.localPosition.z);

        if (a.Sheet.grow_unit.IsNullOrEmpty() == false)
        {
            UnitsInField.Add(SpawnUnit(a.Sheet.grow_unit, pos));

            if (SpawnLevel < a.Sheet.index)
                SpawnLevel = a.Sheet.index;
            SpawnLevel = Mathf.Clamp(SpawnLevel, 1, 3);
        }

        IgnoreUnitGUID.Remove(cached_guid_a);
        IgnoreUnitGUID.Remove(cached_guid_b);
    }

    private void OnMergeEvent(UnitBase a, UnitBase b, int Combo)
    {
        //콤보 출력
        panelIngame.PlayCombo(a.transform.position, Combo);
        _comboDelta = EnvironmentValue.COMBO_DURATION;

        //스코어 갱신
        var gain = (a.Sheet.score + b.Sheet.score) * 10 * Combo;
        OnGainScore(gain);

        //획득 스코어만큼 스킬게이지 충전
        ChargeSkillGauge(gain);
        
        //주변 방해블록 삭제
        var remove_bad_units = new List<UnitBase>();

        foreach (var unit in BadUnits)
        {
            if (IgnoreUnitGUID.Contains(unit.GUID)) continue;

            if (unit.eUnitType == eUnitType.Bad)
            {
                var distance = Vector3.Distance(a.transform.localPosition, unit.transform.localPosition);
                var r1 = a.transform.localScale.x * 1.8f;
                var r2 = unit.transform.localScale.x * 1.8f;
                if (distance <= (r1 + r2))
                {
                    IgnoreUnitGUID.Add(unit.GUID);
                    remove_bad_units.Add(unit);
                }
            }
        }

        for (var i = 0; i < remove_bad_units.Count; i++)
        {
            IgnoreUnitGUID.Remove(remove_bad_units[i].GUID);
            RemoveUnit(remove_bad_units[i]);
            remove_bad_units.RemoveAt(i--);
        }

        int remove_badblock = remove_bad_units.Count;
        
        var comboBonus = Combo > 3 ? 18 * Combo : 0;
        var badBlock = (a.Sheet.score + b.Sheet.score) * Combo + comboBonus + (remove_badblock * 2);
        
        //내 방해블록 제거
        if (0 < MyBadBlockValue)
        {
            MyBadBlockValue -= badBlock;

            //내 방해블록 제거 + 상대방에게 공격
            if (MyBadBlockValue <= 0)
            {
                AttackBadBlockValue = Mathf.Abs(MyBadBlockValue);

                PlayMergeAttackVFX(a.transform.position, panelIngame.MyBadBlockVFXPoint.position, 0.5f, () =>
                {
                    //블록 갱신
                    RefreshBadBlockUI();

                    PlayMergeAttackVFX(panelIngame.MyBadBlockVFXPoint.position,
                        panelIngame.EnemyBadBlockVFXPoint.position,
                        0.3f, () => { });
                });
            }
            //내 방해블록만 제거
            else
            {
                PlayMergeAttackVFX(a.transform.position, panelIngame.MyBadBlockVFXPoint.position, 0.5f,
                    () => { RefreshBadBlockUI(); });
            }
        }
        //상대방에게 공격
        else
        {
            PlayMergeAttackVFX(a.transform.position, panelIngame.EnemyBadBlockVFXPoint.position, 0.5f, () => { });
            AttackBadBlockValue += badBlock;
        }

        //방해블록 타이머 1초 감소
        ReduceBadBlockTimer(1f);
    }

    private void OnGainScore(int gain)
    {
        var before = Score;
        Score += gain;

        panelIngame.RefreshScore(before, Score);
    }

    private void ComboUpdate(float delta)
    {
        if (_comboDelta <= 0f)
            Combo = 0;
        else
            _comboDelta -= delta;
    }

    private void ChargeSkillGauge(int value)
    {
        SkillGaugeValue += value;
        panelIngame.RefreshSkillGauge( (SkillGaugeValue / (float)SKILL_CHARGE_VALUE));
    }
    public void OnLeave()
    {
        PlayerScreen.SetActive(false);
        EnemyScreen.gameObject.SetActive(false);
    }

    #region VFX

    public void PlayMergeAttackVFX(Vector3 from, Vector3 to, float duration, Action onFinish = null)
    {
        var vfx = ResourceManager.Instance.GetUIVFX(Key.VFX_MERGE_ATTACK_TRAIL);
        vfx.transform.position = Utils.WorldToCanvas(Camera.main, from, UIManager.Instance.CanvasRect);
        vfx.SetActive(true);
        to.z = from.z;
        StartCoroutine(PlayBezier(vfx, from, to, duration, () =>
        {
            var bomb = ResourceManager.Instance.GetUIVFX(Key.VFX_MERGE_ATTACK_BOMB);
            bomb.transform.position = to;
            bomb.SetActive(true);

            GameManager.DelayInvoke(() =>
            {
                ResourceManager.Instance.RestoreUIVFX(vfx);
                ResourceManager.Instance.RestoreUIVFX(bomb);
            }, 3f);

            onFinish?.Invoke();
        }));
    }

    #endregion


    #region GameOver

    public GameObject GameOverLine;
    public float GameoverTimeoutDelta;
    public bool isGameOver;
    public DateTime GameOverTime;

    private void GameOverUpdate(float delta)
    {
        if (isGameOver) return;
        if (isPauseGameOverTimer) return;
        
        var isEnable = false;

        foreach (var unit in BadUnits)
        {
            if (unit.eUnitDropState != eUnitDropState.Complete) continue;
            if (GameOverLine.transform.position.y <= unit.transform.position.y)
            {
                isEnable = true;
                break;
            }
        }

        if (isEnable == false)
            foreach (var unit in UnitsInField)
            {
                if (unit.eUnitDropState != eUnitDropState.Complete) continue;
                if (GameOverLine.transform.position.y <= unit.transform.position.y)
                {
                    isEnable = true;
                    break;
                }
            }

        SetEnableDeadline(isEnable);

        if (GameOverLine.activeSelf)
        {
            GameoverTimeoutDelta += delta;
            if (EnvironmentValue.GAME_OVER_TIME_OUT <= GameoverTimeoutDelta)
            {
                isGameOver = true;
                GameOverTime = DateTime.UtcNow;
            }
        }
    }

    public void SetEnableDeadline(bool isEnable)
    {
        if (GameOverLine.activeSelf != isEnable)
        {
            GameOverLine.SetActive(isEnable);
            GameoverTimeoutDelta = 0f;
        }
    }

    #endregion

    #region Timer Value

    private float _mergeDelayDelta;
    private float _unitSpawnDelayDelta;
    private float _syncCaptureDelta;
    private float _comboDelta;

    #endregion

    #region Screen

    public GameObject PlayerScreen;
    public EnemyScreen EnemyScreen;

    #endregion

    #region UI

    public Canvas Canvas;
    private PanelIngame panelIngame;

    #endregion

    #region Unit Value

    private UnitBase _unit;
    public Transform UnitParent;
    public UnitBase UnitPrefab;
    public List<UnitBase> UnitsInField = new List<UnitBase>();
    public List<UnitBase> BadUnits = new List<UnitBase>();
    public Vector3 UnitSpawnPosition;

    #endregion

    #region Player Data

    public int Score;
    public int MAX_BADBLOCK_VALUE;
    public int MyBadBlockValue;
    public int AttackBadBlockValue;
    public bool isMergeProcess;
    public int Combo;
    public List<string> IgnoreUnitGUID = new List<string>();
    private readonly List<Unit> _badBlockSheet = new List<Unit>();
    public const int SKILL_CHARGE_VALUE = 3000;
    public int SkillGaugeValue = 0;
    #endregion

    #region BadBlock

    //방해블록 타이머
    private float _badBlockTimerDelta;

    //방해블록 한줄 최대치
    private const int _badBlockFloorMaxUnit = 6;

    //방해블록 최대 층
    private const int _badBlockMaxFloor = 4;

    private int _badBlockMaxDropOneTime => _badBlockFloorMaxUnit * _badBlockMaxFloor;

    //방해블록 가로영역 크기
    private const float _badBlockWidth = 880;

    //Y축 시작 지점
    private const float _badBlockY = 1050;

    //Y축 증가량
    private const float _badBlockFloorHeight = 150;

    private readonly List<List<Vector3>> Floors = new List<List<Vector3>>();

    
    private void BadBlockUpdate(float delta)
    {
        if (isPauseBadBlockTimer) return;
        
        if (0 < MyBadBlockValue)
        {
            panelIngame.SetActiveBadBlockTimer(true);

            _badBlockTimerDelta += delta;

            panelIngame.UpdateBadBlockTimer(
                EnvironmentValue.BAD_BLOCK_TIMER - _badBlockTimerDelta,
                EnvironmentValue.BAD_BLOCK_TIMER);

            if (EnvironmentValue.BAD_BLOCK_TIMER < _badBlockTimerDelta)
            {
                _badBlockTimerDelta = 0f;

                //쌓인 방해블록 소모 
                var drop = 0;
                if (MyBadBlockValue >= _badBlockMaxDropOneTime)
                {
                    MyBadBlockValue -= _badBlockMaxDropOneTime;
                    drop = _badBlockMaxDropOneTime;
                }
                else
                {
                    drop = MyBadBlockValue;
                    MyBadBlockValue = 0;
                }

                DropBadBlock(drop);
                RefreshBadBlockUI();
            }
        }
        else
        {
            panelIngame.SetActiveBadBlockTimer(false);
        }
    }

    private void ReduceBadBlockTimer(float reduce)
    {
        _badBlockTimerDelta -= reduce;
        if (_badBlockTimerDelta < 0)
            _badBlockTimerDelta = 0;
    }

    private void DropBadBlock(int count)
    {
        if (Floors.Count == 0)
            for (var i = 0; i < _badBlockMaxFloor; i++)
            {
                Floors.Add(new List<Vector3>());
                for (var j = 0; j < _badBlockFloorMaxUnit; j++)
                {
                    var start = -(_badBlockWidth * 0.5f);
                    var spacing = _badBlockWidth / (_badBlockFloorMaxUnit - 1);

                    Floors[i].Add(new Vector3(
                        start + spacing * j,
                        _badBlockY + _badBlockFloorHeight * i,
                        -1f));
                }
            }

        var shuffled_floor = new List<List<Vector3>>();
        foreach (var f in Floors)
            shuffled_floor.Add(Utils.Shuffle(f));

        for (var i = 0; i < count; i++)
        {
            var unit = SpawnUnit("bad");
            var floor = i / _badBlockFloorMaxUnit;
            var index = i % _badBlockFloorMaxUnit;

            unit.eUnitType = eUnitType.Bad;
            unit.SetPosition(shuffled_floor[floor][index]);
            unit.Drop();
            BadUnits.Add(unit);
        }
    }

    #endregion

    #region Input
    private bool isPress;

    private void InputUpdate()
    {
        if (Input.GetMouseButtonDown(0)) OnPress();

        if (isPress && Input.GetMouseButton(0) == false) OnRelease();

        if (isPress)
            if (_unit != null)
            {
                var input_pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                _unit.transform.position =
                    new Vector3(input_pos.x, _unit.transform.position.y, _unit.transform.position.z);

                var horizontalLimit = 540f - EnvironmentValue.UNIT_SPRITE_BASE_SIZE * EnvironmentValue.WORLD_RATIO *
                    _unit.Sheet.size;
                _unit.transform.localPosition =
                    new Vector3(Mathf.Clamp(_unit.transform.localPosition.x, -horizontalLimit, horizontalLimit),
                        _unit.transform.localPosition.y,
                        _unit.transform.localPosition.z);
            }
    }

    private void OnPress()
    {
        isPress = true;
        //
        // var pos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        // //Spawn Ray Cast
        // RaycastHit hit;
        // int layer = LayerMask.GetMask("Unit Spawn Area");
        // if (Physics.Raycast(pos, Vector3.forward, out hit, float.MaxValue , layer))
        // {
        //     isPressSpawnArea = true;
        // }
    }

    private void OnRelease()
    {
        isPress = false;

        if (_unit != null)
        {
            _unit.Drop();
            UnitsInField.Add(_unit);
            _unit = null;
            _unitSpawnDelayDelta = EnvironmentValue.UNIT_SPAWN_DELAY;
        }
    }
    #endregion

    #region Attack

    private void OnReceiveBadBlock(int value)
    {
        MyBadBlockValue += value;
        MyBadBlockValue = Mathf.Clamp(MyBadBlockValue, 0, MAX_BADBLOCK_VALUE);

        RefreshBadBlockUI();
    }

    private void RefreshBadBlockUI()
    {
        var blocks = new List<Unit>();

        var current = MyBadBlockValue;
        foreach (var bad in _badBlockSheet)
        {
            var count = current / bad.score;

            if (0 < count)
                for (var i = 0; i < count; i++)
                    blocks.Add(bad);

            current = current % bad.score;
        }

        panelIngame.RefreshBadBlock(blocks);
    }

    #endregion

    #region Sync

    public void OnCaptureSyncPacket(SyncManager.SyncPacket packet)
    {
    }

    public void OnReceiveSyncPacket(SyncManager.SyncPacket packet)
    {
        RefreshEnemy(packet);

        OnReceiveBadBlock(packet.BadBlockValue);

        if (packet.isGameOver && isGameFinish == false)
            OnReceiveGameOver(packet.isGameOver, packet.GameOverTime);

        RefreshBadBlockUI();
    }

    public void RefreshEnemy(SyncManager.SyncPacket packet)
    {
        EnemyScreen.Refresh(packet);
    }

    public void OnReceiveGameOver(bool isGameOver, DateTime time)
    {
        var isWin = false;
        //나도 게임오버인 경우
        if (this.isGameOver)
        {
            //상대가 더 늦게 죽음
            if (GameOverTime < time)
                //패배
                isWin = false;
            else
                //승리
                isWin = true;
        }
        else
        {
            //승리
            isWin = true;
            //게임 오버 처리해서 플레이 정지
            this.isGameOver = true;
            GameOverTime = time.AddSeconds(100);
        }

        isGameFinish = true;

        var popup = UIManager.Instance.ShowPopup<PopupGameResult>();
        popup.SetResult(isWin);
    }

    #endregion

    #region Utility

    public IEnumerator PlayBezier(GameObject go, Vector3 from, Vector3 to, float duration, Action onFinish)
    {
        var delta = 0f;
        while (delta < duration)
        {
            var t = delta / duration;
            var st = (from - to).normalized;
            var et = (to - from).normalized;
            go.transform.position = BezierUtility.BezierPoint(from, st, et, to, t);

            delta += Time.deltaTime;
            yield return null;
        }

        onFinish?.Invoke();
    }

    public void Clear()
    {
        //전투 풀 모두 삭제
        GameObjectPool.DestroyPools(Key.IngamePoolCategory);
        GameObjectPool.DestroyPools(Key.UIVFXPoolCategory);
        
        //변수 초기화
        isGameOver = false;
        isGameFinish = false;
        UnitsInField.Clear();
        BadUnits.Clear();
        _unit = null;
        AttackBadBlockValue = 0;
        MyBadBlockValue = 0;
        
        //UI 초기화
        PlayerScreen.SetActive(false);
        EnemyScreen.gameObject.SetActive(false);
        SetEnableDeadline(false);

        //이벤트 초기화
        SyncManager.Instance.OnSyncCapture = null;
        SyncManager.Instance.OnSyncReceive = null;
        Backend.Match.OnMatchRelay -= SyncManager.Instance.OnReceiveMatchRelay;
        
        //오디오 종료
        AudioManager.Instance.StopBGM();
    }

    #endregion

    public void UseSkill()
    {
        if (SkillGaugeValue < SKILL_CHARGE_VALUE) return;
        SkillGaugeValue = 0;
        StartCoroutine(RunSkill_Shake());
    }

    public float SHAKE_FORCE = 10f;
    private IEnumerator RunSkill_Shake()
    {
        //방해블록 타이머 퍼지
        isPauseBadBlockTimer = true;
        //게임오버 타이머 퍼지
        isPauseGameOverTimer = true;
        // //모든 방해블록 삭제
        // for (int i = 0; i < BadUnits.Count; i++)
        // {
        //     RemoveUnit(BadUnits[i--]);
        //     yield return null;
        // }
        //모든 블록 위로 튕겨냄
        //모든 방해블록 삭제
        for (int i = 0; i < BadUnits.Count; i++)
        {
            var direction = Vector2.up * SHAKE_FORCE;
            direction.x = Random.Range(-0.3f, 0.3f);
            BadUnits[i].Rigidbody2D.velocity = Vector2.zero;
            BadUnits[i].Rigidbody2D.AddForce(direction);
        }
        foreach (var unit in UnitsInField)
        {
            var direction = Vector2.up * SHAKE_FORCE;
            direction.x = Random.Range(-0.3f, 0.3f);
            unit.Rigidbody2D.velocity = Vector2.zero;
            unit.Rigidbody2D.AddForce(direction);
        }

        isPauseBadBlockTimer = false;
        isPauseGameOverTimer = false;
        
        yield break;
    }
}