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

