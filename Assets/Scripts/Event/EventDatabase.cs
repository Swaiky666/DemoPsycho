using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 事件数据库
/// 集中管理所有事件数据
/// 可以从 JSON、ScriptableObject 或代码中加载
/// </summary>
[CreateAssetMenu(menuName = "Events/Event Database")]
public class EventDatabase : ScriptableObject
{
    [SerializeField] public EventData[] events = new EventData[0];
    
    private Dictionary<string, EventData> eventDict;
    
    public void Initialize()
    {
        eventDict = new Dictionary<string, EventData>();
        
        foreach (var eventData in events)
        {
            if (!eventDict.ContainsKey(eventData.eventId))
            {
                eventDict.Add(eventData.eventId, eventData);
            }
            else
            {
                Debug.LogWarning($"[EventDatabase] 重复的事件 ID: {eventData.eventId}");
            }
        }
        
        Debug.Log($"[EventDatabase] 已加载 {events.Length} 个事件");
    }
    
    /// <summary>
    /// 根据 ID 获取事件
    /// </summary>
    public EventData GetEventById(string eventId)
    {
        if (eventDict == null)
            Initialize();
        
        eventDict.TryGetValue(eventId, out var eventData);
        return eventData;
    }
    
    /// <summary>
    /// 获取指定分类的所有事件
    /// </summary>
    public EventData[] GetEventsByCategory(EventCategory category)
    {
        return events.Where(e => e.category == category).ToArray();
    }
    
    /// <summary>
    /// 获取所有事件
    /// </summary>
    public EventData[] GetAllEvents()
    {
        return events;
    }
    
    /// <summary>
    /// 获取事件数量
    /// </summary>
    public int GetEventCount()
    {
        return events.Length;
    }
    
    /// <summary>
    /// 快速调试：打印所有事件摘要
    /// </summary>
    [ContextMenu("DEBUG: 打印事件列表")]
    public void DebugPrintEvents()
    {
        Debug.Log($"\n========== 事件数据库 ({events.Length} 个事件) ==========");
        
        var grouped = events.GroupBy(e => e.category);
        
        foreach (var group in grouped)
        {
            Debug.Log($"\n【{group.Key}】({group.Count()} 个)");
            foreach (var eventData in group)
            {
                Debug.Log($"  • {eventData.eventName} ({eventData.eventId})");
                Debug.Log($"    触发概率: {eventData.triggerProbability * 100}%, 选择数: {eventData.choices.Length}");
            }
        }
        
        Debug.Log($"\n===================================================\n");
    }
}

/// <summary>
/// 事件故事提供者
/// 负责获取或生成事件的文本内容
/// 支持预写库 + AI 生成的混合方案
/// </summary>
public class EventStoryProvider : MonoBehaviour
{
    [Header("预写故事库")]
    [SerializeField] private TextAsset prewrittenStoriesJson;
    
    [Header("AI 生成配置")]
    [SerializeField] private bool useAIGeneration = false;  // 是否使用 AI 生成
    
    private Dictionary<string, string> prewrittenStories = new();
    
    void Start()
    {
        LoadPrewrittenStories();
    }
    
    /// <summary>
    /// 加载预写故事库
    /// </summary>
    private void LoadPrewrittenStories()
    {
        prewrittenStories.Clear();
        
        if (prewrittenStoriesJson != null)
        {
            try
            {
                // 简化版本：直接从预设中加载
                // 实际项目中可以使用 JSON 解析
                InitializeDefaultStories();
                Debug.Log($"[EventStoryProvider] 已加载 {prewrittenStories.Count} 个预写故事");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[EventStoryProvider] 加载故事库出错: {ex.Message}");
            }
        }
        else
        {
            InitializeDefaultStories();
        }
    }
    
