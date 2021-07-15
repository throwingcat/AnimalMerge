using System.Collections;
using System.Collections.Generic;
using SheetData;
using UnityEngine;
using Violet;

public class EnemyScreen : MonoBehaviour
{
    [SerializeField]private Camera _camera;
    [SerializeField]private RenderTexture _renderTexture;
    [SerializeField]private SyncUnitBase _syncUnitPrefab;
    [SerializeField]private Transform _unitParent;
    
    private List<UnitBase> _units = new List<UnitBase>();
    
    public void Refresh(SyncManager.SyncPacket packet)
    {
        //Restore
        foreach (var unit in _units)
        {
            string pool_key = string.Format("{0}_{1}", "sync", unit.Sheet.key);
            var pool = GameObjectPool.GetPool(pool_key);
            if(pool !=null)
                pool.Restore(unit.gameObject);
        }
        _units.Clear();
        
        //Get
        foreach (var unit in packet.UnitsDatas)
        {
            var sheet = unit.UnitKey.ToTableData<Unit>();
            string pool_key = string.Format("{0}_{1}", "sync", sheet.key);
            var pool = GameObjectPool.GetPool(pool_key);
            if (pool == null)
            {
                pool = GameObjectPool.CreatePool(pool_key, () =>
                {
                    var go = Instantiate(_syncUnitPrefab, _unitParent);
                    go.transform.LocalReset();
                    go.gameObject.SetActive(false);
                    return go.gameObject;
                }, 1,category:Define.Key.IngamePoolCategory);
            }

            var go = pool.Get();
            var unitBase = go.GetComponent<UnitBase>();
            
            unitBase.transform.SetParent(_unitParent);
            unitBase.OnSpawn(unit.UnitKey)
                .SetPosition(unit.UnitPosition.ToVector3())
                .SetRotation(unit.UnitRotation.ToVector3());
            unitBase.gameObject.SetActive(true);
            
            _units.Add(unitBase);
        }
    }
    
    public RenderTexture Capture()
    {
        return _renderTexture;
    }
}