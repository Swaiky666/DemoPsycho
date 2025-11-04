using UnityEngine;
using System;

/// <summary>
/// 可消费物品数据结构
/// 定义一个消费物品的所有信息
/// 
/// 分类：
/// - food: 食物（便利店、餐厅、速冻食品）
/// - rest: 休息（睡眠、小睡、冥想）
/// - entertainment: 娱乐（电影、咖啡、游戏）
/// - tool: 工具（自行车、维生素）
/// </summary>
[System.Serializable]
public class ConsumableItem
{
    [Header("基础信息")]
    public string itemId = "food_001";                           // 物品 ID（唯一标识）
    public string itemNameKey = "item_food_convenience";         // 物品名称本地化 key
    public string category = "food";                             // 分类：food/rest/entertainment/tool
    public string descriptionKey = "desc_food_convenience";      // 物品描述本地化 key

    [Header("消费成本")]
    public float cost = 15f;                                     // 花费金币
    public float timeRequired = 0.5f;                            // 消耗时间（小时）

    [Header("效果变化")]
    public float healthGain = 10f;                               // 健康变化（正数为恢复，负数为消耗）
    public float vChange = 1f;                                   // 情绪效价变化（V）
    public float aChange = -0.5f;                                // 情绪唤醒变化（A）

    [Header("UI 展示（可选）")]
    public Sprite itemIcon;                                      // 物品图标
    public Color itemColor = Color.white;                        // 物品卡牌颜色

    /// <summary>
    /// 创建新的消费物品数据
    /// </summary>
    public ConsumableItem(
        string id,
        string nameKey,
        string categoryType,
        float costGold,
        float timeHours,
        float health,
        float v,
        float a,
        string descKey = "")
    {
        itemId = id;
        itemNameKey = nameKey;
        category = categoryType;
        cost = costGold;
        timeRequired = timeHours;
        healthGain = health;
        vChange = v;
        aChange = a;
        descriptionKey = descKey;
    }

    /// <summary>
    /// 获取物品的格式化描述
    /// </summary>
    public string GetFormattedInfo()
    {
        return $@"
【{itemNameKey}】
分类：{category}
费用：¥{cost:F0}
耗时：{timeRequired}小时

属性变化：
  健康：{healthGain:+0.0;-0.0;0}
  情绪效价：{vChange:+0.0;-0.0;0}
  情绪唤醒：{aChange:+0.0;-0.0;0}

描述：{descriptionKey}
";
    }

    /// <summary>
    /// 检查玩家是否能使用该物品
    /// </summary>
    public bool CanPlayerUse(float playerGold, float remainTime)
    {
        if (playerGold < cost)
            return false;
        if (remainTime < timeRequired)
            return false;
        return true;
    }

    /// <summary>
    /// 获取分类的显示颜色
    /// </summary>
    public Color GetCategoryColor()
    {
        return category switch
        {
            "food" => new Color(1f, 0.8f, 0.4f),           // 黄色
            "rest" => new Color(0.5f, 1f, 0.5f),           // 绿色
            "entertainment" => new Color(1f, 0.5f, 1f),    // 紫色
            "tool" => new Color(0.5f, 0.8f, 1f),           // 蓝色
            _ => Color.white
        };
    }

    /// <summary>
    /// 获取分类的中文名称
    /// </summary>
    public string GetCategoryName()
    {
        return category switch
        {
            "food" => "食物",
            "rest" => "休息",
            "entertainment" => "娱乐",
            "tool" => "工具",
            _ => "未知"
        };
    }

    /// <summary>
    /// 调试：打印物品信息
    /// </summary>
    public void DebugPrint()
    {
        Debug.Log($"\n========== ConsumableItem 信息 ==========");
        Debug.Log($"物品 ID: {itemId}");
        Debug.Log($"名称 Key: {itemNameKey}");
        Debug.Log($"分类: {category}");
        Debug.Log($"费用: {cost} 金币");
        Debug.Log($"时间: {timeRequired} 小时");
        Debug.Log($"健康变化: {healthGain:+0.0;-0.0;0}");
        Debug.Log($"情绪变化: V{vChange:+0.0;-0.0;0}, A{aChange:+0.0;-0.0;0}");
        Debug.Log($"描述 Key: {descriptionKey}");
        Debug.Log($"========================================\n");
    }
}