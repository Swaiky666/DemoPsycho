using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 事件触发管理器 - 核心系统
/// 基于故事线、游戏状态和条件来智能触发事件
/// 
/// ✅ 修改：使用 GetEventDatabase() 方法访问EventDatabase
/// 职责：
/// 1. 根据故事阶段判断何时触发哪些事件
/// 2. 根据玩家选择历史调整事件触发
/// 3. 确保事件序列的连贯性
/// 4. 管理事件之间的因果关系
/// </summary>
public class EventTriggerManager : MonoBehaviour
{
    [Header("系统引用")]
    [SerializeField] private EventManager eventManager;
    [SerializeField] private StoryArcSystem storyArcSystem;
    [SerializeField] private AffectGameState gameState;
    [SerializeField] private GameFlagManager flagManager;
    [SerializeField] private TimeManager timeManager;
    
    [Header("触发配置")]
    [SerializeField] private bool debugMode = true;
    
    // 故事线特定的事件池
    private Dictionary<StoryPhase, List<string>> careerEventsByPhase = new();
    private Dictionary<StoryPhase, List<string>> mentalHealthEventsByPhase = new();
    private Dictionary<StoryPhase, List<string>> relationshipEventsByPhase = new();
    private Dictionary<StoryPhase, List<string>> financialEventsByPhase = new();
    
    // 事件冷却时间
    private Dictionary<string, float> eventCooldowns = new();
    
    public static EventTriggerManager Instance { get; private set; }
    
    // 事件
    public event Action<string> OnEventAboutToTrigger;
    public event Action<string> OnEventBlocked;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // 自动查找引用
        if (eventManager == null) eventManager = FindObjectOfType<EventManager>();
        if (storyArcSystem == null) storyArcSystem = FindObjectOfType<StoryArcSystem>();
        if (gameState == null) gameState = FindObjectOfType<AffectGameState>();
        if (flagManager == null) flagManager = FindObjectOfType<GameFlagManager>();
        if (timeManager == null) timeManager = FindObjectOfType<TimeManager>();
        
