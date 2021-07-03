using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Violet;

public class GameCore : MonoSingleton<GameCore>
{
    public CSVDownloadConfig CSVDownloadConfig;

    public UnitBase UnitPrefab;
    public Transform UnitParent;
    public Vector3 UnitSpawnPosition;
    public float UnitHorizontalLimit = 333f;
    private UnitBase _unit;

    public List<UnitBase> UnitsInField = new List<UnitBase>();

    public void OnUpdate(float delta)
    {
        InputUpdate();

        if (_unit == null)
        {
            _unit = SpawnUnit();
        }

        MergeUpdate(delta);
    }

    private UnitBase SpawnUnit()
    {
        var go = Instantiate(UnitPrefab, UnitParent);
        go.transform.SetAsLastSibling();
        var component = go.GetComponent<UnitBase>();

        component.OnSpawn("cat1", UnitSpawnPosition);

        return component;
    }

    private UnitBase SpawnUnit(string key, Vector3 pos)
    {
        var go = Instantiate(UnitPrefab, UnitParent);
        go.transform.SetAsLastSibling();
        var component = go.GetComponent<UnitBase>();
        component.OnSpawn(key, pos);
        component.Drop();
        return component;
    }

    private void RemoveUnit(UnitBase unit)
    {
        for (int i = 0; i < UnitsInField.Count; i++)
        {
            if (UnitsInField[i].GUID == unit.GUID)
            {
                UnitsInField.RemoveAt(i);
                break;
            }
        }

        Destroy(unit.gameObject);
    }

    private float MERGE_DELAY = 0.15f;
    private float _mergeDelayDelta = 0f;
    private void MergeUpdate(float dleta)
    {
        if (0 < _mergeDelayDelta)
        {
            _mergeDelayDelta -= dleta;
            return;
        }

        bool isMerge = false;
        for (int i = 0; i < UnitsInField.Count; i++)
        {
            var unit = UnitsInField[i];

            foreach (var friend in UnitsInField)
            {
                if (unit.GUID == friend.GUID) continue;
                if (unit.Sheet.key != friend.Sheet.key) continue;
                float distance = Vector3.Distance(unit.transform.localPosition, friend.transform.localPosition);
                if (distance <= (unit.transform.localScale.x * 1.26 * 2) + 5)
                {
                    //Merge 실행
                    isMerge = true;
                    //Remove A B

                    Vector3 pos = new Vector3(
                        (unit.transform.localPosition.x + friend.transform.localPosition.x) * 0.5f,
                        (unit.transform.localPosition.y + friend.transform.localPosition.y) * 0.5f,
                        unit.transform.localPosition.z);

                    RemoveUnit(unit);
                    RemoveUnit(friend);

                    if (unit.Sheet.grow_unit.IsNullOrEmpty() == false)
                        UnitsInField.Add(SpawnUnit(unit.Sheet.grow_unit, pos));

                    _mergeDelayDelta = MERGE_DELAY;
                    break;
                }
            }

            if (isMerge)
                break;
        }
    }

    #region Input

    private bool isPress = false;

    private void InputUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            OnPress();
        }

        if (isPress && Input.GetMouseButton(0) == false)
        {
            OnRelease();
        }

        if (isPress)
        {
            if (_unit != null)
            {
                var input_pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                _unit.transform.position =
                    new Vector3(input_pos.x, _unit.transform.position.y, _unit.transform.position.z);
                _unit.transform.localPosition =
                    new Vector3(Mathf.Clamp(_unit.transform.localPosition.x, -UnitHorizontalLimit, UnitHorizontalLimit),
                        _unit.transform.localPosition.y,
                        _unit.transform.localPosition.z);
            }
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
        }
    }

    #endregion
}