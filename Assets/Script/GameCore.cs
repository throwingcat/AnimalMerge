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
    public void Update()
    {
        InputUpdate();

        if (_unit == null)
        {
            _unit = SpawnUnit();
        }
    }

    private UnitBase SpawnUnit()
    {
        var go = Instantiate(UnitPrefab, UnitParent);
        go.transform.SetAsLastSibling();
        var component = go.GetComponent<UnitBase>();
        
        component.OnSpawn(UnitSpawnPosition);
        
        return component;
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
                _unit.transform.position = new Vector3(input_pos.x, _unit.transform.position.y, _unit.transform.position.z);
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
            _unit = null;
        }
    }
    #endregion
}