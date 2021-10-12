using System;
using System.Collections.Generic;
using UnityEngine;

//==================================================================================================
//     Summary : MonoObjectPool 생성과 파괴 한다.
//               매니져를 통해 , 객체 가져오고 반환해야함.
//               
//     Note : pool을 createPool() 로 생성을 하고 , GetMonoObject로 instance를 가져다 쓰면 된다.
//==================================================================================================

namespace Violet
{
    public class MonoObjectPoolManager : MonoSingleton<MonoObjectPoolManager>
    {
        private readonly Dictionary<string, MonoObjectPool> _poolDic = new Dictionary<string, MonoObjectPool>();


        public void CreatePool<T>(GameObject prefab, string key, int capacityCount, Transform parent = null,
            bool hasContainer = true) where T : MonoBehaviour
        {
            key = CreateKey<T>(key);

            if (_poolDic.ContainsKey(key))
            {
                if (_poolDic[key] == null)
                    _poolDic.Remove(key);
                else
                    return;
            }

            //FAGLog.LogWarningFormat("Create Pool - ( prefab {0} ) ( key {1} ),", prefab.name, key);

            var goContainer = hasContainer ? new GameObject() : parent.gameObject;

            if (hasContainer)
            {
                goContainer.transform.SetParent(parent == null ? transform : parent);
                goContainer.transform.localPosition = Vector3.zero;
                goContainer.transform.localRotation = Quaternion.identity;
                goContainer.transform.localScale = Vector3.one;
                goContainer.name = key + "_Pool";
            }

            var pool = goContainer.AddComponent<MonoObjectPool>();
            pool.InitWithPrefab<T>(prefab, capacityCount);

            _poolDic.Add(key, pool);
        }

        public bool Contains<T>(string key)
        {
            key = CreateKey<T>(key);
            return _poolDic.ContainsKey(key);
        }

        public T GetMonoObject<T>(string key, bool isActive = true) where T : MonoBehaviour
        {
            key = CreateKey<T>(key);
            MonoObjectPool pool;
            if (_poolDic.TryGetValue(key, out pool) == false || pool == null)
            {
                VioletLogger.LogWarningFormat("MonoObject Pool is null [key]{0}", key);
                return null;
            }

            var obj = pool.GetObject<T>(isActive);
            return obj;
        }

        public void ReturnMonoObject<T>(string key, T item) where T : MonoBehaviour
        {
            key = CreateKey<T>(key);
            MonoObjectPool pool;
            if (_poolDic.TryGetValue(key, out pool) == false || pool == null)
                //FAGLog.LogWarningFormat("MonoObject Pool is null [key]{0}", key);
                return;
            pool.ReturnObject(item);
        }

        public void ReturnMonoObjectWithActive<T>(string key, T item) where T : MonoBehaviour
        {
            key = CreateKey<T>(key);
            MonoObjectPool pool;
            if (_poolDic.TryGetValue(key, out pool) == false || pool == null)
                //FAGLog.LogWarningFormat("MonoObject Pool is null [key]{0}", key);
                return;
            pool.ReturnObjectActive(item);
        }

        public void ReturnAllMonoObject<T>(string key) where T : MonoBehaviour
        {
            key = CreateKey<T>(key);
            MonoObjectPool pool;
            if (_poolDic.TryGetValue(key, out pool) == false || pool == null)
                //FAGLog.LogWarningFormat("MonoObject Pool is null [key]{0}", key);
                return;
            pool.ReturnAllObject();
        }

        public void ChangeCapacity<T>(string key, int count) where T : MonoBehaviour
        {
            key = CreateKey<T>(key);
            MonoObjectPool pool;
            if (_poolDic.TryGetValue(key, out pool) == false || pool == null)
            {
                VioletLogger.LogWarningFormat("MonoObject Pool is null [key]{0}", key);
                return;
            }

            pool.ChangeCapacity<T>(count);
        }

        public void DestroyPool<T>(string key)
        {
            key = CreateKey<T>(key);

            if (_poolDic.ContainsKey(key) == false) return;

            var pool = _poolDic[key];
            _poolDic.Remove(key);

            if (pool != null)
                Destroy(pool);
        }

        public void DestroyAllPool()
        {
            using (var enumerator = _poolDic.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var key = enumerator.Current.Key;
                    var pool = enumerator.Current.Value;
                    if (pool != null)
                        Destroy(pool.gameObject);
                }
            }

            _poolDic.Clear();
        }

        public void ClearPoolElements()
        {
            using (var enumerator = _poolDic.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var key = enumerator.Current.Key;
                    var pool = enumerator.Current.Value;
                    if (pool != null)
                        pool.ClearObject();
                }
            }
        }

        public void HidePoolElements(Type type)
        {
            using (var enumerator = _poolDic.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var key = enumerator.Current.Key;
                    var pool = enumerator.Current.Value;
                    if (pool != null && pool.gameObject.name.Contains(type.ToString())) pool.HideObject();
                }
            }
        }

        public MonoObjectPool GetPool<T>(string key) where T : MonoBehaviour
        {
            key = CreateKey<T>(key);

            if (_poolDic.ContainsKey(key) == false) return null;

            var pool = _poolDic[key];

            return pool;
        }

        private string CreateKey<T>(string key)
        {
            return typeof(T) + key;
        }
    }
}