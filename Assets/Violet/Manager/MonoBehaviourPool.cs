using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MonoBehaviourPool<T> where T : MonoBehaviour
{
    public static Dictionary<string, MonoBehaviourPool<T>> Pools = new Dictionary<string, MonoBehaviourPool<T>>();
    private readonly string _key;

    private GameObject _root;

    public Func<T> constructor;
    public Queue<T> Queue = new Queue<T>();

    public MonoBehaviourPool(string key)
    {
        _key = key;
    }

    public static MonoBehaviourPool<T> CreatePool(string key, Func<T> constructor, int capacity, GameObject root = null)
    {
        if (Pools.ContainsKey(key) == false)
        {
            var pool = new MonoBehaviourPool<T>(key);
            pool.constructor = constructor;
            pool._root = root;
            for (var i = 0; i < capacity; i++)
                pool.Push();
            Pools.Add(key, pool);
        }

        return Pools[key];
    }

    public static MonoBehaviourPool<T> GetPool(string key)
    {
        if (Pools.ContainsKey(key))
            return Pools[key];

        return null;
    }

    public static T Get(string key)
    {
        var pool = GetPool(key);
        return pool?.Get();
    }

    public T Get()
    {
        if (Queue.Count == 0)
            Push();

        return Queue.Dequeue();
    }

    private void Push()
    {
        if (constructor == null)
        {
            Debug.LogError("ObjectPool Push Fail / Constructor is null");
            return;
        }

        var item = constructor();
        if (item == null)
            Debug.LogError("ObjectPool Push Fail / item is null");

        if (_root == null)
            _root = new GameObject(_key);

        item.transform.SetParent(_root.transform);

        Queue.Enqueue(item);
    }

    public void Restore(T item)
    {
        item.transform.SetParent(_root.transform);
        item.gameObject.SetActive(false);
        Queue.Enqueue(item);
    }
}

public class GameObjectPool
{
    public static Dictionary<string, GameObjectPool> Pools = new Dictionary<string, GameObjectPool>();
    private readonly string _key;
    private string _cagetory = "";
    private GameObject _root;

    public Func<GameObject> constructor;
    public Queue<GameObject> Queue = new Queue<GameObject>();
    private List<GameObject> _createdObjects = new List<GameObject>();
    public string Key => _key;

    public GameObjectPool(string key)
    {
        _key = key;
    }

    public static GameObjectPool CreatePool(string key, Func<GameObject> constructor, int capacity,
        GameObject root = null, string category = "")
    {
        if (Pools.ContainsKey(key) == false)
        {
            var pool = new GameObjectPool(key);
            pool.constructor = constructor;
            pool._root = root;
            pool._cagetory = category;
            for (var i = 0; i < capacity; i++)
                pool.Push();
            Pools.Add(key, pool);
        }

        return Pools[key];
    }

    public static GameObjectPool GetPool(string key)
    {
        if (Pools.ContainsKey(key))
            return Pools[key];

        return null;
    }

    public static GameObject Get(string key)
    {
        var pool = GetPool(key);
        return pool?.Get();
    }

    public GameObject Get()
    {
        if (Queue.Count == 0)
            Push();

        return Queue.Dequeue();
    }

    public void AddCapacity(int capacity)
    {
        for(int i=0;i<capacity;i++)
            Push();
    }
    private void Push()
    {
        if (constructor == null)
        {
            Debug.LogError("ObjectPool Push Fail / Constructor is null");
            return;
        }

        var item = constructor();
        if (item == null)
            Debug.LogError("ObjectPool Push Fail / item is null");

        if (_root == null)
            _root = new GameObject(_key);

        item.transform.SetParent(_root.transform);

        _createdObjects.Add(item);

        Queue.Enqueue(item);
    }

    public void Restore(GameObject item)
    {
        if (item == null) return;

        item.transform.SetParent(_root.transform);
        item.gameObject.SetActive(false);
        Queue.Enqueue(item);
    }

    public static void DestroyPools(string category)
    {
        List<string> remove_target = new List<string>();
        foreach (var pool in Pools)
        {
            if (pool.Value._cagetory.Equals(category))
            {
                remove_target.Add(pool.Key);
                for (int i = 0; i < pool.Value._createdObjects.Count; i++)
                    GameObject.Destroy(pool.Value._createdObjects[i]);

                pool.Value._createdObjects.Clear();
                pool.Value.Queue.Clear();

                if (pool.Value._root.transform.parent == null)
                    GameObject.Destroy(pool.Value._root);
            }
        }

        for (int i = 0; i < remove_target.Count; i++)
            Pools.Remove(remove_target[i]);
    }
}