        InitializeEventPhaseMapping();
        Debug.Log("[EventTriggerManager] ✅ 事件触发管理器已初始化");
    }

    /// <summary>
    /// 初始化故事阶段-事件映射表
    /// 定义每个故事阶段应该触发哪些事件
    /// </summary>
    private void InitializeEventPhaseMapping()
    {
        // ===== 职业线事件 =====
        careerEventsByPhase[StoryPhase.Exploration] = new List<string>
        {
            "event_job_opportunity",
            "event_weekend",
            "event_random_meeting"
        };
        
        careerEventsByPhase[StoryPhase.Development] = new List<string>
        {
            "event_job_opportunity",
            "event_work_conflict",
            "event_overwork"
        };
        
        careerEventsByPhase[StoryPhase.CriticalTurning] = new List<string>
        {
            "event_promotion",      // 升迁机会
            "event_job_loss",       // 或者失业
            "event_burnout"         // 或者倦怠
        };
        
        careerEventsByPhase[StoryPhase.Harvest] = new List<string>
        {
            "event_wealth",
            "event_investment"
        };
        
        // ===== 心理健康线事件 =====
        mentalHealthEventsByPhase[StoryPhase.Stable] = new List<string>
        {
            "event_first_week"
        };
        
        mentalHealthEventsByPhase[StoryPhase.Fluctuation] = new List<string>
        {
            "event_loneliness",
            "event_depression",
            "event_friendship"
        };
        
        mentalHealthEventsByPhase[StoryPhase.CriticalTurning] = new List<string>
        {
            "event_depression",      // 情绪低谷
            "event_health_crisis",   // 健康危机
            "event_burnout"          // 工作倦怠
        };
        
        mentalHealthEventsByPhase[StoryPhase.NewBalance] = new List<string>
        {
            "event_serendipity",
            "event_month_anniversary"
        };
        
        // ===== 关系线事件 =====
        relationshipEventsByPhase[StoryPhase.Isolation] = new List<string>
        {
            "event_loneliness"
        };
        
        relationshipEventsByPhase[StoryPhase.Connection] = new List<string>
        {
            "event_friendship",
            "event_random_meeting"
        };
        
        relationshipEventsByPhase[StoryPhase.CriticalTurning] = new List<string>
        {
            "event_friendship",     // 友谊深化
            "event_work_conflict"   // 冲突
        };
        
        relationshipEventsByPhase[StoryPhase.NewRelationship] = new List<string>
        {
            "event_random_meeting",
            "event_serendipity"
        };
        
        // ===== 财务线事件 =====
        financialEventsByPhase[StoryPhase.Poverty] = new List<string>
        {
            "event_poverty",
            "event_unexpected_expense"
        };
        
        financialEventsByPhase[StoryPhase.Stability] = new List<string>
        {
            "event_wealth",
            "event_investment"
        };
        
        financialEventsByPhase[StoryPhase.CriticalTurning] = new List<string>
        {
            "event_investment",
            "event_unexpected_expense"
        };
        
        financialEventsByPhase[StoryPhase.Comfort] = new List<string>
        {
            "event_wealth",
            "event_serendipity"
        };
    }

    /// <summary>
    /// 每天调用一次，尝试触发故事线相关事件
    /// </summary>
    public void TriggerDailyStoryEvent(int dayOfMonth)
    {
        if (eventManager == null || storyArcSystem == null)
        {
            Debug.LogError("[EventTriggerManager] ❌ 缺少必要的系统引用");
            return;
        }
        
        // 获取当前故事线的阶段
        var careerPhase = storyArcSystem.GetCareerPhase();
        var mentalPhase = storyArcSystem.GetMentalHealthPhase();
        var relationshipPhase = storyArcSystem.GetRelationshipPhase();
        var financialPhase = storyArcSystem.GetFinancialPhase();
        
        if (debugMode)
        {
            Debug.Log($"\n[EventTriggerManager] 📅 Day {dayOfMonth} 故事事件检查");
            Debug.Log($"  职业: {careerPhase}");
            Debug.Log($"  心理: {mentalPhase}");
            Debug.Log($"  关系: {relationshipPhase}");
            Debug.Log($"  财务: {financialPhase}");
        }
        
        // 尝试从各条线触发事件（优先级：职业 > 心理 > 关系 > 财务）
        bool triggered = false;
        
        triggered = triggered || TryTriggerEventFromPhase(careerPhase, careerEventsByPhase, "career");
        if (!triggered) triggered = triggered || TryTriggerEventFromPhase(mentalPhase, mentalHealthEventsByPhase, "mental");
        if (!triggered) triggered = triggered || TryTriggerEventFromPhase(relationshipPhase, relationshipEventsByPhase, "relationship");
        if (!triggered) triggered = triggered || TryTriggerEventFromPhase(financialPhase, financialEventsByPhase, "financial");
        
        if (debugMode && !triggered)
        {
            Debug.Log("[EventTriggerManager] ℹ️ 今天没有触发故事线事件");
        }
    }

    /// <summary>
    /// 从特定阶段的事件池中随机选择并触发一个事件
    /// </summary>
    private bool TryTriggerEventFromPhase(StoryPhase phase, Dictionary<StoryPhase, List<string>> eventPool, string storyLineName)
    {
        if (!eventPool.ContainsKey(phase))
            return false;
        
        var events = eventPool[phase];
        if (events == null || events.Count == 0)
            return false;
        
        // 过滤掉冷却中的事件
        var availableEvents = events.Where(eventId => !IsEventOnCooldown(eventId)).ToList();
        
        if (availableEvents.Count == 0)
        {
            if (debugMode)
                Debug.Log($"[EventTriggerManager] ❌ {storyLineName}线: 所有事件都在冷却中");
            return false;
        }
        
        // 随机选择一个事件
        string selectedEventId = availableEvents[UnityEngine.Random.Range(0, availableEvents.Count)];
        
        // 检查条件
        if (!CheckEventConditions(selectedEventId))
        {
            if (debugMode)
                Debug.Log($"[EventTriggerManager] ⏸️ {storyLineName}线: {selectedEventId} 条件不满足");
            OnEventBlocked?.Invoke(selectedEventId);
            return false;
        }
        
        // 触发事件
        OnEventAboutToTrigger?.Invoke(selectedEventId);
        bool success = eventManager.TriggerEvent(selectedEventId, $"story_{storyLineName}");
        
        if (success)
        {
            SetEventCooldown(selectedEventId, 3f);  // 3天冷却
            if (debugMode)
                Debug.Log($"[EventTriggerManager] ✅ {storyLineName}线: 触发事件 {selectedEventId}");
        }
        
        return success;
    }

    /// <summary>
    /// 检查事件的所有条件是否满足
    /// ✅ 修改：使用 GetEventDatabase() 方法
    /// </summary>
    private bool CheckEventConditions(string eventId)
    {
        if (eventManager == null)
            return false;
        
        var eventData = eventManager.GetEventDatabase().GetEventById(eventId);
        if (eventData == null)
            return false;
        
        // 检查条件
        if (eventData.conditions != null && eventData.conditions.Length > 0)
        {
            foreach (var condition in eventData.conditions)
            {
                if (!condition.IsSatisfied(gameState, timeManager, flagManager))
                    return false;
            }
        }
        
        // 检查标志要求
        if (eventData.requiredFlags != null && eventData.requiredFlags.Length > 0)
        {
            if (!flagManager.HasAllFlags(eventData.requiredFlags))
                return false;
        }
        
        if (eventData.excludedFlags != null && eventData.excludedFlags.Length > 0)
        {
            if (flagManager.HasAnyFlag(eventData.excludedFlags))
                return false;
        }
        
        return true;
    }

    /// <summary>
    /// 根据玩家选择调整后续事件触发
    /// </summary>
    public void AdjustTriggerProbabilityBasedOnChoice(string choiceId)
    {
        if (debugMode)
        {
            Debug.Log($"[EventTriggerManager] 📊 玩家选择 {choiceId}，调整触发概率");
        }
        
        // 这里可以实现基于选择的动态调整
    }

    /// <summary>
    /// 事件冷却管理
    /// </summary>
    private void SetEventCooldown(string eventId, float cooldownDays)
    {
        eventCooldowns[eventId] = cooldownDays;
    }

    private bool IsEventOnCooldown(string eventId)
    {
        return eventCooldowns.ContainsKey(eventId) && eventCooldowns[eventId] > 0;
    }

    void Update()
    {
        // 每天递减冷却时间
        var keysToUpdate = new List<string>(eventCooldowns.Keys);
        foreach (var key in keysToUpdate)
        {
            eventCooldowns[key] -= Time.deltaTime / (24f * 3600f);
        }
    }

    /// <summary>
    /// 立即触发指定的故事里程碑事件
    /// </summary>
    public void TriggerMilestoneEvent(string milestone)
    {
        if (debugMode)
        {
            Debug.Log($"[EventTriggerManager] 🎯 触发里程碑: {milestone}");
        }
        
        var eventId = milestone switch
        {
            "first_week_completed" => "event_first_week",
            "first_month_completed" => "event_month_anniversary",
            "story_week3_all_lines_climax" => "event_life_change",
            _ => null
        };
        
        if (eventId != null)
        {
            eventManager.TriggerEvent(eventId, "milestone");
        }
    }

    /// <summary>
    /// 打印当前的故事线事件映射
    /// </summary>
    [ContextMenu("DEBUG: 打印事件映射")]
    public void DebugPrintEventMapping()
    {
        Debug.Log("\n========== 职业线事件映射 ==========");
        foreach (var kvp in careerEventsByPhase)
        {
            Debug.Log($"{kvp.Key}: {string.Join(", ", kvp.Value)}");
        }
        Debug.Log("=====================================\n");
    }

    /// <summary>
    /// 测试触发指定的故事事件
    /// </summary>
    [ContextMenu("DEBUG: 测试触发故事事件")]
    public void DebugTestTriggerStoryEvent()
    {
        TriggerDailyStoryEvent(1);
    }
}