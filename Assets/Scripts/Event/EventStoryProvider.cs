using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 事件故事提供者 - 本地化版本
/// 使用 LocalizationManager 提供中英文切换
/// </summary>
public class EventStoryProvider : MonoBehaviour
{
    [Header("调试模式")]
    [SerializeField] private bool debugMode = true;
    
    public static EventStoryProvider Instance { get; private set; }
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (debugMode)
        {
            Debug.Log($"[EventStoryProvider] 初始化完成，使用本地化系统");
        }
    }
    
    /// <summary>
    /// 获取故事文本（自动本地化）
    /// </summary>
    public string GetStory(EventData eventData)
    {
        if (eventData == null) return "";
        
        // 使用 LocalizationManager 获取本地化文本
        if (LocalizationManager.Instance != null)
        {
            return LocalizationManager.Instance.GetString(eventData.storyKey);
        }
        
        Debug.LogWarning($"[EventStoryProvider] LocalizationManager 未找到");
        return eventData.storyKey;
    }
    
    /// <summary>
    /// 获取选择文本（自动本地化）
    /// </summary>
    public string GetChoiceText(EventChoice choice)
    {
        if (choice == null) return "";
        
        if (LocalizationManager.Instance != null)
        {
            return LocalizationManager.Instance.GetString(choice.choiceTextKey);
        }
        
        return choice.choiceTextKey;
    }
    
    /// <summary>
    /// 获取结果文本（自动本地化）
    /// </summary>
    public string GetResultText(EventChoice choice)
    {
        if (choice == null || string.IsNullOrEmpty(choice.resultTextKey)) return "";
        
        if (LocalizationManager.Instance != null)
        {
            return LocalizationManager.Instance.GetString(choice.resultTextKey);
        }
        
        return "";
    }
}