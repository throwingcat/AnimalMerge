using System.Collections;
using System.Collections.Generic;
using BackEnd;
using Define;
using DG.Tweening;
using SheetData;
using UnityEngine;
using Violet;
using Violet.Audio;

public class GameCore : MonoSingleton<GameCore>
{
    public int SpawnLevel = 1;

    public void Initialize()
    {
        PlayerScreen.SetActive(true);
        EnemyScreen.gameObject.SetActive(true);
        SyncManager.Instance.OnSyncCapture = OnCaptureSyncPacket;
        SyncManager.Instance.OnSyncReceive = OnReceiveSyncPacket;

        AudioManager.Instance.ChangeBGMVolume(0.3f);
        AudioManager.Instance.ChangeSFXVolume(0.3f);
        AudioManager.Instance.Play("Sound/bgm", eAUDIO_TYPE.BGM);

        Backend.Match.OnMatchRelay -= SyncManager.Instance.OnReceiveMatchRelay;
        Backend.Match.OnMatchRelay += SyncManager.Instance.OnReceiveMatchRelay;

        //방해 블록 초기화
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
    }

    public void OnUpdate(float delta)
    {
        InputUpdate();

        if (_unit == null && _unitSpawnDelayDelta <= 0f)
            _unit = SpawnUnit();
        else
            _unitSpawnDelayDelta -= delta;

        MergeUpdate(delta);

        BadBlockUpdate(delta);

        ComboUpdate(delta);

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
            }, 1, UnitParent.gameObject);

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

    private void MergeUpdate(float dleta)
    {
        if (0 < _mergeDelayDelta)
        {
            _mergeDelayDelta -= dleta;
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

        var comboBonus = Combo > 3 ? 18 * Combo : 0;
        var badBlock = (a.Sheet.score + b.Sheet.score) * Combo + comboBonus;
        OnReceiveBadBlock(badBlock);

        //방해블록 타이머 1초 감소
        ReduceBadBlockTimer(1f);

        //주변 방해블록 삭제
        var remove_bad_units = new List<UnitBase>();

        foreach (var unit in BadUnits)
        {
            if (IgnoreUnitGUID.Contains(unit.GUID)) continue;

            if (unit.eUnitType == eUnitType.Bad)
            {
                var distance = Vector3.Distance(a.transform.localPosition, unit.transform.localPosition);
                if (distance <= a.transform.localScale.x * 1.26 * 2 + 5)
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
    }

    private void OnGainScore(int gain)
    {
        var before = Score;
        Score += gain;

        panelIngame.RefreshScore(before, Score);

        //내 방해블록 제거
        if (0 < MyBadBlockValue)
        {
            MyBadBlockValue -= gain;
        
            RefreshBadBlockUI();
            //내 방해블록 제거 + 상대방에게 공격
            if (MyBadBlockValue <= 0)
                AttackBadBlockValue = Mathf.Abs(MyBadBlockValue);
        }
        //상대방에게 공격
        else
        {
            AttackBadBlockValue += gain;
        }
    }

    private void ComboUpdate(float delta)
    {
        if (_comboDelta <= 0f)
            Combo = 0;
        else
            _comboDelta -= delta;
    }

    public void OnLeave()
    {
        PlayerScreen.SetActive(false);
        EnemyScreen.gameObject.SetActive(false);
    }

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
        
        MyBadBlockValue += packet.BadBlockValue;
        RefreshBadBlockUI();
    }

    public void RefreshEnemy(SyncManager.SyncPacket packet)
    {
        EnemyScreen.Refresh(packet);
    }

    #endregion
}