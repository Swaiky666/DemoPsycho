using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 可消费物品数据
/// </summary>
[Serializable]
public class ConsumableItem
{
    public string itemId = "food_001";
    public string itemName = "便利店便当";
    public float cost = 15f;                  // 花费金币
    public float timeRequired = 0.5f;         // 消耗时间
    public float healthGain = 10f;            // 恢复健康
    public float vChange = 1f;                // 情绪变化 V
    public float aChange = -0.5f;             // 情绪变化 A
    public string category = "food";          // 分类：food/rest/entertainment/tool
    public string description = "简单填饱肚子";
}

/// <summary>
/// 消费系统
/// 演示如何与 TimeManager 和 AffectGameState 交互
/// </summary>
public class ConsumeSystem : MonoBehaviour
{
    [Header("参考")]
    [SerializeField] private TimeManager timeManager;
    
    // 直接引用你的 AffectGameState
    private AffectGameState gameState;

    void Awake()
    {
        if (gameState == null)
            gameState = FindObjectOfType<AffectGameState>();
    }

    [Header("消费数据")]
    private List<ConsumableItem> consumables = new();

    void Start()
    {
        if (timeManager == null)
            timeManager = GetComponent<TimeManager>();
        
        InitializeConsumables();
    }

    /// <summary>
    /// 初始化可消费物品列表
    /// </summary>
    private void InitializeConsumables()
    {
        consumables.Clear();

        // ===== 食物分类 =====
        consumables.Add(new ConsumableItem
        {
            itemId = "food_convenience",
            itemName = "便利店便当",
            cost = 15f,
            timeRequired = 0.5f,
            healthGain = 10f,
            vChange = 1f,
            aChange = -0.5f,
            category = "food",
            description = "简单填饱肚子"
        });

        consumables.Add(new ConsumableItem
        {
            itemId = "food_restaurant",
            itemName = "餐厅大餐",
            cost = 50f,
            timeRequired = 1.5f,
            healthGain = 30f,
            vChange = 2f,
            aChange = 0f,
            category = "food",
            description = "高质量的餐厅用餐体验"
        });

        consumables.Add(new ConsumableItem
        {
            itemId = "food_instant",
            itemName = "速冻面",
            cost = 5f,
            timeRequired = 0.25f,
            healthGain = 3f,
            vChange = -0.5f,
            aChange = 0f,
            category = "food",
            description = "快速充饥，但营养不足"
        });

        // ===== 休息分类 =====
        consumables.Add(new ConsumableItem
        {
            itemId = "rest_sleep",
            itemName = "充足睡眠",
            cost = 0f,
            timeRequired = 8f,           // 睡眠消耗大量时间
            healthGain = 50f,
            vChange = 1f,
            aChange = -2f,               // 显著降低唤醒度
            category = "rest",
            description = "恢复精神"
        });

        consumables.Add(new ConsumableItem
        {
            itemId = "rest_nap",
            itemName = "小憩",
            cost = 0f,
            timeRequired = 1f,
            healthGain = 10f,
            vChange = 0.5f,
            aChange = -1f,
            category = "rest",
            description = "快速恢复"
        });

        consumables.Add(new ConsumableItem
        {
            itemId = "rest_meditation",
            itemName = "冥想",
            cost = 0f,
            timeRequired = 0.5f,
            healthGain = 5f,
            vChange = 0.5f,
            aChange = -1.5f,
            category = "rest",
            description = "平静心绪"
        });

        // ===== 娱乐分类 =====
        consumables.Add(new ConsumableItem
        {
            itemId = "ent_movie",
            itemName = "看电影",
            cost = 30f,
            timeRequired = 2.5f,
            healthGain = -5f,            // 娱乐不直接增加健康，甚至可能减少（久坐）
            vChange = 2f,
            aChange = 0.5f,
            category = "entertainment",
            description = "娱乐身心"
        });

        consumables.Add(new ConsumableItem
        {
            itemId = "ent_coffee",
            itemName = "咖啡馆闲逛",
            cost = 20f,
            timeRequired = 1.5f,
            healthGain = 0f,
            vChange = 1.5f,
            aChange = -0.5f,
            category = "entertainment",
            description = "舒适的环境"
        });

        consumables.Add(new ConsumableItem
        {
            itemId = "ent_game",
            itemName = "玩游戏",
            cost = 0f,
            timeRequired = 2f,
            healthGain = -3f,
            vChange = 1.5f,
            aChange = 1f,
            category = "entertainment",
            description = "沉浸式娱乐"
        });

        // ===== 工具分类（购买一次，长期提升效率） =====
        consumables.Add(new ConsumableItem
        {
            itemId = "tool_bicycle",
            itemName = "购买自行车",
            cost = 200f,
            timeRequired = 0.5f,         // 购买时间
            healthGain = 0f,
            vChange = 1f,
            aChange = 0f,
            category = "tool",
            description = "提升送快递效率 -20% 时间"
        });

        consumables.Add(new ConsumableItem
        {
            itemId = "tool_vitamins",
            itemName = "购买维生素",
            cost = 80f,
            timeRequired = 0.25f,
            healthGain = 20f,
            vChange = 0.5f,
            aChange = 0f,
            category = "tool",
            description = "提升身体状态"
        });
    }

