using System;
using System.Collections.Generic;

public class SubscribeBase<T>
{
    [NonSerialized] protected Dictionary<long, Action<T>> _onUpdate = new Dictionary<long, Action<T>>();

    [NonSerialized] private long _uniqueID;

    public virtual long Subscribe(Action<T> action, bool isInitializeUpdate = true)
    {
        var id = GetUniqueID();
        _onUpdate.Add(id, action);

        return id;
    }

    public void Unsubscribe(long id)
    {
        _onUpdate.Remove(id);
    }

    public void ClearSubscribe()
    {
        _onUpdate.Clear();
    }

    public void Response(T value)
    {
        foreach (var action in _onUpdate) action.Value?.Invoke(value);
    }

    private long GetUniqueID()
    {
        return _uniqueID++;
    }
}

public class SubscribeValue<T> : SubscribeBase<T>
{
    private T _prevValue;

    public SubscribeValue(T value)
    {
        Value = value;
    }

    public T Value { get; private set; }

    public override long Subscribe(Action<T> action, bool isInitializeUpdate = true)
    {
        var id = base.Subscribe(action, isInitializeUpdate);

        if (isInitializeUpdate)
            Response(Value);

        return id;
    }

    public void Set(T value)
    {
        Value = value;
        if (_prevValue.Equals(Value) == false)
        {
            _prevValue = Value;
            Response(Value);
        }
    }

    public void Clear()
    {
        Value = default(T);
        _prevValue = default(T);
    }
}