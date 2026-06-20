using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 编辑器工具：一键生成默认的 AnimationConfig 资产文件。
/// 用 Tool → Create Default Animation Config 运行。
/// </summary>
public static class AnimationConfigGenerator
{
    private const string AssetPath = "Assets/AnimationConfig.asset";

    [MenuItem("Tools/Create Default Animation Config")]
    public static void CreateDefaultConfig()
    {
        // 如果已存在，直接选中并提示
        var existing = AssetDatabase.LoadAssetAtPath<AnimationConfigSO>(AssetPath);
        if (existing != null)
        {
            EditorGUIUtility.PingObject(existing);
            Debug.Log($"[ConfigGenerator] 资产已存在于 {AssetPath}，已选中。可双击编辑。");
            return;
        }

        var config = ScriptableObject.CreateInstance<AnimationConfigSO>();
        config.globalDefaultDuration = 0.1f;
        config.entities = new List<EntityAnimConfig>
        {
            CreatePlayerConfig(),
            CreateGoblinConfig(),
        };

        AssetDatabase.CreateAsset(config, AssetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorGUIUtility.PingObject(config);
        Debug.Log($"[ConfigGenerator] 默认动画配置已创建于 {AssetPath}，请双击编辑。");
    }

    private static EntityAnimConfig CreatePlayerConfig()
    {
        return new EntityAnimConfig
        {
            entityId = "Player",
            states = new List<StateAnimEntry>
            {
                new() { stateName = "idle",         animationState = "idle",        transitionDuration = 0.1f  },
                new() { stateName = "isRun",   animationState = "run_start", transitionDuration = 0.05f },
                new() { stateName = "run_end", animationState = "run_brake", transitionDuration = 0.05f },
                new() { stateName = "isAttack",     animationState = "Attack_A",   transitionDuration = 0.05f,
                        comboAnimations = new[] { "Attack_A", "Attack_B", "Attack_C" } },
                new() { stateName = "exAttack",     animationState = "ExAttack",   transitionDuration = 0.05f },
                new() { stateName = "evade",        animationState = "Evade",      transitionDuration = 0.05f },
                new() { stateName = "isHit_Front",  animationState = "Hit_Front",  transitionDuration = 0.05f },
                new() { stateName = "isHit_Back",   animationState = "Hit_Back",   transitionDuration = 0.05f },
                new() { stateName = "interim",      animationState = "idle",       transitionDuration = 0.15f },
            }
        };
    }

    private static EntityAnimConfig CreateGoblinConfig()
    {
        return new EntityAnimConfig
        {
            entityId = "Goblin",
            states = new List<StateAnimEntry>
            {
                new() { stateName = "idle",             animationState = "idle",                 transitionDuration = 0.1f  },
                new() { stateName = "patrol",           animationState = "patrol",               transitionDuration = 0.1f  },
                new() { stateName = "isRun", animationState = "run_start", transitionDuration = 0.1f },
                new() { stateName = "isAttack",         animationState = "attack",               transitionDuration = 0.1f  },
                new() { stateName = "isVigilant_left",  animationState = "vigilant_left",        transitionDuration = 0.1f  },
                new() { stateName = "isVigilant_right", animationState = "vigilant_right",       transitionDuration = 0.1f  },
                new() { stateName = "isHit",            animationState = "hit",                  transitionDuration = 0.05f },
                new() { stateName = "interim",          animationState = "idle",                 transitionDuration = 0.15f },
            }
        };
    }
}
