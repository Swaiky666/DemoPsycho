// Assets/Scripts/Consume/ConsumableItem.cs

using UnityEngine;
using System;

/// <summary>
/// 可消费物品数据结构
/// 定义一个消费物品的所有信息
/// </summary>
[System.Serializable]
public class ConsumableItem
{
    [Header("基础信息")]
    public string itemId = "food_001";
    public string itemNameKey = "item_food_convenience";
    public string category = "food";
    public string descriptionKey = "desc_food_convenience";

    [Header("消费成本")]
    public float cost = 15f;
    public float timeRequired = 0.5f;

    [Header("效果变化")]
    public float healthGain = 10f;
    public float vChange = 1f;
    public float aChange = -0.5f;
    public float hungerRestore = 0f;

    [Header("UI 展示（可选）")]
    public Sprite itemIcon;
    public Color itemColor = Color.white;

    [Header("特殊标记")]
    public bool isSleepToNextDay = false;  

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
        hungerRestore = 0f;
    }

    public string GetFormattedInfo()
    {
        return $@"
【{itemNameKey}】
分类：{category}
费用：¥{cost:F0}
耗时：{(isSleepToNextDay ? "睡到第二天" : $"{timeRequired}小时")}

属性变化：
  健康：{healthGain:+0.0;-0.0;0}
  情绪效价：{vChange:+0.0;-0.0;0}
  情绪唤醒：{aChange:+0.0;-0.0;0}

描述：{descriptionKey}
";
    }

    public bool CanPlayerUse(float playerGold, float remainTime)
    {
        if (playerGold < cost)
            return false;
        
        // ✨ 睡到第二天的物品不需要检查时间
        if (isSleepToNextDay)
            return true;
            
        if (remainTime < timeRequired)
            return false;
        return true;
    }

    public Color GetCategoryColor()
    {
        return category switch
        {
            "food" => new Color(1f, 0.8f, 0.4f),
            "rest" => new Color(0.5f, 1f, 0.5f),
            "entertainment" => new Color(1f, 0.5f, 1f),
            "tool" => new Color(0.5f, 0.8f, 1f),
            _ => Color.white
        };
    }

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

    public void DebugPrint()
    {
        Debug.Log($"\n========== ConsumableItem 信息 ==========");
        Debug.Log($"物品 ID: {itemId}");
        Debug.Log($"名称 Key: {itemNameKey}");
        Debug.Log($"分类: {category}");
        Debug.Log($"费用: {cost} 金币");
        Debug.Log($"时间: {(isSleepToNextDay ? "睡到第二天" : $"{timeRequired} 小时")}");
        Debug.Log($"健康变化: {healthGain:+0.0;-0.0;0}");
        Debug.Log($"情绪变化: V{vChange:+0.0;-0.0;0}, A{aChange:+0.0;-0.0;0}");
        Debug.Log($"描述 Key: {descriptionKey}");
        Debug.Log($"特殊标记: 睡到第二天={isSleepToNextDay}");
        Debug.Log($"========================================\n");
    }
}