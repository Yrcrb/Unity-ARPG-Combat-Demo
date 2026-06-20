using System;
using System.Collections.Generic;

/// <summary>
/// 事件名枚举，所有模块间通信的事件统一在此定义。
/// </summary>
public enum E
{
    // 战斗
    OnAttack,
    OnExAttack,
    OutExAttack,
    EnemyHit,
    HitVFX,
    OnDamage,
    OnHit,
    // 背包
    SlotChanged,
    SlotSelected,
    ItemDiscard,
    SlotSwap,
    ItemPickup
}

/// <summary>
/// 全局事件总线单例。所有模块间事件通过此类广播/订阅，
/// 无需挂载到 GameObject。
/// </summary>
public class EventBus
{
    private static EventBus _instance;  // 这里使用懒汉式单例
    public static EventBus Instance => _instance ??= new EventBus(); // ??= 空合并赋值运算符：如果左边为 null，把右边的值赋给左边

    private readonly Dictionary<E, Delegate> _events = new();

    private EventBus() { }

    #region AddListener   无参 / 单参 / 双参

    public void Add(E key, Action callback)
    {
        if (_events.TryGetValue(key, out var del))
            _events[key] = (Action)del + callback;
        else
            _events[key] = callback;
    }

    public void Add<T>(E key, Action<T> callback)
    {
        if (_events.TryGetValue(key, out var del))
            _events[key] = (Action<T>)del + callback;
        else
            _events[key] = callback;
    }

    public void Add<T1, T2>(E key, Action<T1, T2> callback)
    {
        if (_events.TryGetValue(key, out var del))
            _events[key] = (Action<T1, T2>)del + callback;
        else
            _events[key] = callback;
    }

    #endregion

    #region RemoveListener   无参 / 单参 / 双参

    public void Remove(E key, Action callback)
    {
        if (_events.TryGetValue(key, out var del))
        {
            var result = (Action)del - callback;
            if (result == null)
                _events.Remove(key);
            else
                _events[key] = result;
        }
    }

    public void Remove<T>(E key, Action<T> callback)
    {
        if (_events.TryGetValue(key, out var del))
        {
            var result = (Action<T>)del - callback;
            if (result == null)
                _events.Remove(key);
            else
                _events[key] = result;
        }
    }

    public void Remove<T1, T2>(E key, Action<T1, T2> callback)
    {
        if (_events.TryGetValue(key, out var del))
        {
            var result = (Action<T1, T2>)del - callback;
            if (result == null)
                _events.Remove(key);
            else
                _events[key] = result;
        }
    }

    #endregion

    #region Invoke   无参 / 单参 / 双参

    public void Invoke(E key)
    {
        if (_events.TryGetValue(key, out var del) && del is Action action)   // 检测是否可以转换为对应参数的Action类型
            action?.Invoke();
    }

    public void Invoke<T>(E key, T arg)
    {
        if (_events.TryGetValue(key, out var del) && del is Action<T> action)
            action?.Invoke(arg);
    }

    public void Invoke<T1, T2>(E key, T1 arg1, T2 arg2)
    {
        if (_events.TryGetValue(key, out var del) && del is Action<T1, T2> action)
            action?.Invoke(arg1, arg2);
    }

    #endregion
}
