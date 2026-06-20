using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 集中管理所有实体（角色/怪物）的动画配置。
/// 在 Project 窗口中右键 → Create → Custom → Entity Animation Config 创建资产文件。
/// </summary>
[CreateAssetMenu(menuName = "Custom/Entity Animation Config", fileName = "AnimationConfig")]
public class AnimationConfigSO : ScriptableObject
{
    [Header("全局默认过渡时间（秒）")]
    public float globalDefaultDuration = 0.1f;

    [Header("各实体的动画配置（Player / Goblin / Boss …）")]
    public List<EntityAnimConfig> entities;

    public EntityAnimConfig GetEntityConfig(string entityId)
    {
        if (entities == null) return null;
        return entities.Find(e => e.entityId == entityId);
    }
}
