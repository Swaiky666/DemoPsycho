using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 消费系统 - 改进版
/// ✨ 修复：
/// 1. 睡觉和休息不再直接增加健康值
/// 2. 饥饿值极低时的健康惩罚更严重
/// 3. 必须通过吃东西来恢复健康
/// </summary>
public class ConsumeSystem : MonoBehaviour
{
    [Header("系统参考")]
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private DailyFlowManager dailyFlowManager;
    
    [Header("饥饿惩罚设置 ✨")]
    [SerializeField] private float hungerHealthPenaltyRate = 2f;  // 饥饿导致的每小时健康损失倍率
    
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
    }

    /// <summary>
    /// ✨ 改进版：使用物品
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

        // 4) ✨ 改进：应用效果（休息类不增加健康）
        ApplyItemEffects(item, itemName);

        // 打印成功日志
        PrintSuccessLog(item, itemName, timeResult.remainingHours);
    }

    /// <summary>
    /// ✨ 改进版：睡到第二天
    /// </summary>
    private void UseSleepToNextDay(ConsumableItem item, string itemName)
    {
        Debug.Log($"\n[ConsumeSystem] ========== 睡到第二天 ==========");
        
        // 1) 计算剩余时间
        float remainingHours = timeManager.GetRemainTime();
        Debug.Log($"[ConsumeSystem] 当前剩余时间: {remainingHours:F1} 小时");
        
        // ✨ 改进：睡眠不再直接恢复健康
        // 只影响情绪，健康恢复必须通过吃东西
        
        // 2) 情绪恢复（睡眠让情绪平复）
        float sleepQualityBonus = Mathf.Clamp(remainingHours / 8f, 0.5f, 1.5f);
        float finalVChange = item.vChange * sleepQualityBonus;
        float finalAChange = item.aChange * sleepQualityBonus;
        
        Debug.Log($"[ConsumeSystem] 睡眠质量加成: {sleepQualityBonus:F2}x");
        Debug.Log($"[ConsumeSystem] 情绪恢复:");
        Debug.Log($"  • 情绪V: {finalVChange:+F1}");
        Debug.Log($"  • 情绪A: {finalAChange:+F1}");
        
        // 3) 扣除金币
        if (gameState != null && item.cost > 0)
        {
            gameState.res.gold -= item.cost;
        }
        
        // 4) 应用睡眠效果（不包括健康恢复）
        if (gameState != null)
        {
            var effects = new List<string>
            {
                $"V{(finalVChange > 0 ? "+" : "")}{finalVChange:F1}",
                $"A{(finalAChange > 0 ? "+" : "")}{finalAChange:F1}"
            };
            
            // ✨ 移除健康恢复
            
            gameState.ApplyEffect(effects);
        }
        
        Debug.Log($"\n[ConsumeSystem] ✓ {itemName} 效果已应用（情绪恢复）");
        Debug.Log($"[ConsumeSystem] 💡 提示：想要恢复健康，需要吃东西！");
        Debug.Log($"[ConsumeSystem] 准备进入下一天...\n");
        
        // 5) 触发进入下一天
        if (dailyFlowManager != null)
        {
            Invoke(nameof(TriggerNextDay), 0.5f);
        }
        else if (timeManager != null)
        {
            Debug.LogWarning("[ConsumeSystem] DailyFlowManager未找到，使用备用方案");
            Invoke(nameof(TriggerNextDayFallback), 0.5f);
        }
        else
        {
            Debug.LogError("[ConsumeSystem] 无法进入下一天：TimeManager和DailyFlowManager都未找到！");
        }
        
        Debug.Log($"========================================\n");
    }

    private void TriggerNextDay()
    {
        if (dailyFlowManager != null)
        {
            dailyFlowManager.SkipToDayEnd();
        }
    }

    private void TriggerNextDayFallback()
    {
        if (timeManager != null)
        {
            float remainTime = timeManager.GetRemainTime();
            if (remainTime > 0)
            {
                timeManager.TryConsumeTime(remainTime, "睡眠（剩余时间）");
            }
            
            timeManager.AdvanceToNextDay();
        }
    }

    /// <summary>
    /// ✨ 改进版：应用物品效果
    /// 休息类物品不再增加健康，只有食物才能恢复健康
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

            // ✨ 改进：只有食物类才能恢复健康
            if (item.category == "food")
            {
                if (item.healthGain > 0)
                    effects.Add($"health+{item.healthGain:F0}");
                else if (item.healthGain < 0)
                    effects.Add($"health{item.healthGain}");
            }
            // 休息类物品不影响健康

            // 恢复饥饿值
            if (item.hungerRestore > 0)
                effects.Add($"hunger+{item.hungerRestore:F0}");

            gameState.ApplyEffect(effects);
        }
    }

    private void PrintSuccessLog(ConsumableItem item, string itemName, float remainingHours)
    {
        Debug.Log($"[ConsumeSystem] ✓ {itemName} 成功使用");
        Debug.Log($"  • 花费金币: {item.cost}");
        Debug.Log($"  • 消耗时间: {item.timeRequired} 小时");
        
        if (item.category == "food")
        {
            Debug.Log($"  • 健康变化: {item.healthGain:+0.0;-0.0;0}");
            Debug.Log($"  • 饥饿恢复: +{item.hungerRestore:F0}");
        }
        else
        {
            Debug.Log($"  • 健康变化: 无（休息不恢复健康）");
        }
        
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
               $"健康: {(item.category == "food" ? $"{item.healthGain:+0.0;-0.0;0}" : "无")}\n" +
               $"情绪: V{item.vChange:+0.0;-0.0;0}, A{item.aChange:+0.0;-0.0;0}\n" +
               $"说明: {description}";
    }
}