using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 事件管理系统核心 - 书本显示版
/// 使用BookEventDisplay替代EventUIPanel
/// </summary>
public class EventManager : MonoBehaviour
{
    [Header("系统引用")]
    [SerializeField] private AffectGameState gameState;
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private GameFlagManager flagManager;
    
    [Header("事件数据库")]
    [SerializeField] private EventDatabase eventDatabase;
    
    [Header("故事提供者")]
    [SerializeField] private EventStoryProvider storyProvider;
    
    [Header("UI 引用 - 书本显示")]
    [SerializeField] private BookEventDisplay bookEventDisplay;
    
    [Header("✨ 触发概率设置")]
    [SerializeField] [Range(0f, 1f)] private float dailyEventTriggerProbability = 0.3f;
    [SerializeField] [Range(1, 5)] private int maxEventsPerDay = 1;
    
    [Header("调试模式")]
    [SerializeField] private bool debugMode = true;
    [SerializeField] private bool alwaysTriggerEvents = false;

    // 当前事件
    private EventData currentEvent;
    private bool isEventActive = false;
    
    // 事件历史
    private List<string> triggeredEventIds = new();
    private int eventsTriggeredToday = 0;
    private Dictionary<string, int> eventTriggerCount = new();
    
    // 单例
    public static EventManager Instance { get; private set; }
    
    // 事件委托
    public delegate void OnEventTriggeredDelegate(EventData eventData);
    public delegate void OnEventChoiceSelectedDelegate(EventData eventData, EventChoice choice);
    public delegate void OnEventEndedDelegate(EventData eventData);
    
    public event OnEventTriggeredDelegate OnEventTriggered;
    public event OnEventChoiceSelectedDelegate OnEventChoiceSelected;
    public event OnEventEndedDelegate OnEventEnded;

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
        // 自动查找系统引用
        if (gameState == null) gameState = FindObjectOfType<AffectGameState>();
        if (timeManager == null) timeManager = FindObjectOfType<TimeManager>();
        if (flagManager == null) flagManager = FindObjectOfType<GameFlagManager>();
        if (bookEventDisplay == null) bookEventDisplay = FindObjectOfType<BookEventDisplay>();
        
        // 加载事件数据库
        if (eventDatabase == null)
        {
            eventDatabase = Resources.Load<EventDatabase>("Events/EventDatabase");
            if (eventDatabase == null)
            {
                eventDatabase = FindObjectOfType<EventDatabase>();
            }
        }
        
        // 初始化数据库
        if (eventDatabase != null)
        {
            eventDatabase.Initialize();
            Debug.Log($"[EventManager] ✅ 事件数据库已加载，共 {eventDatabase.GetEventCount()} 个事件");
        }
        else
        {
            Debug.LogError("[EventManager] ❌ 无法加载事件数据库！");
        }
        
        // 初始化故事提供者
        if (storyProvider == null)
        {
            storyProvider = FindObjectOfType<EventStoryProvider>();
            if (storyProvider == null)
            {
                storyProvider = gameObject.AddComponent<EventStoryProvider>();
            }
        }
        
        // 订阅事件
        if (timeManager != null)
        {
            timeManager.onDayChanged += OnDayChanged;
        }
        
