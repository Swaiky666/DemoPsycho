using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 消费系统 - 改进版
/// 使用 ConsumableItemDatabase 管理物品数据
/// 演示如何与 TimeManager 和 AffectGameState 交互
/// </summary>
public class ConsumeSystem : MonoBehaviour
{
    [Header("系统参考")]
    [SerializeField] private TimeManager timeManager;
    
    // 直接引用 AffectGameState
    private AffectGameState gameState;

    void Awake()
    {
        if (gameState == null)
            gameState = FindObjectOfType<AffectGameState>();
    }

    void Start()
    {
        if (timeManager == null)
            timeManager = GetComponent<TimeManager>();
        
        Debug.Log("[ConsumeSystem] 系统已初始化");
        
        // 调试：打印所有物品信息
        if (false)  // 设为 true 以启用调试打印
        {
            ConsumableItemDatabase.DebugPrintAllItems();
        }
    }

    /// <summary>
    /// 使用/购买物品（主要调用接口）
    /// </summary>
    public void UseItem(string itemId)
    {
        var item = ConsumableItemDatabase.GetItemById(itemId);
        if (item == null)
        {
            Debug.LogError($"[ConsumeSystem] 物品不存在: {itemId}");
            return;
        }

        // 使用本地化获取物品名称
        string itemName = GetItemName(item);

        // 1) 检查金币
        if (gameState != null && gameState.res.gold < item.cost)
        {
            Debug.LogWarning($"[ConsumeSystem] {itemName} - 金币不足！需要 {item.cost}，你有 {gameState.res.gold}");
            if (gameState != null)
            {
                gameState.ApplyEffect(new List<string> { "V-1" });  // 失望情绪
            }
            return;
        }

        // 2) 请求消耗时间
        var timeRequest = new TimeConsumeRequest(item.timeRequired, $"消费: {itemName}");
        var timeResult = timeManager.RequestTimeConsume(timeRequest);

        if (!timeResult.success)
        {
            Debug.LogWarning($"[ConsumeSystem] {timeResult}");
            return;  // 时间不足，消费失败
        }

        // 3) 扣除金币
        if (gameState != null)
        {
            gameState.res.gold -= item.cost;
        }

        // 4) 应用所有效果
        if (gameState != null)
        {
            var effects = new List<string>
            {
                $"V{(item.vChange > 0 ? "+" : "")}{item.vChange}",   // 情绪变化
                $"A{(item.aChange > 0 ? "+" : "")}{item.aChange}"
            };

            // 只有在 healthGain != 0 时才加入
            if (item.healthGain > 0)
                effects.Add($"health+{item.healthGain:F0}");
            else if (item.healthGain < 0)
                effects.Add($"health{item.healthGain}");

            gameState.ApplyEffect(effects);
        }

        // 打印成功日志
        Debug.Log($"[ConsumeSystem] ✓ {itemName} 成功使用");
        Debug.Log($"  • 花费金币: {item.cost}");
        Debug.Log($"  • 消耗时间: {item.timeRequired} 小时");
        Debug.Log($"  • 健康变化: {item.healthGain:+0.0;-0.0;0}");
        Debug.Log($"  • 情绪变化: V{item.vChange:+0.0;-0.0;0}, A{item.aChange:+0.0;-0.0;0}");
        if (gameState != null)
        {
            Debug.Log($"  • 剩余金币: {gameState.res.gold:F0}");
        }
        Debug.Log($"  • 剩余时间: {timeResult.remainingHours:F1} 小时\n");
    }

    /// <summary>
    /// 获取物品本地化名称
    /// </summary>
    private string GetItemName(ConsumableItem item)
    {
        if (LocalizationManager.Instance != null)
        {
            return LocalizationManager.Instance.GetString(item.itemNameKey);
        }
        return item.itemNameKey;
    }

    /// <summary>
    /// 获取物品本地化描述
    /// </summary>
    private string GetItemDescription(ConsumableItem item)
    {
        if (LocalizationManager.Instance != null)
        {
            return LocalizationManager.Instance.GetString(item.descriptionKey);
        }
        return item.descriptionKey;
    }

    /// <summary>
    /// 获取指定分类的物品列表
    /// </summary>
    public List<ConsumableItem> GetItemsByCategory(string category)
    {
        return ConsumableItemDatabase.GetItemsByCategory(category);
    }

    /// <summary>
    /// 获取所有物品（调试用）
    /// </summary>
    public List<ConsumableItem> GetAllItems()
    {
        return ConsumableItemDatabase.GetAllItems();
    }

    /// <summary>
    /// 检查玩家是否能使用某物品（不会改变状态，仅检查）
    /// </summary>
    public bool CanUseItem(string itemId)
    {
        var item = ConsumableItemDatabase.GetItemById(itemId);
        if (item == null) return false;

        // 检查金币
        if (gameState != null && gameState.res.gold < item.cost) 
            return false;

        // 检查时间
        if (!timeManager.HasEnoughTime(item.timeRequired)) 
            return false;

        return true;
    }

    /// <summary>
    /// 获取物品详情
    /// </summary>
    public string GetItemInfo(string itemId)
    {
        var item = ConsumableItemDatabase.GetItemById(itemId);
        if (item == null) return "物品不存在";

        string itemName = GetItemName(item);
        string description = GetItemDescription(item);

        return $"{itemName}\n" +
               $"分类: {item.category}\n" +
               $"费用: {item.cost} 金币\n" +
               $"时间: {item.timeRequired} 小时\n" +
               $"健康: {item.healthGain:+0.0;-0.0;0}\n" +
               $"情绪: V{item.vChange:+0.0;-0.0;0}, A{item.aChange:+0.0;-0.0;0}\n" +
               $"说明: {description}";
    }

    /// <summary>
    /// 调试：打印所有物品
    /// </summary>
    [ContextMenu("DEBUG: 打印所有物品")]
    public void DebugPrintAllItems()
    {
        ConsumableItemDatabase.DebugPrintAllItems();
    }

    /// <summary>
    /// 调试：打印特定分类
    /// </summary>
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
}