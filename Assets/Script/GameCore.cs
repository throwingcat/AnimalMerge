using System;
using System.Collections;
using System.Collections.Generic;
using BackEnd;
using Define;
using DG.Tweening;
using Packet;
using SheetData;
using UnityEngine;
using UnityEngine.U2D;
using Violet;
using Violet.Audio;

public class GameCore : MonoBehaviour
{
    #region Environment

    [Header("환경")] public string INGAME_BGM;

    #endregion

    protected virtual void Initialize()
    {
        #region 데이터 초기화

        isReady = false;
        isLaunchGame = false;
        isGameStart = false;
        isGameOver = false;
        isGameFinish = false;
        AttackBadBlockValue = 0;
        MyBadBlockValue = 0;
        Score = 0;

        _elapsedGameTimer = 0f;
        _mergeDelayDelta = 0f;
        _unitSpawnDelayDelta = 0f;
        _syncCaptureDelta = 0f;
        _comboDelta = 0f;

        SpawnPhase = SpawnPhase.DefaultPhase;
        NextUnitList.Clear();

        //플레이어가 선택한 유닛 그룹 초기화
        PlayerUnitGroupList.Clear();
        var unitTable = TableManager.Instance.GetTable<Unit>();
        foreach (var sheet in unitTable)
        {
            var unit = sheet.Value as Unit;
            if (unit.group.Equals(PlayerUnitGroup))
                PlayerUnitGroupList.Add(unit);
        }

        PlayerUnitGroupList.Sort((a, b) =>
        {
            if (a.index < b.index) return -1;
            if (a.index > b.index) return 1;
            return 0;
        });

        //유닛 큐 초기화
        NextUnitList.Add(PlayerUnitGroupList[0]);
        NextUnitList.Add(PlayerUnitGroupList[0]);

        MAX_BADBLOCK_VALUE = Unit.BadBlocks[0].score * 5;

        switch (PlayerUnitGroup)
        {
            case "Cat":
                Passive = new CatPassive(this);
                break;
        }

        Active = new ActiveShake(this);

        #endregion

        #region 환경 초기화

        SetEnableDeadline(false);

        SyncManager = new SyncManager(this);
        SyncManager.OnSyncCapture = OnCaptureSyncPacket;
        SyncManager.OnSyncReceive = OnReceiveSyncPacket;

        float delay = IsPlayer ? 0.1f : 2f;

        GameManager.DelayInvoke(() =>
        {
            isReady = true;
            MyReadyTime = GameManager.GetTime();
            EnemyReadyTime = new DateTime();
        }, delay);

        #endregion

        #region VFX 풀링

        ResourceManager.Instance.CreateUIVFXPool(Key.VFX_MERGE_ATTACK_TRAIL, 30);
        ResourceManager.Instance.CreateUIVFXPool(Key.VFX_MERGE_ATTACK_BOMB, 30);
        ResourceManager.Instance.CreateUIVFXPool(Key.VFX_MERGE_ATTACK_TRAIL_Red, 30);
        ResourceManager.Instance.CreateUIVFXPool(Key.VFX_MERGE_ATTACK_BOMB_Red, 30);

        #endregion
    }

    public virtual void Initialize(bool isPlayer)
    {
        IsPlayer = isPlayer;

        Initialize();

        //사운드 초기화
        AudioManager.Instance.ChangeBGMVolume(0.3f);
        AudioManager.Instance.ChangeSFXVolume(0.3f);
        AudioManager.Instance.Play(string.Format("Sound/{0}", INGAME_BGM), eAUDIO_TYPE.BGM);

        //인게임 UI 초기화
        if (PlayerScreen != null)
            PlayerScreen.SetActive(true);
        if (EnemyScreen != null)
            EnemyScreen.gameObject.SetActive(true);

        PanelIngame = UIManager.Instance.Show<PanelIngame>();
        PanelIngame.OnClickSkillEvent -= UseSkill;
        PanelIngame.OnClickSkillEvent += UseSkill;
        PanelIngame.RefreshScore(0, 0);
        PanelIngame.RefreshSkillGauge(0f);
        PanelIngame.RefreshPassiveSkillGauge(0f);
        PanelIngame.SetActiveWaitPlayer(true);
        PanelIngame.SetActiveCountDown(false);

        RefreshBadBlockUI();
        ingameDynamicCanvas.Initialize();

        //매치 릴레이 세팅
        if (GameManager.Instance.isSinglePlay == false)
        {
            Backend.Match.OnMatchRelay -= SyncManager.OnReceiveMatchRelay;
            Backend.Match.OnMatchRelay += SyncManager.OnReceiveMatchRelay;
        }
    }

