// Assets/Scripts/Consume/ConsumeSystem.cs

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 消费系统 - 改进版
/// ✨ 新增：支持"睡到第二天"功能
/// </summary>
public class ConsumeSystem : MonoBehaviour
{
    [Header("系统参考")]
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private DailyFlowManager dailyFlowManager;  // ✨ 新增：用于触发进入下一天
    
    private AffectGameState gameState;

    void Awake()
    {
        if (gameState == null)
            gameState = FindObjectOfType<AffectGameState>();
        if (dailyFlowManager == null)
            dailyFlowManager = FindObjectOfType<DailyFlowManager>();
    }

    void Start()
    {
        if (timeManager == null)
            timeManager = GetComponent<TimeManager>();
        
        Debug.Log("[ConsumeSystem] 系统已初始化");
        
        if (false)
        {
            ConsumableItemDatabase.DebugPrintAllItems();
        }
    }

    /// <summary>
    /// 使用/购买物品（主要调用接口）
    /// ✨ 改进：支持"睡到第二天"特殊处理
    /// </summary>
    public void UseItem(string itemId)
    {
        var item = ConsumableItemDatabase.GetItemById(itemId);
        if (item == null)
        {
            Debug.LogError($"[ConsumeSystem] 物品不存在: {itemId}");
            return;
        }

        string itemName = GetItemName(item);

        // 1) 检查金币
        if (gameState != null && gameState.res.gold < item.cost)
        {
            Debug.LogWarning($"[ConsumeSystem] {itemName} - 金币不足！需要 {item.cost}，你有 {gameState.res.gold}");
            if (gameState != null)
            {
                gameState.ApplyEffect(new List<string> { "V-1" });
            }
            return;
        }

        // ✨ 特殊处理：睡到第二天
        if (item.isSleepToNextDay)
        {
            UseSleepToNextDay(item, itemName);
            return;
        }

        // 2) 普通物品：请求消耗时间
        var timeRequest = new TimeConsumeRequest(item.timeRequired, $"消费: {itemName}");
        var timeResult = timeManager.RequestTimeConsume(timeRequest);

        if (!timeResult.success)
        {
            Debug.LogWarning($"[ConsumeSystem] {timeResult}");
            return;
        }

        // 3) 扣除金币
        if (gameState != null)
        {
            gameState.res.gold -= item.cost;
        }

        // 4) 应用所有效果
        ApplyItemEffects(item, itemName);

        // 打印成功日志
        PrintSuccessLog(item, itemName, timeResult.remainingHours);
    }

    /// <summary>
    /// ✨ 新增：处理"睡到第二天"的特殊逻辑
    /// </summary>
    private void UseSleepToNextDay(ConsumableItem item, string itemName)
    {
        Debug.Log($"\n[ConsumeSystem] ========== 睡到第二天 ==========");
        
        // 1) 计算剩余时间
        float remainingHours = timeManager.GetRemainTime();
        Debug.Log($"[ConsumeSystem] 当前剩余时间: {remainingHours:F1} 小时");
        
        // 2) 根据剩余时间计算睡眠效果加成
        // 剩余时间越多，恢复效果越好
        float sleepQualityBonus = Mathf.Clamp(remainingHours / 8f, 0.5f, 1.5f);  // 0.5x ~ 1.5x
        
        float finalHealthGain = item.healthGain * sleepQualityBonus;
        float finalVChange = item.vChange * sleepQualityBonus;
        float finalAChange = item.aChange * sleepQualityBonus;
        
        Debug.Log($"[ConsumeSystem] 睡眠质量加成: {sleepQualityBonus:F2}x");
        Debug.Log($"[ConsumeSystem] 最终恢复效果:");
        Debug.Log($"  • 健康: +{finalHealthGain:F1} (基础 {item.healthGain})");
        Debug.Log($"  • 情绪V: {finalVChange:+F1} (基础 {item.vChange:+0;-0;0})");
        Debug.Log($"  • 情绪A: {finalAChange:+F1} (基础 {item.aChange:+0;-0;0})");
        
        // 3) 扣除金币（虽然睡觉免费，但保留这个逻辑以防以后有付费住宿）
        if (gameState != null && item.cost > 0)
        {
            gameState.res.gold -= item.cost;
        }
        
        // 4) 应用睡眠效果
        if (gameState != null)
        {
            var effects = new List<string>
            {
                $"V{(finalVChange > 0 ? "+" : "")}{finalVChange:F1}",
                $"A{(finalAChange > 0 ? "+" : "")}{finalAChange:F1}"
            };

            if (finalHealthGain > 0)
                effects.Add($"health+{finalHealthGain:F0}");
            else if (finalHealthGain < 0)
                effects.Add($"health{finalHealthGain:F0}");

            gameState.ApplyEffect(effects);
        }
        
        Debug.Log($"\n[ConsumeSystem] ✓ {itemName} 效果已应用");
        Debug.Log($"[ConsumeSystem] 准备进入下一天...\n");
        
        // 5) ✨ 关键：触发进入下一天
        if (dailyFlowManager != null)
        {
            // 延迟0.5秒执行，让UI有时间更新
            Invoke(nameof(TriggerNextDay), 0.5f);
        }
        else if (timeManager != null)
        {
            // 备用方案：直接调用TimeManager
            Debug.LogWarning("[ConsumeSystem] DailyFlowManager未找到，使用备用方案");
            Invoke(nameof(TriggerNextDayFallback), 0.5f);
        }
        else
        {
            Debug.LogError("[ConsumeSystem] 无法进入下一天：TimeManager和DailyFlowManager都未找到！");
        }
        
        Debug.Log($"========================================\n");
    }