        Debug.Log($"[EventManager] ✅ 系统初始化完成");
        Debug.Log($"[EventManager] 📊 每日触发概率: {dailyEventTriggerProbability * 100}%");
        Debug.Log($"[EventManager] 📊 每天最多事件数: {maxEventsPerDay}");
    }

    void OnDestroy()
    {
        if (timeManager != null)
        {
            timeManager.onDayChanged -= OnDayChanged;
        }
    }

    /// <summary>
    /// 每天的回调
    /// </summary>
    private void OnDayChanged()
    {
        eventsTriggeredToday = 0;
        
        if (debugMode)
        {
            Debug.Log($"[EventManager] 🌅 新的一天开始，事件计数已重置");
        }
        
        TryTriggerDailyEvent();
    }

    /// <summary>
    /// 尝试触发每日随机事件
    /// </summary>
    private void TryTriggerDailyEvent()
    {
        if (eventsTriggeredToday >= maxEventsPerDay)
        {
            if (debugMode)
            {
                Debug.Log($"[EventManager] ⏹️ 今日事件已达上限 ({maxEventsPerDay})");
            }
            return;
        }
        
        float randomValue = UnityEngine.Random.Range(0f, 1f);
        
        if (debugMode)
        {
            Debug.Log($"[EventManager] 🎲 每日事件触发检查：");
            Debug.Log($"  • 随机值: {randomValue:F3}");
            Debug.Log($"  • 触发阈值: {dailyEventTriggerProbability:F3}");
            Debug.Log($"  • 今日已触发: {eventsTriggeredToday}/{maxEventsPerDay}");
        }
        
        if (alwaysTriggerEvents)
        {
            Debug.Log($"[EventManager] 🔧 测试模式：强制触发事件");
        }
        else if (randomValue > dailyEventTriggerProbability)
        {
            if (debugMode)
            {
                Debug.Log($"[EventManager] ❌ 今日没有触发随机事件");
            }
            return;
        }
        
        var randomEvent = SelectRandomEvent();
        
        if (randomEvent != null && CheckAllConditions(randomEvent))
        {
            if (debugMode)
            {
                Debug.Log($"[EventManager] ✅ 触发每日事件: {randomEvent.eventName}");
            }
            TriggerEvent(randomEvent, "daily_random");
        }
        else
        {
            if (debugMode)
            {
                Debug.Log($"[EventManager] ⚠️ 没有满足条件的事件");
            }
        }
    }

    /// <summary>
    /// 选择随机事件
    /// </summary>
    private EventData SelectRandomEvent()
    {
        if (eventDatabase == null || eventDatabase.events.Length == 0)
        {
            Debug.LogWarning("[EventManager] ⚠️ 事件数据库为空！");
            return null;
        }
        
        var validEvents = eventDatabase.events
            .Where(e => 
                (e.category == EventCategory.Random || e.category == EventCategory.Personal) &&
                CheckAllConditions(e) &&
                CheckFlagRequirements(e)
            )
            .ToList();
        
        if (validEvents.Count == 0)
        {
            if (debugMode)
                Debug.Log("[EventManager] ⚠️ 没有满足条件的随机事件");
            return null;
        }
        
        if (debugMode)
        {
            Debug.Log($"[EventManager] 📋 找到 {validEvents.Count} 个满足条件的事件");
        }
        
        float totalWeight = validEvents.Sum(e => e.eventWeight);
        float randomValue = UnityEngine.Random.Range(0f, totalWeight);
        
        float accumulatedWeight = 0f;
        foreach (var eventData in validEvents)
        {
            accumulatedWeight += eventData.eventWeight;
            if (randomValue <= accumulatedWeight)
            {
                if (debugMode)
                {
                    Debug.Log($"[EventManager] 🎯 选中事件: {eventData.eventName}");
                }
                return eventData;
            }
        }
        
        return validEvents.LastOrDefault();
    }

    /// <summary>
    /// 检查所有条件
    /// </summary>
    private bool CheckAllConditions(EventData eventData)
    {
        if (eventData.conditions == null || eventData.conditions.Length == 0)
            return true;
        
        foreach (var condition in eventData.conditions)
        {
            if (!condition.IsSatisfied(gameState, timeManager, flagManager))
            {
                if (debugMode)
                {
                    Debug.Log($"[EventManager] ❌ {eventData.eventName} 条件不满足");
                }
                return false;
            }
        }
        
        return true;
    }

    /// <summary>
    /// 检查标志要求
    /// </summary>
    private bool CheckFlagRequirements(EventData eventData)
    {
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
    /// 直接触发指定事件
    /// </summary>
    public bool TriggerEvent(string eventId, string reason = "manual")
    {
        if (eventDatabase == null)
        {
            Debug.LogError("[EventManager] ❌ 事件数据库未加载！");
            return false;
        }
        
        var eventData = eventDatabase.GetEventById(eventId);
        if (eventData == null)
        {
            Debug.LogError($"[EventManager] ❌ 事件不存在: {eventId}");
            return false;
        }
        
        return TriggerEvent(eventData, reason);
    }

    /// <summary>
    /// 触发事件（内部）
    /// </summary>
    private bool TriggerEvent(EventData eventData, string reason)
    {
        if (isEventActive)
        {
            Debug.LogWarning("[EventManager] ⚠️ 已有事件在进行中");
            return false;
        }
        
        if (bookEventDisplay == null)
        {
            Debug.LogError("[EventManager] ❌ BookEventDisplay 未分配！");
            return false;
        }
        
        currentEvent = eventData;
        isEventActive = true;
        eventsTriggeredToday++;
        
        // 统计
        if (!eventTriggerCount.ContainsKey(eventData.eventId))
            eventTriggerCount[eventData.eventId] = 0;
        eventTriggerCount[eventData.eventId]++;
        triggeredEventIds.Add(eventData.eventId);
        
        // 设置触发后标志
        if (!string.IsNullOrEmpty(eventData.onTriggerFlag))
        {
            flagManager.SetFlag(eventData.onTriggerFlag);
        }
        
        // 广播事件
        OnEventTriggered?.Invoke(eventData);
        
        // 显示事件在书本上
        bookEventDisplay.ShowEvent(eventData);
        
        if (debugMode)
        {
            Debug.Log($"\n[EventManager] ========== 事件触发 ==========");
            Debug.Log($"✅ 事件: {eventData.eventName} ({eventData.eventId})");
            Debug.Log($"📝 原因: {reason}");
            Debug.Log($"📊 今日第 {eventsTriggeredToday} 个事件");
            Debug.Log($"📊 该事件总触发次数: {eventTriggerCount[eventData.eventId]}");
            Debug.Log($"==========================================\n");
        }
        
        return true;
    }

    /// <summary>
    /// 处理玩家选择
    /// </summary>
    public void OnPlayerChoice(int choiceIndex)
    {
        if (!isEventActive || currentEvent == null)
        {
            Debug.LogWarning("[EventManager] ⚠️ 没有活跃事件");
            return;
        }
        
        if (choiceIndex < 0 || choiceIndex >= currentEvent.choices.Length)
        {
            Debug.LogError($"[EventManager] ❌ 无效的选择索引: {choiceIndex}");
            return;
        }
        
        var choice = currentEvent.choices[choiceIndex];
        
        // 应用效果
        ApplyChoiceEffects(choice);
        
        // 设置选择标志
        if (!string.IsNullOrEmpty(choice.onChoiceFlag))
        {
            flagManager.SetFlag(choice.onChoiceFlag);
        }
        
        // 广播事件
        OnEventChoiceSelected?.Invoke(currentEvent, choice);
        
        if (debugMode)
        {
            Debug.Log($"[EventManager] 🎯 玩家选择: {choice.GetChoiceText()}");
        }
        
        // 检查后续事件
        if (!string.IsNullOrEmpty(choice.nextEventId))
        {
            Invoke(nameof(TriggerNextEvent), 1f);
            nextEventId = choice.nextEventId;
        }
        else
        {
            EndCurrentEvent();
        }
    }

    private static string nextEventId;
    
    private void TriggerNextEvent()
    {
        if (!string.IsNullOrEmpty(nextEventId))
        {
            TriggerEvent(nextEventId, "chained_event");
            nextEventId = null;
        }
    }

    /// <summary>
    /// 应用选择效果
    /// </summary>
    private void ApplyChoiceEffects(EventChoice choice)
    {
        if (choice.effects == null || choice.effects.Length == 0)
            return;
        
        if (gameState == null)
        {
            Debug.LogError("[EventManager] ❌ GameState 为空！");
            return;
        }
        
        gameState.ApplyEffect(new List<string>(choice.effects));
        
        if (debugMode)
        {
            Debug.Log($"[EventManager] ✅ 效果已应用: {string.Join(", ", choice.effects)}");
        }
    }

    /// <summary>
    /// 结束当前事件
    /// </summary>
    private void EndCurrentEvent()
    {
        if (currentEvent == null) return;
        
        OnEventEnded?.Invoke(currentEvent);
        
        currentEvent = null;
        isEventActive = false;
        
        if (debugMode)
        {
            Debug.Log("[EventManager] ✅ 事件已结束");
        }
    }

    /// <summary>
    /// 获取当前事件
    /// </summary>
    public EventData GetCurrentEvent()
    {
        return currentEvent;
    }

    /// <summary>
    /// 检查是否有活跃事件
    /// </summary>
    public bool IsEventActive()
    {
        return isEventActive;
    }

    /// <summary>
    /// 获取事件统计
    /// </summary>
    public int GetEventTriggerCount(string eventId)
    {
        return eventTriggerCount.ContainsKey(eventId) ? eventTriggerCount[eventId] : 0;
    }

    /// <summary>
    /// 获取已触发事件列表
    /// </summary>
    public List<string> GetTriggeredEventIds()
    {
        return new List<string>(triggeredEventIds);
    }

    // ===== 调试方法 =====

    [ContextMenu("DEBUG: 打印事件统计")]
    public void DebugPrintEventStats()
    {
        Debug.Log($"\n========== 事件统计 ==========");
        Debug.Log($"总触发次数: {triggeredEventIds.Count}");
        Debug.Log($"已触发事件数: {eventTriggerCount.Count}");
        Debug.Log($"今日触发数: {eventsTriggeredToday}/{maxEventsPerDay}");
        Debug.Log($"触发概率设置: {dailyEventTriggerProbability * 100}%");
        
        Debug.Log("\n--- 触发列表 ---");
        foreach (var kvp in eventTriggerCount.OrderByDescending(x => x.Value))
        {
            Debug.Log($"  {kvp.Key}: {kvp.Value} 次");
        }
        
        Debug.Log($"==============================\n");
    }

    [ContextMenu("DEBUG: 列出所有事件")]
    public void DebugListAllEvents()
    {
        if (eventDatabase == null)
        {
            Debug.LogError("[EventManager] ❌ 事件数据库未加载");
            return;
        }
        
        Debug.Log($"\n========== 所有事件 (共 {eventDatabase.events.Length} 个) ==========");
        
        int index = 1;
        foreach (var eventData in eventDatabase.events)
        {
            Debug.Log($"{index}. {eventData.eventName} ({eventData.eventId})");
            Debug.Log($"   分类: {eventData.category}, 概率: {eventData.triggerProbability * 100}%");
            Debug.Log($"   选择数: {eventData.choices.Length}");
            index++;
        }
        
        Debug.Log($"================================================\n");
    }

    [ContextMenu("DEBUG: 强制触发随机事件")]
    public void DebugTriggerRandomEvent()
    {
        var randomEvent = SelectRandomEvent();
        if (randomEvent != null)
        {
            TriggerEvent(randomEvent, "debug_manual");
        }
        else
        {
            Debug.Log("[DEBUG] 没有满足条件的事件");
        }
    }
    
    [ContextMenu("DEBUG: 切换测试模式")]
    public void DebugToggleTestMode()
    {
        alwaysTriggerEvents = !alwaysTriggerEvents;
        Debug.Log($"[DEBUG] 测试模式: {(alwaysTriggerEvents ? "ON" : "OFF")}");
    }
}