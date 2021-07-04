using System.Collections.Generic;
using Define;
using UnityEngine;
using Violet;

public class GameCore : MonoSingleton<GameCore>
{
    #region Timer Value
    private float _mergeDelayDelta;
    private float _unitSpawnDelayDelta;
    private float _syncCaptureDelta = 0f;
    #endregion
    
    public CSVDownloadConfig CSVDownloadConfig;
    
    #region Unit Value
    private UnitBase _unit;
    public Transform UnitParent;
    public UnitBase UnitPrefab;
    public List<UnitBase> UnitsInField = new List<UnitBase>();
    public Vector3 UnitSpawnPosition;
    #endregion
    
    #region Enemy Layer

    public EnemyScreen EnemyScreen;
    #endregion
    
    public void Initialize()
    {
        SyncManager.Instance.OnSyncCapture = OnCaptureSyncPacket;
        SyncManager.Instance.OnSyncReceive = OnReceiveSyncPacket;
    }
    public void OnUpdate(float delta)
    {
        InputUpdate();

        if (_unit == null && _unitSpawnDelayDelta <= 0f)
            _unit = SpawnUnit();
        else
            _unitSpawnDelayDelta -= delta;

        MergeUpdate(delta);

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

    private UnitBase SpawnUnit()
    {
        var go = Instantiate(UnitPrefab, UnitParent);
        go.transform.SetAsLastSibling();
        var component = go.GetComponent<UnitBase>();

        component.OnSpawn("cat1")
            .SetPosition(UnitSpawnPosition)
            .SetRotation(Vector3.zero);

        return component;
    }

    private UnitBase SpawnUnit(string key, Vector3 pos)
    {
        var go = Instantiate(UnitPrefab, UnitParent);
        go.transform.SetAsLastSibling();
        var component = go.GetComponent<UnitBase>();
        component.OnSpawn(key)
            .SetPosition(pos);
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

        Destroy(unit.gameObject);
    }

    private void MergeUpdate(float dleta)
    {
        if (0 < _mergeDelayDelta)
        {
            _mergeDelayDelta -= dleta;
            return;
        }

        var isMerge = false;
        for (var i = 0; i < UnitsInField.Count; i++)
        {
            var unit = UnitsInField[i];

            foreach (var friend in UnitsInField)
            {
                if (unit.GUID == friend.GUID) continue;
                if (unit.Sheet.key != friend.Sheet.key) continue;
                var distance = Vector3.Distance(unit.transform.localPosition, friend.transform.localPosition);
                if (distance <= unit.transform.localScale.x * 1.26 * 2 + 5)
                {
                    //Merge 실행
                    isMerge = true;
                    //Remove A B

                    var pos = new Vector3(
                        (unit.transform.localPosition.x + friend.transform.localPosition.x) * 0.5f,
                        (unit.transform.localPosition.y + friend.transform.localPosition.y) * 0.5f,
                        unit.transform.localPosition.z);

                    RemoveUnit(unit);
                    RemoveUnit(friend);

                    if (unit.Sheet.grow_unit.IsNullOrEmpty() == false)
                        UnitsInField.Add(SpawnUnit(unit.Sheet.grow_unit, pos));

                    _mergeDelayDelta = EnvironmentValue.MERGE_DELAY;
                    break;
                }
            }

            if (isMerge)
                break;
        }
    }

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

                var horizontalLimit = 540f - (EnvironmentValue.UNIT_SPRITE_BASE_SIZE * EnvironmentValue.WORLD_RATIO * _unit.Sheet.size);
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