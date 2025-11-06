using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 事件管理系统核心
/// 
/// 职责：
/// 1. 加载事件数据库
/// 2. 处理事件触发（概率、条件、标志）
/// 3. 管理事件显示和选择
/// 4. 应用事件效果
/// 5. 与其他系统整合
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
    
    [Header("UI 引用")]
    [SerializeField] private EventUIPanel eventUIPanel;
    
    [Header("调试模式")]
    [SerializeField] private bool debugMode = true;

    // 当前事件
    private EventData currentEvent;
    private bool isEventActive = false;
    
    // 事件历史
    private List<string> triggeredEventIds = new();
    private int eventsTriggeredToday = 0;
    private Dictionary<string, int> eventTriggerCount = new();  // 统计信息
    
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
        if (eventUIPanel == null) eventUIPanel = FindObjectOfType<EventUIPanel>();
        
        // 加载事件数据库
        if (eventDatabase == null)
        {
            eventDatabase = Resources.Load<EventDatabase>("Events/EventDatabase");
        }
        
        // 初始化故事提供者
        if (storyProvider == null)
        {
            storyProvider = gameObject.AddComponent<EventStoryProvider>();
        }
        
        // 订阅事件
        if (timeManager != null)
        {
            timeManager.onDayChanged += OnDayChanged;
        }
        
        Debug.Log("[EventManager] 系统初始化完成");
    }

    void OnDestroy()
    {
        if (timeManager != null)
        {
            timeManager.onDayChanged -= OnDayChanged;
        }
    }

    /// <summary>
    /// 每天的回调 - 每天重置事件计数
    /// </summary>
    private void OnDayChanged()
    {
        eventsTriggeredToday = 0;
        
        // 尝试触发每日随机事件
        TryTriggerDailyEvent();
    }

    /// <summary>
    /// 尝试触发每日随机事件
    /// 概率 10-20%，每天最多一次
    /// </summary>
    private void TryTriggerDailyEvent()
    {
        if (eventsTriggeredToday > 0) return;  // 每天只一次
        
        // ✅ 修复：使用 UnityEngine.Random 而不是 System.Random
        if (UnityEngine.Random.Range(0f, 1f) > 0.15f) return;  // 15% 触发概率
        
        // 选择一个随机事件
        var randomEvent = SelectRandomEvent();
        
        if (randomEvent != null && CheckAllConditions(randomEvent))
        {
            TriggerEvent(randomEvent, "daily_random");
        }
    }

    /// <summary>
    /// 从可用事件中选择一个随机事件
    /// 考虑权重和触发条件
    /// </summary>
    private EventData SelectRandomEvent()
    {
        if (eventDatabase == null || eventDatabase.events.Length == 0)
        {
            Debug.LogWarning("[EventManager] 事件数据库为空！");
            return null;
        }
        
        // 筛选满足条件的事件
        var validEvents = eventDatabase.events
            .Where(e => 
                e.category == EventCategory.Random &&
                CheckAllConditions(e) &&
                CheckFlagRequirements(e)
            )
            .ToList();
        
        if (validEvents.Count == 0)
        {
            if (debugMode)
                Debug.Log("[EventManager] 没有满足条件的随机事件");
            return null;
        }
        
        // 基于权重随机选择
        float totalWeight = validEvents.Sum(e => e.eventWeight);
        // ✅ 修复：使用 UnityEngine.Random
        float randomValue = UnityEngine.Random.Range(0f, totalWeight);
        
        float accumulatedWeight = 0f;
        foreach (var eventData in validEvents)
        {
            accumulatedWeight += eventData.eventWeight;
            if (randomValue <= accumulatedWeight)
            {
                return eventData;
            }
        }
        
        return validEvents.LastOrDefault();
    }

    /// <summary>
    /// 检查所有条件是否满足（AND 逻辑）
    /// </summary>
    private bool CheckAllConditions(EventData eventData)
    {
        if (eventData.conditions == null || eventData.conditions.Length == 0)
            return true;
        
        foreach (var condition in eventData.conditions)
        {
            if (!condition.IsSatisfied(gameState, timeManager, flagManager))
                return false;
        }
        
        return true;
    }

    /// <summary>
    /// 检查标志要求
    /// </summary>
    private bool CheckFlagRequirements(EventData eventData)
    {
        // 检查必需标志（都要有）
        if (eventData.requiredFlags != null && eventData.requiredFlags.Length > 0)
        {
            if (!flagManager.HasAllFlags(eventData.requiredFlags))
                return false;
        }
        
        // 检查互斥标志（都不能有）
        if (eventData.excludedFlags != null && eventData.excludedFlags.Length > 0)
        {
            if (flagManager.HasAnyFlag(eventData.excludedFlags))
                return false;
        }
        
        return true;
    }

    /// <summary>
    /// 直接触发指定事件
    /// 用于测试或特定剧情节点
    /// </summary>
    public bool TriggerEvent(string eventId, string reason = "manual")
    {
        var eventData = eventDatabase.GetEventById(eventId);
        if (eventData == null)
        {
            Debug.LogError($"[EventManager] 事件不存在: {eventId}");
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
            Debug.LogWarning("[EventManager] 已有事件在进行中，无法触发新事件");
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
        
        // 显示事件 UI
        if (eventUIPanel != null)
        {
            eventUIPanel.ShowEvent(eventData);
        }
        
        if (debugMode)
        {
            Debug.Log($"[EventManager] ✓ 事件已触发: {eventData.eventName} ({eventData.eventId})");
            Debug.Log($"  原因: {reason}");
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
            Debug.LogWarning("[EventManager] 没有活跃事件");
            return;
        }
        
        if (choiceIndex < 0 || choiceIndex >= currentEvent.choices.Length)
        {
            Debug.LogError($"[EventManager] 无效的选择索引: {choiceIndex}");
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
            Debug.Log($"[EventManager] 玩家选择: {choice.GetChoiceText()}");
        }
        
        // 检查后续事件
        if (!string.IsNullOrEmpty(choice.nextEventId))
        {
            // 延迟触发后续事件
            Invoke(nameof(TriggerNextEvent), 1f);
            nextEventId = choice.nextEventId;
        }
        else
        {
            EndCurrentEvent();
        }
    }

    // 用于存储后续事件 ID 的临时变量
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
    /// 应用选择的所有效果
    /// </summary>
    private void ApplyChoiceEffects(EventChoice choice)
    {
        if (choice.effects == null || choice.effects.Length == 0)
            return;
        
        if (gameState == null)
        {
            Debug.LogError("[EventManager] GameState 为空！");
            return;
        }
        
        gameState.ApplyEffect(new List<string>(choice.effects));
        
        if (debugMode)
        {
            Debug.Log($"[EventManager] 效果已应用: {string.Join(", ", choice.effects)}");
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
            Debug.Log("[EventManager] 事件已结束");
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
    /// 获取事件统计信息
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
            Debug.LogError("[EventManager] 事件数据库未加载");
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

    [ContextMenu("DEBUG: 触发随机事件")]
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
}