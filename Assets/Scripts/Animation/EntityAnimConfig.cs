using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 单个实体（Player / Goblin / Boss …）的全部动画状态配置。
/// </summary>
[System.Serializable]
public class EntityAnimConfig
{
    [Header("实体标识（与 StateManager 上的 Entity Id 保持一致）")]
    public string entityId;

    [Header("该实体的所有动画状态")]
    public List<StateAnimEntry> states;

    public StateAnimEntry GetStateConfig(string stateName)
    {
        if (states == null) return null;
        return states.Find(s => s.stateName == stateName);
    }
}
