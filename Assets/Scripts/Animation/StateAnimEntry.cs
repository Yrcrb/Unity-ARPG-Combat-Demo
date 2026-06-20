using UnityEngine;

/// <summary>
/// 单个状态（idle / isRun / isAttack / isHit …）的动画信息。
/// </summary>
[System.Serializable]
public class StateAnimEntry
{
    [Header("状态枚举名（如 idle、isRun、isAttack）")]
    public string stateName;

    [Header("Animator Controller 中该状态的入口动画名")]
    [Tooltip("CrossFade 到此动画。run_start 会由 Animator 自动过渡到 run_loop 循环")]
    public string animationState;

    [Header("过渡时间（秒），-1 表示使用全局默认值")]
    public float transitionDuration = -1f;

    [Header("播放速度倍率（1 = 原速）")]
    public float speedMultiplier = 1f;

    [Header("连击动画列表（可选，仅攻击状态需要）")]
    public string[] comboAnimations;
}
