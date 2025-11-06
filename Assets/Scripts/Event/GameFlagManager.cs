using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 游戏标志管理器
/// 追踪玩家的历史选择、已触发的里程碑等
/// 
/// 标志分类：
/// 1. 行为标志：worked_at_bar, bought_bicycle, dated_someone
/// 2. 里程碑标志：first_week_completed, rich_status, poor_status
/// 3. 事件标志：event_001_choice_a_picked, medical_emergency_occurred
/// 4. 状态标志：married_status, injured_status
/// </summary>
public class GameFlagManager : MonoBehaviour
{
    [Header("调试模式")]
    [SerializeField] private bool debugMode = true;
    
    // 标志存储
    private HashSet<string> flags = new();
    
    // 预设标志（编辑器中配置）
    [SerializeField] private string[] initialFlags = new string[0];  // ✅ 修复：明确初始化
    
    // 单例
    public static GameFlagManager Instance { get; private set; }
    
    // 事件
    public event Action<string> OnFlagSet;
    public event Action<string> OnFlagCleared;

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
        InitializeFlags();
        Debug.Log("[GameFlagManager] 初始化完成");
    }

    /// <summary>
    /// 初始化标志
    /// </summary>
    private void InitializeFlags()
    {
        flags.Clear();
        
        if (initialFlags != null)  // ✅ 修复：添加空值检查
        {
            foreach (var flag in initialFlags)
            {
                if (!string.IsNullOrEmpty(flag))
                {
                    flags.Add(flag);
                }
            }
        }
        
        if (debugMode)
        {
            Debug.Log($"[GameFlagManager] 已加载 {flags.Count} 个初始标志");
        }
    }

    /// <summary>
    /// 设置标志
    /// </summary>
    public void SetFlag(string flagName)
    {
        if (string.IsNullOrEmpty(flagName)) return;
        
        bool isNew = flags.Add(flagName);
        
        if (isNew)
        {
            OnFlagSet?.Invoke(flagName);
            
            if (debugMode)
            {
                Debug.Log($"[GameFlagManager] ✓ 设置标志: {flagName}");
            }
        }
    }

    /// <summary>
    /// 清除标志
    /// </summary>
    public void ClearFlag(string flagName)
    {
        if (string.IsNullOrEmpty(flagName)) return;
        
        bool removed = flags.Remove(flagName);
        
        if (removed)
        {
            OnFlagCleared?.Invoke(flagName);
            
            if (debugMode)
            {
                Debug.Log($"[GameFlagManager] ✗ 清除标志: {flagName}");
            }
        }
    }

    /// <summary>
    /// 检查是否有标志
    /// </summary>
    public bool HasFlag(string flagName)
    {
        if (string.IsNullOrEmpty(flagName)) return false;
        return flags.Contains(flagName);
    }

    /// <summary>
    /// 检查是否有所有标志
    /// </summary>
    public bool HasAllFlags(params string[] flagNames)
    {
        if (flagNames == null) return true;  // ✅ 修复：null 检查
        
        foreach (var flag in flagNames)
        {
            if (!HasFlag(flag)) return false;
        }
        return true;
    }

    /// <summary>
    /// 检查是否有任意标志
    /// </summary>
    public bool HasAnyFlag(params string[] flagNames)
    {
        if (flagNames == null) return false;  // ✅ 修复：null 检查
        
        foreach (var flag in flagNames)
        {
            if (HasFlag(flag)) return true;
        }
        return false;
    }

    /// <summary>
    /// 检查是否有互斥标志（都没有）
    /// </summary>
    public bool HasNoneOfFlags(params string[] flagNames)
    {
        if (flagNames == null) return true;  // ✅ 修复：null 检查
        
        foreach (var flag in flagNames)
        {
            if (HasFlag(flag)) return false;
        }
        return true;
    }

    /// <summary>
    /// 获取所有标志
    /// </summary>
    public IEnumerable<string> GetAllFlags()
    {
        return flags;
    }

    /// <summary>
    /// 获取标志数量
    /// </summary>
    public int GetFlagCount()
    {
        return flags.Count;
    }

    /// <summary>
    /// 清除所有标志
    /// </summary>
    public void ClearAllFlags()
    {
        flags.Clear();
        
        if (debugMode)
        {
            Debug.Log("[GameFlagManager] 所有标志已清除");
        }
    }

    /// <summary>
    /// 重置为初始状态
    /// </summary>
    public void ResetToInitial()
    {
        InitializeFlags();
        
        if (debugMode)
        {
            Debug.Log("[GameFlagManager] 已重置为初始状态");
        }
    }

    /// <summary>
    /// 预设标志常量（便于使用）
    /// </summary>
    public static class FlagNames
    {
        // ===== 行为标志 =====
        public const string WORKED_AT_BAR = "worked_at_bar";
        public const string WORKED_AT_RESTAURANT = "worked_at_restaurant";
        public const string WORKED_AS_DELIVERY = "worked_as_delivery";
        public const string WORKED_AS_LEAFLET = "worked_as_leaflet";
        
        // ===== 购买标志 =====
        public const string BOUGHT_BICYCLE = "bought_bicycle";
        public const string BOUGHT_VITAMINS = "bought_vitamins";
        
        // ===== 里程碑标志 =====
        public const string FIRST_WEEK_COMPLETED = "first_week_completed";
        public const string FIRST_MONTH_COMPLETED = "first_month_completed";
        public const string RICH_STATUS = "rich_status";          // 金币 > 5000
        public const string POOR_STATUS = "poor_status";          // 金币 < 100
        public const string HEALTHY_STATUS = "healthy_status";    // 健康 > 80
        public const string ILLNESS_STATUS = "illness_status";    // 健康 < 30
        
        // ===== 事件标志 =====
        public const string OVERWORK_EVENT_OCCURRED = "overwork_event_occurred";
        public const string DEPRESSION_EVENT_OCCURRED = "depression_event_occurred";
        public const string WEALTH_EVENT_OCCURRED = "wealth_event_occurred";
        
        // ===== 选择标志 =====
        public const string CHOSE_REST_FOR_OVERWORK = "chose_rest_for_overwork";
        public const string CHOSE_CONTINUE_FOR_OVERWORK = "chose_continue_for_overwork";
        
        // ===== 状态标志 =====
        public const string IN_CRISIS = "in_crisis";              // 处于危机状态
        public const string RECOVERING = "recovering";            // 正在恢复
    }

    // ===== 调试方法 =====

    /// <summary>
    /// 快速设置多个标志
    /// </summary>
    public void SetFlags(params string[] flagNames)
    {
        if (flagNames == null) return;  // ✅ 修复：null 检查
        
        foreach (var flag in flagNames)
        {
            SetFlag(flag);
        }
    }

    /// <summary>
    /// 快速清除多个标志
    /// </summary>
    public void ClearFlags(params string[] flagNames)
    {
        if (flagNames == null) return;  // ✅ 修复：null 检查
        
        foreach (var flag in flagNames)
        {
            ClearFlag(flag);
        }
    }

    /// <summary>
    /// 打印所有标志
    /// </summary>
    [ContextMenu("DEBUG: 打印所有标志")]
    public void DebugPrintAllFlags()
    {
        Debug.Log($"\n========== 所有标志 (共 {flags.Count} 个) ==========");
        int index = 1;
        foreach (var flag in flags)
        {
            Debug.Log($"{index}. {flag}");
            index++;
        }
        Debug.Log("==========================================\n");
    }

    /// <summary>
    /// 模拟设置标志
    /// </summary>
    [ContextMenu("DEBUG: 设置测试标志")]
    public void DebugSetTestFlags()
    {
        SetFlags(
            FlagNames.WORKED_AT_BAR,
            FlagNames.BOUGHT_BICYCLE,
            FlagNames.FIRST_WEEK_COMPLETED,
            FlagNames.WEALTH_EVENT_OCCURRED
        );
        DebugPrintAllFlags();
    }

    /// <summary>
    /// 清除所有调试标志
    /// </summary>
    [ContextMenu("DEBUG: 清除所有标志")]
    public void DebugClearAll()
    {
        ClearAllFlags();
        DebugPrintAllFlags();
    }

    /// <summary>
    /// 导出标志列表（用于存档）
    /// </summary>
    public string[] ExportFlags()
    {
        return new List<string>(flags).ToArray();
    }

    /// <summary>
    /// 导入标志列表（用于读档）
    /// </summary>
    public void ImportFlags(string[] flagArray)
    {
        flags.Clear();
        
        if (flagArray != null)  // ✅ 修复：null 检查
        {
            foreach (var flag in flagArray)
            {
                if (!string.IsNullOrEmpty(flag))
                {
                    flags.Add(flag);
                }
            }
        }
        
        if (debugMode)
        {
            Debug.Log($"[GameFlagManager] 已导入 {flags.Count} 个标志");
        }
    }
}