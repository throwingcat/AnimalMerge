using System;
using System.Collections.Generic;
using UnityEngine;

//==================================================================================================
//     Summary : 게임오브젝트를 미리 생성하고 재활용 한다. 
//               기본 Capacity를 인자로 받고 , _destroyCheckTime 마다 사용 하지 않는 객체를 지운다.
//
//==================================================================================================


namespace Violet
{
    public class MonoObjectPool : MonoBehaviour
    {
        private const int MIN_DEFAULT_CAPACITY = 10;

        private const string INIT_METHOD_NAME = "Start"; // 다른 메서드로 바꿔도 됨.
        private const string DESTROY_METHOD_NAME = "DestroyNotUseObject";
        private const float DESTROY_DEFAULT_TIME = 3.0f;
        private List<MonoBehaviour> _allObjectList = new List<MonoBehaviour>();
        private Queue<MonoBehaviour> _availableObjectQueue = new Queue<MonoBehaviour>();

        private Type _componentType;
        private float _destroyCheckTime = DESTROY_DEFAULT_TIME;

        private int _index;

        private Action _onInit;
        private GameObject _prefab;
        private Transform _transform;
        public int DefaultCapacityCount { get; private set; }

        public void Awake()
        {
            _transform = transform;
        }

        private void OnDestroy()
        {
            ClearObject();

            _availableObjectQueue = null;
            _allObjectList = null;
        }

        public void InitWithPrefab<T>(GameObject prefab, int capacityCount, Action onInit = null,
            float destroyCheckTime = DESTROY_DEFAULT_TIME) where T : MonoBehaviour
        {
            _prefab = prefab;
            DefaultCapacityCount = capacityCount;
            _onInit = onInit;
            _destroyCheckTime = destroyCheckTime;

            _componentType = typeof(T);

            for (var i = 0; i < DefaultCapacityCount; i++) _availableObjectQueue.Enqueue(CreateObject<T>());

            InvokeRepeating(DESTROY_METHOD_NAME, _destroyCheckTime, _destroyCheckTime);

            if (_onInit != null)
                _onInit();
        }

        public void ChangeCapacity<T>(int count) where T : MonoBehaviour
        {
            DefaultCapacityCount = count;

            var addCount = DefaultCapacityCount - _allObjectList.Count;

            if (addCount > 0)
                for (var i = 0; i < addCount; i++)
                    _availableObjectQueue.Enqueue(CreateObject<T>());
        }

        public T GetObject<T>(bool isActive = true) where T : MonoBehaviour
        {
            MonoBehaviour mono = null;

            if (_availableObjectQueue.Count == 0)
                mono = CreateObject<T>();
            else
                mono = _availableObjectQueue.Dequeue();

            if (mono == null)
            {
                VioletLogger.LogError("object pool GetObject() error !!!");
                return default;
            }

            mono.transform.position = Vector3.zero;
            mono.transform.rotation = Quaternion.identity;

            mono.gameObject.SetActive(isActive);

            // item.SendMessage(INIT_METHOD_NAME); ToDo :  

            return mono as T;
        }

        // Note : 사용 후 이 메서드로 반환하시오.
        public void ReturnObject<T>(T item) where T : MonoBehaviour
        {
            if (item.GetType() != _componentType)
            {
                VioletLogger.LogError("object pool type error !!!");
                return;
            }

            if (_allObjectList.Contains(item) == false)
            {
                VioletLogger.LogError("object pool return error !!!");
                return;
            }

            item.gameObject.SetActive(false);
            item.transform.SetParent(_transform);

            if (_availableObjectQueue.Contains(item) == false) _availableObjectQueue.Enqueue(item);
        }

        // Note : 사용 후 이 메서드로 반환하시오.
        public void ReturnObjectActive<T>(T item) where T : MonoBehaviour
        {
            if (item.GetType() != _componentType)
            {
                VioletLogger.LogError("object pool type error !!!");
                return;
            }

            if (_allObjectList.Contains(item) == false)
            {
                VioletLogger.LogError("object pool return error !!!");
                return;
            }

            if (_availableObjectQueue.Contains(item) == false) _availableObjectQueue.Enqueue(item);
        }

        public void ReturnAllObject()
        {
            _availableObjectQueue.Clear();

            for (var i = 0; i < _allObjectList.Count; i++)
            {
                var item = _allObjectList[i];
                if (item.gameObject != null)
                    item.gameObject.SetActive(false);

                _availableObjectQueue.Enqueue(item);
            }
        }

        private T CreateObject<T>() where T : MonoBehaviour
        {
            _index++;
            if (_index == int.MaxValue)
                _index = 0;

            var obj = Instantiate(_prefab);
            obj.name = string.Format("{0}_{1}", _prefab.name, _index);
            obj.transform.SetParent(_transform);

            var component = obj.GetComponent<T>();

            if (component == null) component = obj.AddComponent<T>();

            if (component == null)
            {
                VioletLogger.LogError("object pool error !!!");
                return null;
            }

            obj.SetActive(false);
            _allObjectList.Add(component);

            return component;
        }

        //Note : invokerepeat 돌리면서 사용하지 않는 객체를 지운다. 
        private void DestroyNotUseObject()
        {
            if (_allObjectList == null || _availableObjectQueue == null)
                return;

            var notUseCount = _availableObjectQueue.Count - Math.Max(MIN_DEFAULT_CAPACITY, DefaultCapacityCount);

            for (var i = 0; i < notUseCount; i++)
            {
                var mono = _availableObjectQueue.Dequeue();
                if (mono != null)
                {
                    _allObjectList.Remove(mono);
                    Destroy(mono.gameObject);
                }
            }
        }

        public void ClearObject()
        {
            if (_allObjectList != null)
            {
                for (var i = 0; i < _allObjectList.Count; i++)
                {
                    if (_allObjectList[i] != null)
                        Destroy(_allObjectList[i].gameObject);
                    _allObjectList.RemoveAt(i--);
                }

                _allObjectList.Clear();
            }

            if (_availableObjectQueue != null)
                _availableObjectQueue.Clear();
        }

        public void HideObject()
        {
            if (_allObjectList != null)
                for (var i = 0; i < _allObjectList.Count; i++)
                    if (_allObjectList[i] != null && _allObjectList[i].gameObject.activeSelf)
                        _allObjectList[i].gameObject.SetActive(false);
        }
    }
}