    /// <summary>
    /// 使用/购买物品（主要调用接口）
    /// </summary>
    public void UseItem(string itemId)
    {
        var item = consumables.Find(i => i.itemId == itemId);
        if (item == null)
        {
            Debug.LogError($"[ConsumeSystem] 物品不存在: {itemId}");
            return;
        }

        // 1) 检查金币
        if (gameState != null && gameState.res.gold < item.cost)
        {
            Debug.LogWarning($"[ConsumeSystem] ✗ {item.itemName} - 金币不足！需要 {item.cost}，你有 {gameState.res.gold}");
            if (gameState != null)
            {
                gameState.ApplyEffect(new List<string> { "V-1" });  // 失望情绪
            }
            return;
        }

        // 2) 请求消耗时间
        var timeRequest = new TimeConsumeRequest(item.timeRequired, $"消费: {item.itemName}");
        var timeResult = timeManager.RequestTimeConsume(timeRequest);

        if (!timeResult.success)
        {
            Debug.LogWarning($"[ConsumeSystem] ✗ {timeResult}");
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

            // 只有在 healthGain > 0 时才加入
            if (item.healthGain > 0)
                effects.Add($"health+{item.healthGain:F0}");
            else if (item.healthGain < 0)
                effects.Add($"health{item.healthGain}");

            gameState.ApplyEffect(effects);
        }

        // 打印成功日志
        Debug.Log($"[ConsumeSystem] ✓ {item.itemName} 成功使用！");
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
    /// 获取指定分类的物品列表
    /// </summary>
    public List<ConsumableItem> GetItemsByCategory(string category)
    {
        return consumables.FindAll(i => i.category == category);
    }

    /// <summary>
    /// 获取所有物品（调试用）
    /// </summary>
    public List<ConsumableItem> GetAllItems() => new(consumables);

    /// <summary>
    /// 检查玩家是否能使用某物品（不会改变状态，仅检查）
    /// </summary>
    public bool CanUseItem(string itemId)
    {
        var item = consumables.Find(i => i.itemId == itemId);
        if (item == null) return false;

        // 检查金币
        if (gameState != null && gameState.res.gold < item.cost) return false;

        // 检查时间
        if (!timeManager.HasEnoughTime(item.timeRequired)) return false;

        return true;
    }

    /// <summary>
    /// 获取物品详情
    /// </summary>
    public string GetItemInfo(string itemId)
    {
        var item = consumables.Find(i => i.itemId == itemId);
        if (item == null) return "物品不存在";

        return $"{item.itemName}\n" +
               $"  分类: {item.category}\n" +
               $"  费用: {item.cost} 金币\n" +
               $"  时间: {item.timeRequired} 小时\n" +
               $"  健康: {item.healthGain:+0.0;-0.0;0}\n" +
               $"  情绪: V{item.vChange:+0.0;-0.0;0}, A{item.aChange:+0.0;-0.0;0}\n" +
               $"  说明: {item.description}";
    }
}