    /// <summary>
    /// 初始化默认故事（硬编码）
    /// 实际项目应该从 JSON 或数据库加载
    /// </summary>
    private void InitializeDefaultStories()
    {
        // ===== 过劳相关 =====
        prewrittenStories["story_overwork"] = "你感到浑身疲惫，眼睛酸涩，身体沉重地无法做任何事情。一个声音在告诉你，也许是时候休息一下了...";
        
        prewrittenStories["choice_overwork_rest"] = "好吧，今天就好好休息吧";
        prewrittenStories["result_overwork_rest"] = "你躺在床上，很快就陷入了深度睡眠。醒来时，感到精神焕发。";
        
        prewrittenStories["choice_overwork_continue"] = "咬牙坚持，再工作一下";
        prewrittenStories["result_overwork_continue"] = "你勉强撑过了这一段，但身体的抗议声越来越大...";
        
        // ===== 财富膨胀相关 =====
        prewrittenStories["story_wealth"] = "看着银行账户上的数字，你突然意识到自己存了不少钱。一种陌生的满足感涌上心头。";
        
        prewrittenStories["choice_wealth_save"] = "继续存钱，为未来计划";
        prewrittenStories["result_wealth_save"] = "你感到一种安全感。也许存够了钱，就能做自己想做的事...";
        
        prewrittenStories["choice_wealth_spend"] = "犒劳自己，好好消费一番";
        prewrittenStories["result_wealth_spend"] = "购物给了你暂时的快乐，虽然钱包瘪了一些，但心情确实变好了。";
        
        // ===== 抑郁相关 =====
        prewrittenStories["story_depression"] = "一种莫名的悲伤笼罩了你。天空似乎都变成了灰色，所有的事物都失去了颜色。";
        
        prewrittenStories["choice_depression_seek_help"] = "找朋友倾诉，寻求帮助";
        prewrittenStories["result_depression_seek_help"] = "倾诉后，你感到了一些释然。也许，你不用一个人承担所有的重担。";
        
        prewrittenStories["choice_depression_endure"] = "独自承受，相信时间会治愈";
        prewrittenStories["result_depression_endure"] = "黑暗似乎在逐渐褪去，但这个过程很漫长...";
        
        // ===== 工作机会相关 =====
        prewrittenStories["story_job_opportunity"] = "一个新的工作机会出现了。机会难得，但你不确定自己是否能胜任...";
        
        prewrittenStories["choice_take_chance"] = "冒险一试，挑战自己";
        prewrittenStories["result_take_chance"] = "虽然有些吃力，但你在挑战中学到了很多。";
        
        prewrittenStories["choice_play_safe"] = "选择安全，坚持原来的工作";
        prewrittenStories["result_play_safe"] = "至少不会失败，但你多少有些遗憾...";
        
        // ===== 健康危机相关 =====
        prewrittenStories["story_health_crisis"] = "突然的不适让你意识到，也许长期的透支终于开始报应了。";
        
        prewrittenStories["choice_see_doctor"] = "立即去看医生";
        prewrittenStories["result_see_doctor"] = "医生的诊断让你松了一口气——还不是太严重。但确实需要好好休养。";
        
        prewrittenStories["choice_ignore"] = "吃点药硬撑，继续工作";
        prewrittenStories["result_ignore"] = "你试图忽视身体的信号，但这似乎不是个明智的决定...";
    }
    
    /// <summary>
    /// 获取故事文本
    /// 优先从预写库获取，如果没有则考虑 AI 生成
    /// </summary>
    public string GetStory(EventData eventData)
    {
        if (eventData == null) return "";
        
        // 1. 尝试从预写库获取
        if (prewrittenStories.TryGetValue(eventData.storyKey, out var story))
        {
            return story;
        }
        
        // 2. 尝试 AI 生成
        if (useAIGeneration)
        {
            return GenerateStoryWithAI(eventData);
        }
        
        // 3. 降级处理
        return $"[{eventData.eventName}]\n事件故事文本缺失。";
    }
    
    /// <summary>
    /// AI 生成故事文本
    /// </summary>
    private string GenerateStoryWithAI(EventData eventData)
    {
        // TODO: 集成 LLM API
        // 这里是伪代码示例
        
        return $"[AI生成] {eventData.eventName}: 这是一个由AI生成的故事...";
    }
    
    /// <summary>
    /// 获取本地化的选择文本
    /// </summary>
    public string GetChoiceText(EventChoice choice)
    {
        if (prewrittenStories.TryGetValue(choice.choiceTextKey, out var text))
        {
            return text;
        }
        
        return choice.choiceTextKey;  // 降级：返回 key
    }
    
    /// <summary>
    /// 获取结果文本
    /// </summary>
    public string GetResultText(EventChoice choice)
    {
        if (string.IsNullOrEmpty(choice.resultTextKey)) return "";
        
        if (prewrittenStories.TryGetValue(choice.resultTextKey, out var text))
        {
            return text;
        }
        
        return "";
    }
    
    /// <summary>
    /// 添加预写故事（运行时）
    /// </summary>
    public void AddStory(string key, string text)
    {
        prewrittenStories[key] = text;
    }
    
    /// <summary>
    /// 获取所有故事 key
    /// </summary>
    public IEnumerable<string> GetAllStoryKeys()
    {
        return prewrittenStories.Keys;
    }
    
    // ===== 调试方法 =====
    
    [ContextMenu("DEBUG: 打印所有故事")]
    public void DebugPrintAllStories()
    {
        Debug.Log($"\n========== 预写故事库 ({prewrittenStories.Count} 个) ==========");
        
        int index = 1;
        foreach (var kvp in prewrittenStories)
        {
            Debug.Log($"{index}. {kvp.Key}");
            Debug.Log($"   {kvp.Value.Substring(0, System.Math.Min(50, kvp.Value.Length))}...");
            index++;
        }
        
        Debug.Log($"=================================================\n");
    }
    
    [ContextMenu("DEBUG: 重新加载故事库")]
    public void DebugReloadStories()
    {
        LoadPrewrittenStories();
        Debug.Log($"[DEBUG] 故事库已重新加载，共 {prewrittenStories.Count} 个故事");
    }
}