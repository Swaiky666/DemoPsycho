using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 玩家资源结构
/// </summary>
[Serializable]
public class PlayerResources
{
    public float gold = 50f;   // 金币
    public float time = 7f;    // 每周时间（天数）
}

/// <summary>
/// 游戏状态管理器
/// 管理玩家的所有属性、资源、情绪等
/// 这是时间系统、工作系统、消费系统的核心
/// </summary>
public class AffectGameState : MonoBehaviour
{
    [Header("资源")]
    public PlayerResources res = new();

    [Header("属性")]
    [SerializeField] public float health = 100f;        // 健康值 (0-100)
    [SerializeField] public float valence = 0f;         // 情绪效价 (-10 ~ +10)
    [SerializeField] public float arousal = 0f;         // 情绪唤醒 (-10 ~ +10)

    [Header("可选")]
    [SerializeField] public float workSkill = 10f;      // 工作能力
    [SerializeField] public float emotionStability = 50f;  // 情绪稳定性

    // 回调
    public event System.Action<List<string>> OnEffectApplied;

    void Start()
    {
        Debug.Log("[AffectGameState] 游戏状态系统已初始化");
    }

    /// <summary>
    /// 应用一系列效果到当前状态
    /// 这是所有系统（工作、消费等）改变玩家状态的唯一接口
    /// </summary>
    public void ApplyEffect(List<string> effects)
    {
        if (effects == null || effects.Count == 0) return;

        foreach (var effect in effects)
        {
            var trimmed = effect.Trim().Replace(" ", "");
            if (string.IsNullOrEmpty(trimmed)) continue;

            // 解析效果格式: V+1, A-0.5, gold+100, health-10, time-0.5, etc.
            ParseAndApply(trimmed);
        }

        OnEffectApplied?.Invoke(effects);
    }

    /// <summary>
    /// 解析单个效果并应用
    /// </summary>
    private void ParseAndApply(string effect)
    {
        // setFlag / clrFlag 格式
        if (effect.StartsWith("setFlag:") || effect.StartsWith("clrFlag:"))
        {
            // 标志处理（如需要可扩展）
            return;
        }

        // 数值变化格式：[属性][+/-][数值]
        // 例: V+2, A-1, gold+50, health-10, time-0.5
        var match = System.Text.RegularExpressions.Regex.Match(
            effect, 
            @"^([VA]|gold|health|time|workSkill|emotionStability)([+\-])([\d\.]+)$"
        );

        if (!match.Success)
        {
            Debug.LogWarning($"[AffectGameState] 无法解析效果: {effect}");
            return;
        }

        string key = match.Groups[1].Value;
        string sign = match.Groups[2].Value;
        float value = float.Parse(match.Groups[3].Value);

        if (sign == "-") value = -value;

        switch (key)
        {
            case "V":
                valence += value;
                break;
            case "A":
                arousal += value;
                break;
            case "gold":
                res.gold += value;
                break;
            case "time":
                res.time += value;
                break;
            case "health":
                health = Mathf.Clamp(health + value, 0, 100);
                break;
            case "workSkill":
                workSkill += value;
                break;
            case "emotionStability":
                emotionStability = Mathf.Clamp(emotionStability + value, 0, 100);
                break;
        }
    }

    /// <summary>
    /// 快速检查状态的调试信息
    /// </summary>
    [ContextMenu("DEBUG: 打印当前状态")]
    public void DebugPrintState()
    {
        Debug.Log($"\n========== 游戏状态 ==========");
        Debug.Log($"💰 金币: {res.gold:F0}");
        Debug.Log($"⏰ 时间: {res.time:F1} 天");
        Debug.Log($"❤️ 健康: {health:F1}");
        Debug.Log($"😊 情绪: V={valence:F2}, A={arousal:F2}");
        Debug.Log($"💪 工作能力: {workSkill:F1}");
        Debug.Log($"🧠 情绪稳定性: {emotionStability:F1}");
        Debug.Log($"==============================\n");
    }
}