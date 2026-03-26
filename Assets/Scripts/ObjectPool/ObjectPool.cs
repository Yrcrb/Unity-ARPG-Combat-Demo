using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ObjectPool<T> where T : class
{
    private readonly Queue<T> _pool = new Queue<T>();
    private readonly Func<T> onGreat;
    private readonly Action<T> onGetAction;
    private readonly Action<T> onReleaseAction;
    private readonly int maxCount;
    private readonly int originalCount;

    public ObjectPool(Func<T> _onGreat, Action<T> _onGetAction, Action<T> _onReleaseAction, int _maxCount,int _originalCount)
    { 
        onGreat = _onGreat;
        onGetAction = _onGetAction; 
        onReleaseAction = _onReleaseAction;
        maxCount = _maxCount;
        originalCount = _originalCount;
        Greation();
    }
    public void Greation()
    {
        for (int i = 0; i < originalCount && _pool.Count < maxCount; i++)
        {
            var obj = onGreat();
            if (obj != null)
            {
                onReleaseAction?.Invoke(obj);
                _pool.Enqueue(obj);
            }
        }
    }
    public T Get()
    {
        T obj = null;
        if (_pool.Count > 0)
        {
            obj = _pool.Dequeue();
            onGetAction?.Invoke(obj);
        }
        else 
        {
            return null;
        }
        return obj;
    }
    public void Release(T obj)
    {
        if (obj == null)
        {
             return;
        }
        if (obj is UnityEngine.Object unityObj && unityObj == null)
        {
            return;
        }
        if (_pool.Count < maxCount)
        { 
            onReleaseAction?.Invoke(obj);
            _pool.Enqueue(obj);
        }
    }
}