    /// <summary>
    /// 触发进入下一天（通过DailyFlowManager）
    /// </summary>
    private void TriggerNextDay()
    {
        if (dailyFlowManager != null)
        {
            dailyFlowManager.SkipToDayEnd();
        }
    }

    /// <summary>
    /// 备用方案：直接通过TimeManager进入下一天
    /// </summary>
    private void TriggerNextDayFallback()
    {
        if (timeManager != null)
        {
            // 强制消耗剩余时间
            float remainTime = timeManager.GetRemainTime();
            if (remainTime > 0)
            {
                timeManager.TryConsumeTime(remainTime, "睡眠（剩余时间）");
            }
            
            // 进入下一天
            timeManager.AdvanceToNextDay();
        }
    }

    /// <summary>
    /// 应用物品效果（提取为独立方法）
    /// </summary>
    private void ApplyItemEffects(ConsumableItem item, string itemName)
    {
        if (gameState != null)
        {
            var effects = new List<string>
            {
                $"V{(item.vChange > 0 ? "+" : "")}{item.vChange}",
                $"A{(item.aChange > 0 ? "+" : "")}{item.aChange}"
            };

            if (item.healthGain > 0)
                effects.Add($"health+{item.healthGain:F0}");
            else if (item.healthGain < 0)
                effects.Add($"health{item.healthGain}");

            if (item.hungerRestore > 0)
            effects.Add($"hunger+{item.hungerRestore:F0}");

            gameState.ApplyEffect(effects);
        }
    }

    /// <summary>
    /// 打印成功日志（提取为独立方法）
    /// </summary>
    private void PrintSuccessLog(ConsumableItem item, string itemName, float remainingHours)
    {
        Debug.Log($"[ConsumeSystem] ✓ {itemName} 成功使用");
        Debug.Log($"  • 花费金币: {item.cost}");
        Debug.Log($"  • 消耗时间: {item.timeRequired} 小时");
        Debug.Log($"  • 健康变化: {item.healthGain:+0.0;-0.0;0}");
        Debug.Log($"  • 情绪变化: V{item.vChange:+0.0;-0.0;0}, A{item.aChange:+0.0;-0.0;0}");
        if (gameState != null)
        {
            Debug.Log($"  • 剩余金币: {gameState.res.gold:F0}");
        }
        Debug.Log($"  • 剩余时间: {remainingHours:F1} 小时\n");
    }

    private string GetItemName(ConsumableItem item)
    {
        if (LocalizationManager.Instance != null)
        {
            return LocalizationManager.Instance.GetString(item.itemNameKey);
        }
        return item.itemNameKey;
    }

    private string GetItemDescription(ConsumableItem item)
    {
        if (LocalizationManager.Instance != null)
        {
            return LocalizationManager.Instance.GetString(item.descriptionKey);
        }
        return item.descriptionKey;
    }

    public List<ConsumableItem> GetItemsByCategory(string category)
    {
        return ConsumableItemDatabase.GetItemsByCategory(category);
    }

    public List<ConsumableItem> GetAllItems()
    {
        return ConsumableItemDatabase.GetAllItems();
    }

    public bool CanUseItem(string itemId)
    {
        var item = ConsumableItemDatabase.GetItemById(itemId);
        if (item == null) return false;

        if (gameState != null && gameState.res.gold < item.cost) 
            return false;

        // ✨ 睡到第二天的物品总是可用（不需要检查时间）
        if (item.isSleepToNextDay)
            return true;

        if (!timeManager.HasEnoughTime(item.timeRequired)) 
            return false;

        return true;
    }

    public string GetItemInfo(string itemId)
    {
        var item = ConsumableItemDatabase.GetItemById(itemId);
        if (item == null) return "物品不存在";

        string itemName = GetItemName(item);
        string description = GetItemDescription(item);

        return $"{itemName}\n" +
               $"分类: {item.category}\n" +
               $"费用: {item.cost} 金币\n" +
               $"时间: {(item.isSleepToNextDay ? "睡到第二天" : $"{item.timeRequired} 小时")}\n" +
               $"健康: {item.healthGain:+0.0;-0.0;0}\n" +
               $"情绪: V{item.vChange:+0.0;-0.0;0}, A{item.aChange:+0.0;-0.0;0}\n" +
               $"说明: {description}";
    }

    [ContextMenu("DEBUG: 打印所有物品")]
    public void DebugPrintAllItems()
    {
        ConsumableItemDatabase.DebugPrintAllItems();
    }

    [ContextMenu("DEBUG: 打印食物分类")]
    public void DebugPrintFoodCategory()
    {
        ConsumableItemDatabase.DebugPrintCategory("food");
    }

    [ContextMenu("DEBUG: 打印休息分类")]
    public void DebugPrintRestCategory()
    {
        ConsumableItemDatabase.DebugPrintCategory("rest");
    }

    [ContextMenu("DEBUG: 打印娱乐分类")]
    public void DebugPrintEntertainmentCategory()
    {
        ConsumableItemDatabase.DebugPrintCategory("entertainment");
    }

    [ContextMenu("DEBUG: 打印工具分类")]
    public void DebugPrintToolCategory()
    {
        ConsumableItemDatabase.DebugPrintCategory("tool");
    }

    [ContextMenu("DEBUG: 测试睡到第二天")]
    public void DebugTestSleepToNextDay()
    {
        Debug.Log("\n[DEBUG] 开始测试睡到第二天功能...");
        UseItem("rest_sleep");
    }
}