    public virtual void OnUpdate(float delta)
    {
        if (isReady == false) return;
        if (isGameStart)
        {
            if (isGameOver == false)
            {
                _elapsedGameTimer += delta;

                if (CurrentReadyUnit == null && _unitSpawnDelayDelta <= 0f)
                    CurrentReadyUnit = SpawnUnit();
                else
                    _unitSpawnDelayDelta -= delta;

                if (Input.GetKeyDown(KeyCode.Space))
                    OnReceiveBadBlock(10);
                if (Input.GetKeyDown(KeyCode.A))
                {
                    ChargeSkillGauge(EnvironmentValue.SKILL_CHARGE_MAX_VALUE);
                    UseSkill();
                }

                InputUpdate();

                OnUpdatePassiveSkill(delta);

                BadBlockUpdate(delta);

                ComboUpdate(delta);

                GameOverUpdate(delta);
            }
        }

        #region Sync

        if (_syncCaptureDelta <= 0f)
        {
            _syncCaptureDelta = EnvironmentValue.SYNC_CAPTURE_DELAY;
            SyncManager.Capture();
        }
        else
        {
            _syncCaptureDelta -= delta;
        }

        #endregion
    }

    private UnitBase SpawnUnit(string key = "")
    {
        //key가 없는 경우는 기본 생성 로직 
        if (key.IsNullOrEmpty())
        {
            //다음 유닛 리스트에서 하나 빼옴
            key = NextUnitList[0].key;
            NextUnitList.RemoveAt(0);

            //유닛 리스트에 새로운 유닛 추가
            var pick = Utils.RandomPick(SpawnPhase.Phase);
            NextUnitList.Add(PlayerUnitGroupList[pick]);

            //유닛 대기열 UI 갱신
            if (PanelIngame != null)
                PanelIngame.RefreshWaitBlocks(NextUnitList[0].face_texture, NextUnitList[1].face_texture);
        }

        var pool = GameObjectPool.GetPool(key);
        if (pool == null)
        {
            var root = new GameObject(key + "_pool");
            pool = GameObjectPool.CreatePool(key, () =>
            {
                var go = Instantiate(UnitPrefab, root.transform);
                go.gameObject.SetActive(false);
                return go.gameObject;
            }, 1, root, Key.IngamePoolCategory);
        }

        var unit = pool.Get();

        unit.transform.SetParent(UnitParent);
        unit.transform.LocalReset();
        unit.transform.SetAsLastSibling();

        var component = unit.GetComponent<UnitBase>();

        component.OnSpawn(key, CollisionEnter, this)
            .SetPosition(UnitSpawnPosition)
            .SetRotation(Vector3.zero);

        unit.SetActive(true);

        return component;
    }

    private UnitBase SpawnUnit(string key, Vector3 pos)
    {
        var component = SpawnUnit(key);
        component.SetPosition(pos);
        component.Drop();
        return component;
    }

    public void RemoveUnit(UnitBase unit)
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

