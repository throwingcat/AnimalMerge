using System;
using System.Collections;
using System.Collections.Generic;
using BackEnd;
using Common;
using Define;
using DG.Tweening;
using Newtonsoft.Json;
using Packet;
using SheetData;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.U2D;
using Violet;
using Violet.Audio;

public class GameCore : MonoBehaviour
{
    #region Environment

    [Header("환경")] public string INGAME_BGM;
    public GameObject BattleCameraGroup;

    #endregion

    private PlayerInfo PlayerInfo => PlayerDataManager.Get<PlayerInfo>();
    protected float HorizontalSpawnLimit =>
        (float) (500f - EnvironmentValue.UNIT_BASE_SIZE * CurrentReadyUnit.Sheet.size *
            EnvironmentValue.WORLD_RATIO);

    protected virtual void Initialize()
    {
        #region 데이터 초기화

        if (IsPlayer)
        {
            PlayerHeroKey = PlayerInfo.elements.SelectHero;
        }

        isReady = false;
        isLaunchGame = false;
        isGameStart = false;
        MyReadyTime = new SubscribeValue<DateTime>(new DateTime());
        isGameOver = new SubscribeValue<bool>(false);
        AttackDamage = new SubscribeValue<int>(0);
        MyStackDamage = new SubscribeValue<int>(0);
        AttackComboValue = new SubscribeValue<int>(0);
        Score = 0;

        _elapsedGameTimer = 0f;
        _mergeDelayDelta = 0f;
        _unitSpawnDelayDelta = 0f;
        _syncCaptureDelta = 0f;
        _comboDelta = 0f;

        SpawnPhase = SpawnPhase.DefaultPhase;
        NextUnitList.Clear();

        PlayerBattleTracker.Clear();
        //플레이어가 선택한 유닛 그룹 초기화
        PlayerUnitGroupList.Clear();
        var unitTable = TableManager.Instance.GetTable<Unit>();
        foreach (var sheet in unitTable)
        {
            var unit = sheet.Value as Unit;
            if (unit.master.Equals(PlayerHeroKey))
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

        switch (PlayerHeroKey)
        {
            default:
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

        isReady = true;
        GameManager.DelayInvoke(() =>
        {
            MyReadyTime.Set(GameManager.GetTime());
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

        //플레이어 정보 전송
        SyncManager.PlayerInfo playerInfo = new SyncManager.PlayerInfo();
        playerInfo.HeroKey = PlayerHeroKey;
        playerInfo.MMR = PlayerInfo.elements.RankScore;
        playerInfo.Name = PlayerInfo.elements.Nickname;
        SyncManager.Request(playerInfo);

        //게임 카메라 루트 활성화
        BattleCameraGroup.SetActive(true);

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
        PanelIngame.SetPlayerPortrait(PlayerInfo.elements.Nickname, PlayerHeroKey.ToTableData<Hero>());
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
            if (isGameOver.Value == false)
            {
                _elapsedGameTimer += delta;

                if (CurrentReadyUnit == null && _unitSpawnDelayDelta <= 0f)
                    CurrentReadyUnit = SpawnUnit();
                else
                    _unitSpawnDelayDelta -= delta;

                if (Input.GetKeyDown(KeyCode.P))
                    GameOver(GameManager.GetTime());
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
                PanelIngame.RefreshWaitBlocks(NextUnitList[0], NextUnitList[1]);
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
        Utils.SetLayer(UnitReadyLayer, unit.gameObject);

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
                    if (IsPlayer)
                        PlayerBattleTracker.Update(PlayerTracker.REMOVE_BAD_BLOCK, 1);
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
        OnBeforeMergeEvent();

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

        //콤보 상승
        Combo++;

        if (IsPlayer)
        {
            if (Combo <= 3)
                PlayerBattleTracker.Update(PlayerTracker.COMBO3, 1);
            if (Combo <= 5)
                PlayerBattleTracker.Update(PlayerTracker.COMBO5, 1);
            if (Combo <= 7)
                PlayerBattleTracker.Update(PlayerTracker.COMBO7, 1);
            if (Combo <= 10)
                PlayerBattleTracker.Update(PlayerTracker.COMBO10, 1);
            PlayerBattleTracker.UpdateMax(PlayerTracker.MAX_COMBO, Combo);
        }

        //콤보 출력
        OnAfterMergeEvent(a, b, Combo);

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

    private void OnBeforeMergeEvent()
    {
        //방해블록 타이머 1초 감소
        ReduceBadBlockTimer(EnvironmentValue.RECOVERY_BAD_BLOCK_TIMER);
        //게임오버 타이머 1초 감소
        ReduceGameOverTimer(EnvironmentValue.RECOVERY_GAMEOVER_TIMER);
    }

    private void OnAfterMergeEvent(UnitBase a, UnitBase b, int Combo)
    {
        _comboDelta = EnvironmentValue.COMBO_DURATION;

        if (IsPlayer)
        {
            //콤보 출력
            PanelIngame.PlayCombo(Canvas.GetComponent<RectTransform>(), a.transform.position, Combo);
            //콤보 초상화 출력
            ingameDynamicCanvas.PlayComboPortrait(Combo, true);

            PlayerBattleTracker.Update(PlayerTracker.MERGE_COUNT, 1);
        }

        //유닛 생성 페이즈 변경 확인
        if (SpawnPhase.GrowCondition <= a.Sheet.index)
            SpawnPhase = SpawnPhase.GetNextPhase();

        AttackComboValue.Set(Combo);

        //스코어 갱신
        var gain = (a.Sheet.score + b.Sheet.score) * 10 * Combo;
        OnGainScore(gain);

        //획득 스코어만큼 스킬게이지 충전
        ChargeSkillGauge(gain);

        var remove_bad_units = new List<UnitBase>();

        //패시브 스킬 발동
        if (3 <= Combo)
            Passive?.Run(OnCompletePassiveSkill);

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

        //var comboBonus = Combo > 3 ? 3 * Combo : 0;
        var unitDamage = Utils.GetUnitDamage(a.Sheet.score, a.Info.Level);
        int badBlock = (int) ((unitDamage * Combo) * SuddenDeathRatio(_elapsedGameTimer));

        //내 방해블록 제거
        if (0 < MyStackDamage.Value)
        {
            MyStackDamage.Set(MyStackDamage.Value - badBlock);

            if (IsPlayer)
                PlayerBattleTracker.Update(PlayerTracker.DEFENCE_DAMAGE, badBlock);

            //내 방해블록 제거 + 상대방에게 공격
            if (MyStackDamage.Value <= 0)
            {
                int damage = Mathf.Abs(MyStackDamage.Value);
                AttackDamage.Set(damage);

                if (IsPlayer)
                {
                    PlayerBattleTracker.Update(PlayerTracker.DEFENCE_DAMAGE, MyStackDamage.Value);
                    PlayMergeAttackVFX(a.transform.position, PanelIngame.MyBadBlockVFXPoint.position, 0.5f, () =>
                    {
                        //블록 갱신
                        RefreshBadBlockUI();

                        PlayMergeAttackVFX(PanelIngame.MyBadBlockVFXPoint.position,
                            PanelIngame.EnemyBadBlockVFXPoint.position,
                            0.5f, () => { });
                    });
                }
            }
            //내 방해블록만 제거
            else
            {
                if (IsPlayer)
                {
                    PlayerBattleTracker.Update(PlayerTracker.DEFENCE_DAMAGE, badBlock);

                    PlayMergeAttackVFX(a.transform.position, PanelIngame.MyBadBlockVFXPoint.position, 0.5f,
                        () => { RefreshBadBlockUI(); });
                }
            }
        }
        //상대방에게 공격
        else
        {
            if (IsPlayer)
            {
                PlayerBattleTracker.Update(PlayerTracker.ATTACK_DAMAGE, badBlock);
                PlayMergeAttackVFX(a.transform.position, PanelIngame.EnemyBadBlockVFXPoint.position, 0.5f, () => { });
            }

            AttackDamage.Set(AttackDamage.Value + badBlock);
        }
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
        if (0 < MyStackDamage.Value)
        {
            MyStackDamage.Set(MyStackDamage.Value - damage);

            //내 방해블록 제거 + 상대방에게 공격
            if (MyStackDamage.Value <= 0)
            {
                AttackDamage.Set(Mathf.Abs(MyStackDamage.Value));

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

            AttackDamage.Set(AttackDamage.Value + damage);
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

    [Header("시스템")] public SubscribeValue<DateTime> MyReadyTime = new SubscribeValue<DateTime>(new DateTime());
    public DateTime EnemyReadyTime;
    public DateTime GameStartTime;
    public bool isReady;
    public bool isLaunchGame;
    public bool isGameStart;
    public bool isPauseBadBlockTimer;
    public SpawnPhase SpawnPhase;

    [FormerlySerializedAs("PlayerUnitGroup")]
    public string PlayerHeroKey = "Cat";

    public List<Unit> PlayerUnitGroupList = new List<Unit>();
    public bool IsPlayer = true;
    public SyncManager SyncManager;
    [Header("시스템 - 게임오버")] public GameObject GameOverLine;

    public float GameoverTimeoutDelta;
    public SubscribeValue<bool> isGameOver;
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
        if (isGameOver.Value) return;

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
                GameOver(GameManager.GetTime());
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

    public List<UnitBase> IgnoreUnits
    {
        get
        {
            List<UnitBase> result = new List<UnitBase>();
            var list = IgnoreUnitGUID;
            foreach (var unit in UnitsInField)
            {
                if (list.Contains(unit.GUID))
                    result.Add(unit);
            }

            return result;
        }
    }

    protected virtual int UnitLayer => LayerMask.NameToLayer("Unit");
    protected int UnitReadyLayer => LayerMask.NameToLayer("Unit Ready");

    #endregion

    #region Player Data

    [Header("플레이어 데이터")] public int Score;

    public int MAX_BADBLOCK_VALUE;

    [FormerlySerializedAs("MyBadBlockValue")]
    public SubscribeValue<int> MyStackDamage;

    [FormerlySerializedAs("AttackBadBlockValue")]
    public SubscribeValue<int> AttackDamage;

    public SubscribeValue<int> AttackComboValue;
    public int Combo;

    public List<ulong> IgnoreUnitGUID = new List<ulong>();

    public PassiveBase Passive;
    public ActiveBase Active;

    #endregion

    #region BadBlock

    //방해블록 타이머
    private float _badBlockTimerDelta;

    private int _badBlockMaxDropOneTime =>
        (int) Mathf.Clamp(
            EnvironmentValue.BAD_BLOCK_DROP_COUNT_MIN +
            (_elapsedGameTimer * EnvironmentValue.BAD_BLOCK_INCREASE_DROP_COUNT_PER_SECOND),
            EnvironmentValue.BAD_BLOCK_DROP_COUNT_MIN, EnvironmentValue.BAD_BLOCK_DROP_COUNT_MAX);

    private readonly List<List<Vector3>> Floors = new List<List<Vector3>>();


    private void BadBlockUpdate(float delta)
    {
        if (isPauseBadBlockTimer) return;

        if (0 < MyStackDamage.Value)
        {
            if (IsPlayer)
                PanelIngame.SetActiveBadBlockTimer(true);

            _badBlockTimerDelta += delta;

            float t = EnvironmentValue.BAD_BLOCK_TIMER_MAX -
                      (_elapsedGameTimer * EnvironmentValue.BAD_BLOCK_TIMER_PER_SECOND);
            t = Mathf.Clamp(t, EnvironmentValue.BAD_BLOCK_TIMER_MIN, EnvironmentValue.BAD_BLOCK_TIMER_MAX);

            if (IsPlayer)
                PanelIngame.UpdateBadBlockTimer(t - _badBlockTimerDelta, t);

            if (t < _badBlockTimerDelta)
            {
                _badBlockTimerDelta = 0f;

                //쌓인 방해블록 소모 
                var drop = 0;
                if (MyStackDamage.Value >= _badBlockMaxDropOneTime)
                {
                    MyStackDamage.Set(MyStackDamage.Value - _badBlockMaxDropOneTime);
                    drop = _badBlockMaxDropOneTime;
                }
                else
                {
                    drop = MyStackDamage.Value;
                    MyStackDamage.Set(0);
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
        {
            int vertical = EnvironmentValue.BAD_BLOCK_DROP_COUNT_MAX / EnvironmentValue.BAD_BLOCK_HORIZONTAL_MAX_COUNT;
            for (var i = 0; i < vertical; i++)
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
    private Vector2 _touchBegin = Vector2.zero;

    public GuideUnit GuideUnit = null;
    public ContactFilter2D ContactFilter2D;

    protected virtual void InputUpdate()
    {
        if (Input.GetMouseButtonDown(0))
            OnPress();

        if (isPress)
        {
            PanelIngame.InputActiveSkill.On(_touchBegin, Utils.GetTouchPoint());

            if (CurrentReadyUnit != null && PanelIngame.InputActiveSkill.isActive == false)
            {
                var input_pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                CurrentReadyUnit.transform.position =
                    new Vector3(input_pos.x, CurrentReadyUnit.transform.position.y,
                        CurrentReadyUnit.transform.position.z);

                CurrentReadyUnit.transform.localPosition =
                    new Vector3(
                        Mathf.Clamp(CurrentReadyUnit.transform.localPosition.x, -HorizontalSpawnLimit,
                            HorizontalSpawnLimit),
                        CurrentReadyUnit.transform.localPosition.y,
                        CurrentReadyUnit.transform.localPosition.z);
            }
        }

        if (isPress && Input.GetMouseButton(0) == false)
            OnRelease();

        //Guide Unit Tracking
        if (CurrentReadyUnit != null && PanelIngame.InputActiveSkill.isActive == false)
        {
            if (GuideUnit == null)
            {
                var prefab = ResourceManager.Instance.LoadPrefab("Units/GuideUnit");
                var go = Instantiate(prefab);
                GuideUnit = go.GetComponent<GuideUnit>();
                GuideUnit.transform.SetParent(CurrentReadyUnit.transform.parent);
                GuideUnit.transform.LocalReset();
                GuideUnit.transform.localScale = CurrentReadyUnit.transform.localScale;
            }

            RaycastHit2D[] result = new RaycastHit2D[1];
            if (GuideUnit != null)
            {
                if (GuideUnit.gameObject.activeSelf == false)
                    GuideUnit.gameObject.SetActive(true);

                GuideUnit.transform.localScale = CurrentReadyUnit.transform.localScale;

                Vector2 pos = CurrentReadyUnit.transform.localPosition;
                GuideUnit.Set(CurrentReadyUnit.Info.Key);
                GuideUnit.transform.localPosition = pos;

                CurrentReadyUnit.Collider2D.Cast(Vector2.down, ContactFilter2D, result);
                if (result[0] == true)
                {
                    pos = GuideUnit.transform.position;
                    pos.y = result[0].point.y;
                    GuideUnit.transform.position = pos;
                    pos = GuideUnit.transform.localPosition;
                    pos.y += GuideUnit.transform.localScale.y;
                    GuideUnit.transform.localPosition = pos;
                }
            }
        }
        else
        {
            if (GuideUnit != null && GuideUnit.gameObject.activeSelf)
                GuideUnit.gameObject.SetActive(false);
        }
    }

    protected void OnPress()
    {
        isPress = true;
        _touchBegin = Utils.GetTouchPoint();
    }

    protected void OnRelease()
    {
        isPress = false;

        if (IsPlayer && PanelIngame.InputActiveSkill.isActive)
        {
            bool isActive = 0.8f <= PanelIngame.InputActiveSkill.ActiveProgress;
            if (isActive)
                UseSkill();
            PanelIngame.InputActiveSkill.Off();

            return;
        }

        if (CurrentReadyUnit != null)
        {
            CurrentReadyUnit.Drop(true);
            if (IsPlayer)
                PlayerBattleTracker.Update(PlayerTracker.DROP_BLOCK, 1);
            UnitsInField.Add(CurrentReadyUnit);
            CurrentReadyUnit = null;
            _unitSpawnDelayDelta = EnvironmentValue.UNIT_SPAWN_DELAY;
        }
    }

    #endregion

    #region Attack

    private void OnReceiveAttack(int value)
    {
        if (value == 0) return;

        var stackDamage = MyStackDamage.Value + value;
        stackDamage = Mathf.Clamp(stackDamage, 0, MAX_BADBLOCK_VALUE);
        MyStackDamage.Set(stackDamage);

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

        var current = MyStackDamage.Value;
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

    public void OnReceiveSyncPacket(SyncManager.SyncPacket syncPacket)
    {
        foreach (var p in syncPacket.Packets)
        {
            SyncManager.SyncPacketBase packet = p;
            
            switch (p.PacketType)
            {
                case SyncManager.ePacketType.PlayerInfo:
                    OnReceivePlayerInfoPacket(packet as SyncManager.PlayerInfo);
                    break;
                case SyncManager.ePacketType.Ready:
                    OnReceiveReadyPacket((SyncManager.Ready)packet);
                    break;
                case SyncManager.ePacketType.UnitUpdate:
                    if (IsPlayer)
                        OnReceiveUpdateUnit(p as SyncManager.UpdateUnit);
                    break;
                case SyncManager.ePacketType.AttackDamage:
                    OnReceiveAttack((p as SyncManager.AttackDamage).Damage);
                    break;
                case SyncManager.ePacketType.UpdateAttackCombo:
                    OnReceiveCombo((p as SyncManager.UpdateAttackCombo).Combo);
                    break;
                case SyncManager.ePacketType.UpdateStackDamage:
                    if (IsPlayer)
                    {
                        PanelIngame.RefreshEnemyBadBlock((p as SyncManager.UpdateStackDamage).StackDamage);
                        RefreshBadBlockUI();
                    }

                    break;
                case SyncManager.ePacketType.GameResult:
                {
                    var result = p as SyncManager.GameResult;
                    OnReceiveGameResult(result.isGameOver, result.GameOverTime);
                }
                    break;
                case SyncManager.ePacketType.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private SyncManager.PlayerInfo _enemyPlayerInfo;

    public void OnReceivePlayerInfoPacket(SyncManager.PlayerInfo packet)
    {
        _enemyPlayerInfo = packet;
        if (IsPlayer)
            StartCoroutine(UpdatePlayerInfoProcess());
    }

    private IEnumerator UpdatePlayerInfoProcess()
    {
        if (PanelIngame == null)
            yield return null;
        PanelIngame.SetEnemyPortrait(_enemyPlayerInfo.Name, _enemyPlayerInfo.HeroKey.ToTableData<Hero>());
    }

    private void OnReceiveReadyPacket(SyncManager.Ready ready)
    {
        if (isReady && isLaunchGame == false)
        {
            EnemyReadyTime = ready.ReadyTime;
            GameStartTime = MyReadyTime.Value < EnemyReadyTime ? EnemyReadyTime : MyReadyTime.Value;
            GameStartTime = GameStartTime.AddSeconds(6);

            StartCoroutine(GameStartProcess());

            isLaunchGame = true;
        }
    }

    public void OnReceiveUpdateUnit(SyncManager.UpdateUnit packet)
    {
        EnemyScreen.Refresh(packet);
    }

    public void OnReceiveGameResult(bool isGameOver, DateTime time)
    {
        var isWin = false;
        //나도 게임오버인 경우
        if (this.isGameOver.Value)
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
            GameOver(time.AddSeconds(10));
        }

        if (IsPlayer)
        {
            var packet = new PacketBase();
            packet.PacketType = ePACKET_TYPE.REPORT_GAME_RESULT;
            packet.hash.Add("is_win", isWin);

            //모험모드 스테이지 데이터
            if (GameManager.Instance.isAdventure)
                packet.hash.Add("stage", GameManager.Instance.StageKey);

            //트랙커 정보 
            packet.hash.Add("tracker_json", JsonConvert.SerializeObject(PlayerBattleTracker.Tracker));

            PlayerInfo playerInfo = PlayerDataManager.Get<PlayerInfo>();
            var beforeScore = playerInfo.elements.RankScore;
            NetworkManager.Instance.Request(packet, res =>
            {
                if (res.hash.ContainsKey("first_clear"))
                {
                    var isFirstClear = (bool) res.hash["first_clear"];
                    if (isFirstClear)
                    {
                        var stage = (string) res.hash["stage"];
                        var heroes = TableManager.Instance.GetTable<Hero>();
                        foreach (var row in heroes)
                        {
                            var hero = row.Value as Hero;
                            if (hero.unlock_condition == stage)
                            {
                                GameManager.Instance.isUnlockHero = true;
                                break;
                            }
                        }
                    }
                }

                var popup = UIManager.Instance.ShowPopup<PopupGameResult>();
                popup.SetResult(isWin, beforeScore);
            });
        }
    }

    #endregion

    public void GameOver(DateTime time)
    {
        GameOverTime = time;
        isGameOver.Set(true);
    }

    #region Utility

    public IEnumerator PlayBezier(GameObject go, Vector3 from, Vector3 to, float duration, Action onFinish)
    {
        var delta = 0f;
        while (delta < duration)
        {
            var t = delta / duration;
            var st = (from - to) / 2;
            var et = (to - from) / 2;
            if (go != null)
            {
                st.z = go.transform.position.z;
                et.z = st.z;
                go.transform.position = BezierUtility.BezierPoint(from, st, et, to, t);
            }

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
        isGameOver.Set(false);
        UnitsInField.Clear();
        BadUnits.Clear();
        CurrentReadyUnit = null;
        AttackDamage.Clear();
        MyStackDamage.Clear();

        //UI 초기화
        if (IsPlayer)
        {
            PlayerScreen.SetActive(false);
            EnemyScreen.gameObject.SetActive(false);
            BattleCameraGroup.SetActive(false);
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

    public bool isPauseCollider = false;
    private ulong _pauseColliderID;

    public void PauseCollider(float duration)
    {
        isPauseCollider = true;
        int layer = UnitLayer;
        Physics2D.IgnoreLayerCollision(layer, layer, true);

        GameManager.DelayInvokeCancel(_pauseColliderID);
        _pauseColliderID =
            GameManager.DelayInvoke(() =>
            {
                Physics2D.IgnoreLayerCollision(layer, layer, false);
                isPauseCollider = false;
            }, duration);
    }

    #endregion
}