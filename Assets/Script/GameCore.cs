using System.Collections;
using System.Collections.Generic;
using Define;
using SheetData;
using UnityEngine;
using Violet;
using Violet.Audio;

public class GameCore : MonoSingleton<GameCore>
{
    public void Initialize()
    {
        PlayerScreen.SetActive(true);
        EnemyScreen.gameObject.SetActive(true);
        SyncManager.Instance.OnSyncCapture = OnCaptureSyncPacket;
        SyncManager.Instance.OnSyncReceive = OnReceiveSyncPacket;

        AudioManager.Instance.ChangeBGMVolume(0.3f);
        AudioManager.Instance.ChangeSFXVolume(0.3f);
        AudioManager.Instance.Play("Sound/bgm", eAUDIO_TYPE.BGM);

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
        {
            _unit = SpawnUnit();

            //콤보 초기화
            Combo = 0;
        }
        else
        {
            _unitSpawnDelayDelta -= delta;
        }

        MergeUpdate(delta);

        BadBlockUpdate(delta);

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
            key = "cat1";

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
        for (var i = 0; i < UnitsInField.Count; i++)
            if (UnitsInField[i].GUID == unit.GUID)
            {
                UnitsInField.RemoveAt(i);
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

        for (var i = 0; i < UnitsInField.Count; i++)
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
                    if (distance <= unit.transform.localScale.x * 1.26 * 2 + 5)
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

        isMergeProcess = false;
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

        var delta = 0f;
        var duration = 0.5f;
        while (delta < duration)
        {
            a.transform.position = cached_pos_a;
            b.transform.position = Vector3.Lerp(cached_pos_b, cached_pos_a, delta / (duration * 0.7f));
            delta += Time.deltaTime;
            yield return null;
        }

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
            UnitsInField.Add(SpawnUnit(a.Sheet.grow_unit, pos));

        IgnoreUnitGUID.Remove(cached_guid_a);
        IgnoreUnitGUID.Remove(cached_guid_b);
    }

    private void OnMergeEvent(UnitBase a, UnitBase b, int Combo)
    {
        //콤보 출력
        panelIngame.PlayCombo(a.transform.position, Combo);

        //스코어 갱신
        var gain = (a.Sheet.score + b.Sheet.score) * 10 * Combo;
        OnGainScore(gain);
    }

    private void OnGainScore(int gain)
    {
        var before = Score;
        Score += gain;

        panelIngame.RefreshScore(before, Score);

        OnReceiveBadBlock(gain);
        return;

        //내 방해블록 제거
        if (0 < BadBlockValue)
        {
            BadBlockValue -= gain;

            RefreshBadBlockUI();
            //내 방해블록 제거 + 상대방에게 공격
            if (BadBlockValue <= 0)
            {
            }
        }
        //상대방에게 공격
        else
        {
            BadBlockValue += gain;
            RefreshBadBlockUI();
        }
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
    public Vector3 UnitSpawnPosition;

    #endregion

    #region Player Data

    public int Score;
    public int MAX_BADBLOCK_VALUE;
    public int BadBlockValue;
    public bool isMergeProcess;
    public int Combo;
    public List<string> IgnoreUnitGUID = new List<string>();
    private readonly List<Unit> _badBlockSheet = new List<Unit>();

    #endregion


    #region BadBlock

    //방해블록 타이머
    private float _badBlockDropDelta;

    //방해블록 한줄 최대치
    private const int _badBlockFloorMaxUnit = 6;

    //방해블록 최대 층
    private const int _badBlockMaxFloor = 3;

    private int _badBlockMaxDropOneTime => (int) (_badBlockFloorMaxUnit * _badBlockMaxFloor);

    //방해블록 가로영역 크기
    private const float _badBlockWidth = 880;

    //Y축 시작 지점
    private const float _badBlockY = 1050;

    //Y축 증가량
    private const float _badBlockFloorHeight = 150;

    private readonly List<List<Vector3>> Floors = new List<List<Vector3>>();

    private void BadBlockUpdate(float delta)
    {
        if (Input.GetKeyDown(KeyCode.Space))
            DropBadBlock(Random.Range(1, 18));
        if (0 < BadBlockValue)
        {
            _badBlockDropDelta += delta;
            if (_badBlockDropDelta < EnvironmentValue.BAD_BLOCK_TIMER)
            {
                _badBlockDropDelta = 0f;

                //쌓인 방해블록 소모 
                var drop = 0;
                if (BadBlockValue >= _badBlockMaxDropOneTime)
                {
                    BadBlockValue -= _badBlockMaxDropOneTime;
                    drop = _badBlockMaxDropOneTime;
                }
                else
                {
                    drop = BadBlockValue;
                    BadBlockValue = 0;
                }

                DropBadBlock(drop);
                RefreshBadBlockUI();
            }
        }
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

            unit.SetPosition(shuffled_floor[floor][index]);
            unit.Drop();
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

    private void OnSendBadBlock(int value)
    {
    }

    private void OnReceiveBadBlock(int value)
    {
        BadBlockValue += value;

        BadBlockValue = Mathf.Clamp(BadBlockValue, 0, MAX_BADBLOCK_VALUE);
        RefreshBadBlockUI();
    }

    private void RefreshBadBlockUI()
    {
        var blocks = new List<Unit>();

        var current = BadBlockValue;
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
        SyncManager.Instance.Receive(packet);
    }

    public void OnReceiveSyncPacket(SyncManager.SyncPacket packet)
    {
        RefreshEnemy(packet);
    }

    public void RefreshEnemy(SyncManager.SyncPacket packet)
    {
        EnemyScreen.Refresh(packet);
    }

    #endregion
}