        unit.OnRemove();
    }

    public void CollisionEnter(UnitBase a, UnitBase b)
    {
        if (IgnoreUnitGUID.Contains(a.GUID) ||
            IgnoreUnitGUID.Contains(b.GUID)) return;

        if (a.Sheet.key == b.Sheet.key)
        {
            StartCoroutine(MergeProcess(a, b));
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

        var pos = new Vector3(
            (a.transform.localPosition.x + b.transform.localPosition.x) * 0.5f,
            (a.transform.localPosition.y + b.transform.localPosition.y) * 0.5f,
            a.transform.localPosition.z);

        if (a.Sheet.grow_unit.IsNullOrEmpty() == false)
            UnitsInField.Add(SpawnUnit(a.Sheet.grow_unit, pos));

        RemoveUnit(a);
        RemoveUnit(b);
        IgnoreUnitGUID.Remove(cached_guid_a);
        IgnoreUnitGUID.Remove(cached_guid_b);
    }

    private void OnMergeEvent(UnitBase a, UnitBase b, int Combo)
    {
        _comboDelta = EnvironmentValue.COMBO_DURATION;

        if (IsPlayer)
        {
            //콤보 출력
            PanelIngame.PlayCombo(Canvas.GetComponent<RectTransform>(), a.transform.position, Combo);
            //콤보 초상화 출력
            ingameDynamicCanvas.PlayComboPortrait(Combo, true);
        }

        //유닛 생성 페이즈 변경 확인
        if (SpawnPhase.GrowCondition <= a.Sheet.index)
            SpawnPhase = SpawnPhase.GetNextPhase();

        //패시브 스킬 발동
        if (3 <= Combo)
            Passive?.Run(OnCompletePassiveSkill);

        AttackComboValue = Combo;

        //스코어 갱신
        var gain = (a.Sheet.score + b.Sheet.score) * 10 * Combo;
        OnGainScore(gain);

        //획득 스코어만큼 스킬게이지 충전
        ChargeSkillGauge(gain);

        var remove_bad_units = new List<UnitBase>();

        //주변 방해블록 삭제
        foreach (var unit in BadUnits)
        {
            if (IgnoreUnitGUID.Contains(unit.GUID)) continue;

            if (unit.eUnitType == eUnitType.Bad)
            {
                var distance = Vector3.Distance(a.transform.localPosition, unit.transform.localPosition);
                var r1 = a.transform.localScale.x * 1.8f;
                var r2 = unit.transform.localScale.x * 1.8f;
                if (distance <= r1 + r2)
                {
                    IgnoreUnitGUID.Add(unit.GUID);
                    remove_bad_units.Add(unit);
                }
            }
        }

        for (var i = 0; i < remove_bad_units.Count; i++)
        {
            IgnoreUnitGUID.Remove(remove_bad_units[i].GUID);
            RemoveBadBlock(remove_bad_units[i]);
            remove_bad_units.RemoveAt(i--);
        }

        remove_bad_units.Clear();

        var comboBonus = Combo > 2 ? 18 * Combo : 0;
        var unitDamage = Utils.GetUnitDamage(a.Sheet.score, a.Info.Level);
        int badBlock = (int) ((unitDamage * Combo + comboBonus) * SuddenDeathRatio(_elapsedGameTimer));

        //내 방해블록 제거
        if (0 < MyBadBlockValue)
        {
            MyBadBlockValue -= badBlock;

            //내 방해블록 제거 + 상대방에게 공격
            if (MyBadBlockValue <= 0)
            {
                AttackBadBlockValue = Mathf.Abs(MyBadBlockValue);

                if (IsPlayer)
                    PlayMergeAttackVFX(a.transform.position, PanelIngame.MyBadBlockVFXPoint.position, 0.5f, () =>
                    {
                        //블록 갱신
                        RefreshBadBlockUI();

                        PlayMergeAttackVFX(PanelIngame.MyBadBlockVFXPoint.position,
                            PanelIngame.EnemyBadBlockVFXPoint.position,
                            0.5f, () => { });
                    });
            }
            //내 방해블록만 제거
            else
            {
                if (IsPlayer)
                    PlayMergeAttackVFX(a.transform.position, PanelIngame.MyBadBlockVFXPoint.position, 0.5f,
                        () => { RefreshBadBlockUI(); });
            }
        }
        //상대방에게 공격
        else
        {
            if (IsPlayer)
                PlayMergeAttackVFX(a.transform.position, PanelIngame.EnemyBadBlockVFXPoint.position, 0.5f, () => { });

            AttackBadBlockValue += badBlock;
        }

        //방해블록 타이머 1초 감소
        ReduceBadBlockTimer(1f);
        //게임오버 타이머 1초 감소
        ReduceGameOverTimer(1f);
    }

    public void RemoveBadBlock(UnitBase unit)
    {
        int damage = (int) (5 * SuddenDeathRatio(_elapsedGameTimer));
        PlayRemoveBadUnitDamage(damage, unit);
        RemoveUnit(unit);
    }

    private void PlayRemoveBadUnitDamage(int damage, UnitBase unit)
    {
        //내 방해블록 제거
        if (0 < MyBadBlockValue)
        {
            MyBadBlockValue -= damage;

            //내 방해블록 제거 + 상대방에게 공격
            if (MyBadBlockValue <= 0)
            {
                AttackBadBlockValue = Mathf.Abs(MyBadBlockValue);

                if (IsPlayer)
                    PlayMergeAttackVFX(unit.transform.position, PanelIngame.MyBadBlockVFXPoint.position, 0.5f, () =>
                    {
                        //블록 갱신
                        RefreshBadBlockUI();

                        PlayMergeAttackVFX(PanelIngame.MyBadBlockVFXPoint.position,
                            PanelIngame.EnemyBadBlockVFXPoint.position,
                            0.5f, () => { });
                    });
            }
            //내 방해블록만 제거
            else
            {
                if (IsPlayer)
                    PlayMergeAttackVFX(unit.transform.position, PanelIngame.MyBadBlockVFXPoint.position, 0.5f,
                        () => { RefreshBadBlockUI(); });
            }
        }
        //상대방에게 공격
        else
        {
            if (IsPlayer)
                PlayMergeAttackVFX(unit.transform.position, PanelIngame.EnemyBadBlockVFXPoint.position, 0.5f,
                    () => { });

            AttackBadBlockValue += damage;
        }
    }

    private void OnGainScore(int gain)
    {
        var before = Score;
        Score += gain;

        if (IsPlayer)
            PanelIngame.RefreshScore(before, Score);
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
        Active.Charge(value);
        if (IsPlayer)
            PanelIngame.RefreshSkillGauge(Active.Progress);
    }

    public void OnLeave()
    {
        if (PlayerScreen)
        {
            PlayerScreen.SetActive(false);
            EnemyScreen.gameObject.SetActive(false);
        }
    }

    #region VFX

    public void PlayMergeAttackVFX(Vector3 from, Vector3 to, float duration, Action onFinish = null)
    {
        var vfx = ResourceManager.Instance.GetUIVFX(Key.VFX_MERGE_ATTACK_TRAIL);

        from.z = UIManager.Instance.GetLayer(UIManager.eUILayer.VFX).transform.position.z;
        to.z = from.z;
        //vfx.transform.position = Utils.WorldToCanvas(Camera.main, from, UIManager.Instance.CanvasRect);
        vfx.transform.position = from;

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
        vfx.SetActive(true);
    }

    #endregion

    public void UseSkill()
    {
        Active.Run();
        if (IsPlayer)
            PanelIngame.RefreshSkillGauge(0f);
    }

    #region System

    [Header("시스템")] public DateTime MyReadyTime;
    public DateTime EnemyReadyTime;
    public DateTime GameStartTime;
    public bool isReady;
    public bool isLaunchGame;
    public bool isGameStart;
    public bool isGameFinish;
    public bool isPauseBadBlockTimer;
    public SpawnPhase SpawnPhase;
    public string PlayerUnitGroup = "Cat";
    public List<Unit> PlayerUnitGroupList = new List<Unit>();
    public bool IsPlayer = true;
    public SyncManager SyncManager;
    [Header("시스템 - 게임오버")] public GameObject GameOverLine;

    public float GameoverTimeoutDelta;
    public bool isGameOver;
    public DateTime GameOverTime;

    #endregion

    #region UI

    [Header("UI")] public Canvas Canvas;

    public PanelIngame PanelIngame;

    public IngameDynamicCanvas ingameDynamicCanvas;

    #endregion

    #region GameOver

    private void GameOverUpdate(float delta)
    {
        if (isGameOver) return;

        if (GameManager.ContainsTimer(Key.SIMPLE_TIMER_RUNNING_SKILL)) return;

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
        if (IsPlayer)
            PanelIngame.SetActiveGameOverTimer(isEnable);
        if (GameOverLine.activeSelf)
        {
            GameoverTimeoutDelta += delta;
            if (EnvironmentValue.GAME_OVER_TIME_OUT <= GameoverTimeoutDelta)
            {
                isGameOver = true;
                GameOverTime = DateTime.UtcNow;
            }

            if (IsPlayer)
                PanelIngame.SetGameOverTimer(GameoverTimeoutDelta / EnvironmentValue.GAME_OVER_TIME_OUT);
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

    private void ReduceGameOverTimer(float delta)
    {
        GameoverTimeoutDelta -= delta;
        if (GameoverTimeoutDelta <= 0f)
            GameoverTimeoutDelta = 0f;
    }

    #endregion

    #region Timer Value

    private float _elapsedGameTimer;
    private float _mergeDelayDelta;
    private float _unitSpawnDelayDelta;
    private float _syncCaptureDelta;
    private float _comboDelta;

    #endregion

    #region Screen

    public GameObject PlayerScreen;
    public EnemyScreen EnemyScreen;

    #endregion

    #region Unit Value

    protected UnitBase CurrentReadyUnit;
    [Header("유닛")] public Transform UnitParent;
    public UnitBase UnitPrefab;
    public List<UnitBase> UnitsInField = new List<UnitBase>();
    public List<UnitBase> BadUnits = new List<UnitBase>();
    public Vector3 UnitSpawnPosition;
    public List<Unit> NextUnitList = new List<Unit>();

    #endregion

    #region Player Data

    [Header("플레이어 데이터")] public int Score;

    public int MAX_BADBLOCK_VALUE;
    public int MyBadBlockValue;
    public int AttackBadBlockValue;
    public int AttackComboValue;
    public int Combo;
    public List<ulong> IgnoreUnitGUID = new List<ulong>();

    public PassiveBase Passive;
    public ActiveBase Active;

    #endregion

    #region BadBlock

    //방해블록 타이머
    private float _badBlockTimerDelta;

    private int _badBlockMaxDropOneTime => EnvironmentValue.BAD_BLOCK_HORIZONTAL_MAX_COUNT *
                                           EnvironmentValue.BAD_BLOCK_VERTICAL_MAX_COUNT;

    private readonly List<List<Vector3>> Floors = new List<List<Vector3>>();


    private void BadBlockUpdate(float delta)
    {
        if (isPauseBadBlockTimer) return;

        if (0 < MyBadBlockValue)
        {
            if (IsPlayer)
                PanelIngame.SetActiveBadBlockTimer(true);

            _badBlockTimerDelta += delta;

            if (IsPlayer)
                PanelIngame.UpdateBadBlockTimer(
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
            if (IsPlayer)
                PanelIngame.SetActiveBadBlockTimer(false);
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
            for (var i = 0; i < EnvironmentValue.BAD_BLOCK_VERTICAL_MAX_COUNT; i++)
            {
                Floors.Add(new List<Vector3>());
                for (var j = 0; j < EnvironmentValue.BAD_BLOCK_HORIZONTAL_MAX_COUNT; j++)
                {
                    var start = -(EnvironmentValue.BAD_BLOCK_AREA_WIDTH * 0.5f);
                    var spacing = EnvironmentValue.BAD_BLOCK_AREA_WIDTH /
                                  (EnvironmentValue.BAD_BLOCK_HORIZONTAL_MAX_COUNT - 1);

                    Floors[i].Add(new Vector3(
                        start + spacing * j,
                        EnvironmentValue.BAD_BLOCK_SPAWN_Y + EnvironmentValue.BAD_BLOCK_VERTICAL_OFFSET * i,
                        -1f));
                }
            }

        var shuffled_floor = new List<List<Vector3>>();
        foreach (var f in Floors)
            shuffled_floor.Add(Utils.Shuffle(f));

        for (var i = 0; i < count; i++)
        {
            var unit = SpawnUnit("bad");
            var floor = i / EnvironmentValue.BAD_BLOCK_HORIZONTAL_MAX_COUNT;
            var index = i % EnvironmentValue.BAD_BLOCK_HORIZONTAL_MAX_COUNT;

            unit.eUnitType = eUnitType.Bad;
            unit.SetPosition(shuffled_floor[floor][index]);
            unit.Drop(true);
            BadUnits.Add(unit);
        }
    }

    #endregion

    #region Input

    private bool isPress;

    protected virtual void InputUpdate()
    {
        if (Input.GetMouseButtonDown(0)) OnPress();

        if (isPress && Input.GetMouseButton(0) == false) OnRelease();

        if (isPress)
            if (CurrentReadyUnit != null)
            {
                var input_pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                CurrentReadyUnit.transform.position =
                    new Vector3(input_pos.x, CurrentReadyUnit.transform.position.y,
                        CurrentReadyUnit.transform.position.z);

                var horizontalLimit = 540f - EnvironmentValue.UNIT_SPRITE_BASE_SIZE * EnvironmentValue.WORLD_RATIO *
                    CurrentReadyUnit.Sheet.size;
                CurrentReadyUnit.transform.localPosition =
                    new Vector3(
                        Mathf.Clamp(CurrentReadyUnit.transform.localPosition.x, -horizontalLimit, horizontalLimit),
                        CurrentReadyUnit.transform.localPosition.y,
                        CurrentReadyUnit.transform.localPosition.z);
            }
    }

    protected void OnPress()
    {
        isPress = true;
    }

    protected void OnRelease()
    {
        isPress = false;

        if (CurrentReadyUnit != null)
        {
            CurrentReadyUnit.Drop(true);
            UnitsInField.Add(CurrentReadyUnit);
            CurrentReadyUnit = null;
            _unitSpawnDelayDelta = EnvironmentValue.UNIT_SPAWN_DELAY;
        }
    }

    #endregion

    #region Attack

    private void OnReceiveBadBlock(int value)
    {
        if (value == 0) return;

        MyBadBlockValue += value;
        MyBadBlockValue = Mathf.Clamp(MyBadBlockValue, 0, MAX_BADBLOCK_VALUE);

        if (IsPlayer)
        {
            var trail = ResourceManager.Instance.GetUIVFX(Key.VFX_MERGE_ATTACK_TRAIL_Red);
            var from = PanelIngame.EnemyBadBlockVFXPoint.position;
            var to = PanelIngame.MyBadBlockVFXPoint.position;
            trail.transform.position = from;
            trail.SetActive(true);

            trail.transform.DOMove(to, 0.5f).SetEase(Ease.InQuart).OnComplete(() =>
            {
                var bomb = ResourceManager.Instance.GetUIVFX(Key.VFX_MERGE_ATTACK_BOMB_Red);
                bomb.transform.position = to;
                bomb.SetActive(true);
                GameManager.DelayInvoke(() =>
                {
                    ResourceManager.Instance.RestoreUIVFX(trail);
                    ResourceManager.Instance.RestoreUIVFX(bomb);
                }, 3f);

                RefreshBadBlockUI();
            }).Play();
        }
    }

    private void OnReceiveCombo(int combo)
    {
        if (combo == 0) return;
        if (IsPlayer)
            ingameDynamicCanvas.PlayComboPortrait(combo, false);
    }

    private void RefreshBadBlockUI()
    {
        var blocks = new List<Unit>();

        var current = MyBadBlockValue;
        foreach (var bad in Unit.BadBlocks)
        {
            var count = current / bad.score;

            if (0 < count)
                for (var i = 0; i < count; i++)
                    blocks.Add(bad);

            current = current % bad.score;
        }

        if (IsPlayer)
            PanelIngame.RefreshBadBlock(blocks);
    }

    #endregion

    #region Skill

    private readonly float PASSIVE_SKILL_COOL_TIME = 20f;

    private void OnUpdatePassiveSkill(float delta)
    {
        Passive?.OnUpdate(delta);

        if (IsPlayer)
            if (Passive != null)
            {
                PanelIngame.RefreshPassiveSkillGauge(1f - Passive.CoolTimeProgress);

                if (Input.GetKeyDown(KeyCode.W))
                    Passive.Run(OnCompletePassiveSkill);
            }
    }

    private void OnCompletePassiveSkill()
    {
        //스킬 발동 성공

        if (PanelIngame != null)
            PanelIngame.RefreshPassiveSkillGauge(0f);
    }

    #endregion

    private IEnumerator GameStartProcess()
    {        
        if (IsPlayer)
        {
            PanelIngame.SetActiveWaitPlayer(false);
            yield return new WaitForSeconds(1f);
            if (IsPlayer)
                PanelIngame.PlayEnterAnimation();
            yield return new WaitForSeconds(1f);
            PanelIngame.SetActiveCountDown(true);
        }
        else
            yield return new WaitForSeconds(2f);

        float duration = (float) (GameStartTime - GameManager.GetTime()).TotalSeconds;
        float elapsed = 0f;

        int _prevIndex = -1;
        while (elapsed <= duration)
        {
            elapsed += Time.deltaTime;

            if (IsPlayer)
            {
                //0 ~ 2
                int index = (int) elapsed;

                if (_prevIndex != index)
                {
                    PanelIngame.SetCountDown(index);
                    _prevIndex = index;
                }
            }

            yield return null;
        }

        isGameStart = true;

        if (IsPlayer)
        {
            PanelIngame.SetCountDown(3);
            yield return new WaitForSeconds(0.5f);
            PanelIngame.SetActiveCountDown(false);
        }
    }

    #region Sync

    public void OnCaptureSyncPacket(SyncManager.SyncPacket packet)
    {
    }

    public void OnReceiveSyncPacket(SyncManager.SyncPacket packet)
    {
        if (isReady && isLaunchGame == false)
        {
            EnemyReadyTime = packet.ReadyTime;
            GameStartTime = MyReadyTime < EnemyReadyTime ? EnemyReadyTime : MyReadyTime;
            GameStartTime = GameStartTime.AddSeconds(6);

            StartCoroutine(GameStartProcess());

            isLaunchGame = true;
        }

        OnReceiveBadBlock(packet.AttackDamage);
        OnReceiveCombo(packet.AttackCombo);
        if (IsPlayer)
        {
            RefreshEnemy(packet);
            PanelIngame.RefreshEnemyBadBlock(packet.StackDamage);
            RefreshBadBlockUI();
        }

        if (packet.isGameOver && isGameFinish == false)
            OnReceiveGameOver(packet.isGameOver, packet.GameOverTime);
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

        if (IsPlayer)
        {
            var packet = new PacketBase();
            packet.PacketType = ePACKET_TYPE.REPORT_GAME_RESULT;
            packet.hash.Add("is_win", isWin);

            var beforeScore = PlayerInfo.Instance.RankScore;
            NetworkManager.Instance.Request(packet, res =>
            {
                var popup = UIManager.Instance.ShowPopup<PopupGameResult>();
                popup.SetResult(isWin, beforeScore);
            });
        }
    }

    #endregion

    #region Utility

    public IEnumerator PlayBezier(GameObject go, Vector3 from, Vector3 to, float duration, Action onFinish)
    {
        var delta = 0f;
        while (delta < duration)
        {
            var t = delta / duration;
            var st = (from - to) / 2;
            var et = (to - from) / 2;
            st.z = go.transform.position.z;
            et.z = st.z;
            go.transform.position = BezierUtility.BezierPoint(from, st, et, to, t);

            delta += Time.deltaTime;
            yield return null;
        }

        onFinish?.Invoke();
    }

    public virtual void Clear()
    {
        //전투 풀 모두 삭제
        GameObjectPool.DestroyPools(Key.IngamePoolCategory);
        GameObjectPool.DestroyPools(Key.UIVFXPoolCategory);

        //변수 초기화
        isGameOver = false;
        isGameFinish = false;
        UnitsInField.Clear();
        BadUnits.Clear();
        CurrentReadyUnit = null;
        AttackBadBlockValue = 0;
        MyBadBlockValue = 0;

        //UI 초기화
        if (IsPlayer)
        {
            PlayerScreen.SetActive(false);
            EnemyScreen.gameObject.SetActive(false);
            SetEnableDeadline(false);
            PanelIngame.Clear();
            ingameDynamicCanvas.Exit();
            //이벤트 초기화
            SyncManager.OnSyncCapture = null;
            SyncManager.OnSyncReceive = null;
            Backend.Match.OnMatchRelay -= SyncManager.OnReceiveMatchRelay;

            //오디오 종료
            AudioManager.Instance.StopBGM();
        }
    }

    public decimal SuddenDeathRatio(float elapsed)
    {
        return (decimal) (1 + EnvironmentValue.DAMAGE_PER_SECOND * elapsed);
    }

    #endregion
}