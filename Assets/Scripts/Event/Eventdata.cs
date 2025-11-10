using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// ✨ 改进版 EventData
/// 新增功能：
/// 1. 支持连续事件（nextEventId）
/// 2. Inspector中可调整触发概率
/// 3. 更好的链式事件支持
/// </summary>
[System.Serializable]
public class EventData
{
    [Header("基本信息")]
    public string eventId;
    public string eventName;
    public EventCategory category;
    public string storyKey;
    
    [Header("触发设置")]
    [Range(0f, 1f)]
    [Tooltip("事件触发概率 (0-1，0=从不触发，1=必定触发)")]
    public float triggerProbability = 0.1f;  // ✨ 现在可在Inspector中调整
    
    public TriggerCondition[] conditions;
    
    [Header("选择选项")]
    public EventChoice[] choices;
    
    [Header("标志管理")]
    public string[] requiredFlags;
    public string[] excludedFlags;
    public string onTriggerFlag;
    
    [Header("连续事件 ✨")]
    [Tooltip("如果设置，事件结束后会自动触发这个后续事件")]
    public string chainedEventId;  // ✨ 新增：链式事件ID
    [Tooltip("链式事件的延迟时间（秒）")]
    public float chainedEventDelay = 1f;  // ✨ 新增：延迟
    
    [Header("其他")]
    public bool canSkip = true;
    [Range(0.1f, 5f)]
    [Tooltip("事件权重（用于随机选择，数值越大越容易被选中）")]
    public float eventWeight = 1f;
    
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
    /// ✨ 新增：检查是否有链式事件
    /// </summary>
    public bool HasChainedEvent()
    {
        return !string.IsNullOrEmpty(chainedEventId);
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
        Debug.Log($"链式事件: {(HasChainedEvent() ? chainedEventId : "无")}");
        Debug.Log($"=========================================\n");
    }
}

/// <summary>
/// ✨ 改进版 EventChoice
/// 新增功能：支持选择后的连续事件
/// </summary>
[System.Serializable]
public class EventChoice
{
    [Header("选择信息")]
    public string choiceId;
    public string choiceTextKey;
    public string resultTextKey;
    
    [Header("效果")]
    public string[] effects;
    
    [Header("后续 ✨")]
    public string onChoiceFlag;
    [Tooltip("选择此项后触发的下一个事件")]
    public string nextEventId;  // ✨ 每个选择都可以有自己的后续事件
    public bool endsConversation = true;
    
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

// 其他类型保持不变
public enum ConditionType
{
    Health, Valence, Arousal, Gold, WorkSkill, EmotionStability,
    DayOfWeek, TimeOfDay, ConsecutiveWorkDays, TotalGoldEarned, HasFlag
}

public enum ComparisonType 
{ 
    Less, Greater, Equal, LessOrEqual, GreaterOrEqual, NotEqual 
}

public enum EventCategory
{
    Personal, Work, Random, Special, Choice
}

[System.Serializable]
public struct TriggerCondition
{
    [Header("条件设置")]
    public ConditionType type;
    public float value;
    public ComparisonType comparison;
    public string flagName;
    
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
            "早上" => 0, "中午" => 1, "下午" => 2, "晚上" => 3,
            _ => -1
        };
    }
}

public struct EventTriggerResult
{
    public bool triggered;
    public EventData eventData;
    public string reason;
}