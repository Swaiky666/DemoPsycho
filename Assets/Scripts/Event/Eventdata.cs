using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 触发条件类型
/// </summary>
public enum ConditionType
{
    Health,                 // 健康值
    Valence,               // 情绪效价
    Arousal,               // 情绪唤醒
    Gold,                  // 金币
    WorkSkill,             // 工作能力
    EmotionStability,      // 情绪稳定性
    DayOfWeek,             // 周几 (1-7)
    TimeOfDay,             // 时段 (0=早上, 1=中午, 2=下午, 3=晚上)
    ConsecutiveWorkDays,   // 连续工作天数
    TotalGoldEarned,       // 累计金币
    HasFlag                // 是否有特定标志
}

/// <summary>
/// 比较方式
/// </summary>
public enum ComparisonType { Less, Greater, Equal, LessOrEqual, GreaterOrEqual, NotEqual }

/// <summary>
/// 事件分类
/// </summary>
public enum EventCategory
{
    Personal,              // 个人事件（过劳、抑郁等）
    Work,                  // 工作相关
    Random,                // 随机事件
    Special,               // 特殊/里程碑事件
    Choice                 // 基于选择的事件
}

/// <summary>
/// 触发条件
/// </summary>
[System.Serializable]
public struct TriggerCondition
{
    [Header("条件设置")]
    public ConditionType type;              // 条件类型
    public float value;                     // 条件值
    public ComparisonType comparison;       // 比较方式
    
    // 对于 HasFlag 类型
    public string flagName;                 // 标志名称（仅用于 HasFlag）
    
    /// <summary>
    /// 检查条件是否满足
    /// </summary>
    public bool IsSatisfied(AffectGameState gameState, TimeManager timeManager, GameFlagManager flagManager)
    {
        float checkValue = GetCheckValue(gameState, timeManager, flagManager);
        
        return comparison switch
        {
            ComparisonType.Less => checkValue < value,
            ComparisonType.Greater => checkValue > value,
            ComparisonType.Equal => Mathf.Approximately(checkValue, value),
            ComparisonType.LessOrEqual => checkValue <= value,
            ComparisonType.GreaterOrEqual => checkValue >= value,
            ComparisonType.NotEqual => !Mathf.Approximately(checkValue, value),
            _ => false
        };
    }
    
    /// <summary>
    /// 获取要检查的值
    /// </summary>
    private float GetCheckValue(AffectGameState gameState, TimeManager timeManager, GameFlagManager flagManager)
    {
        return type switch
        {
            ConditionType.Health => gameState.health,
            ConditionType.Valence => gameState.valence,
            ConditionType.Arousal => gameState.arousal,
            ConditionType.Gold => gameState.res.gold,
            ConditionType.WorkSkill => gameState.workSkill,
            ConditionType.EmotionStability => gameState.emotionStability,
            ConditionType.DayOfWeek => timeManager.GetCurrentDay(),
            ConditionType.TimeOfDay => GetCurrentTimeSlotIndex(timeManager),
            ConditionType.HasFlag => flagManager.HasFlag(flagName) ? 1 : 0,
            _ => 0
        };
    }
    
    private int GetCurrentTimeSlotIndex(TimeManager timeManager)
    {
        string slotName = timeManager.GetCurrentTimeSlotName();
        return slotName switch
        {
            "早上" => 0,
            "中午" => 1,
            "下午" => 2,
            "晚上" => 3,
            _ => -1
        };
    }
}

/// <summary>
/// 事件选择选项
/// </summary>
[System.Serializable]
public class EventChoice
{
    [Header("选择信息")]
    public string choiceId;                 // 选择ID (event_001_choice_a)
    public string choiceTextKey;            // 本地化选择文本 key
    public string resultTextKey;            // 结果文本 key (可选)
    
    [Header("效果")]
    public string[] effects;                // 效果列表 ("V+1", "gold-50", "health+10")
    
    [Header("后续")]
    public string onChoiceFlag;             // 选择后设置的标志
    public string nextEventId;              // 后续事件ID (可选)
    public bool endsConversation = true;    // 是否结束对话
    
    /// <summary>
    /// 获取选择文本（本地化）
    /// </summary>
    public string GetChoiceText()
    {
        if (LocalizationManager.Instance != null)
        {
            return LocalizationManager.Instance.GetString(choiceTextKey);
        }
        return choiceTextKey;
    }
    
    /// <summary>
    /// 获取结果文本（本地化）
    /// </summary>
    public string GetResultText()
    {
        if (string.IsNullOrEmpty(resultTextKey)) return "";
        
        if (LocalizationManager.Instance != null)
        {
            return LocalizationManager.Instance.GetString(resultTextKey);
        }
        return resultTextKey;
    }
}

/// <summary>
/// 完整的事件数据结构
/// </summary>
[System.Serializable]
public class EventData
{
    [Header("基本信息")]
    public string eventId;                  // 唯一ID
    public string eventName;                // 事件名称
    public EventCategory category;          // 事件分类
    public string storyKey;                 // 本地化故事文本 key
    
    [Header("触发设置")]
    [Range(0f, 1f)]
    public float triggerProbability = 0.1f; // 触发概率
    public TriggerCondition[] conditions;   // 触发条件 (AND 逻辑)
    
    [Header("选择选项")]
    public EventChoice[] choices;           // 2-3 个选择
    
    [Header("标志管理")]
    public string[] requiredFlags;          // 必需的标志集合
    public string[] excludedFlags;          // 互斥的标志集合
    public string onTriggerFlag;            // 触发后设置的标志
    
    [Header("其他")]
    public bool canSkip = true;             // 是否可跳过
    public float eventWeight = 1f;          // 权重（用于随机选择）
    
    /// <summary>
    /// 获取故事文本（本地化）
    /// </summary>
    public string GetStoryText()
    {
        if (LocalizationManager.Instance != null)
        {
            return LocalizationManager.Instance.GetString(storyKey);
        }
        return storyKey;
    }
    
    /// <summary>
    /// 快速调试
    /// </summary>
    public void DebugPrint()
    {
        Debug.Log($"\n========== 事件信息: {eventName} ==========");
        Debug.Log($"事件 ID: {eventId}");
        Debug.Log($"分类: {category}");
        Debug.Log($"触发概率: {triggerProbability * 100}%");
        Debug.Log($"条件数: {conditions?.Length ?? 0}");
        Debug.Log($"选择数: {choices?.Length ?? 0}");
        Debug.Log($"触发后标志: {onTriggerFlag}");
        Debug.Log($"=========================================\n");
    }
}

/// <summary>
/// 事件触发结果
/// </summary>
public struct EventTriggerResult
{
    public bool triggered;                  // 是否触发了事件
    public EventData eventData;             // 触发的事件数据
    public string reason;                   // 触发原因（用于调试）
}