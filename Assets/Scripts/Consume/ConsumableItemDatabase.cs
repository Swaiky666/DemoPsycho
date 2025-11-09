// Assets/Scripts/Consume/ConsumableItemDatabase.cs

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 消费物品数据配置
/// 集中管理所有消费物品的数据
/// 方便后续扩展和修改
/// </summary>
public static class ConsumableItemDatabase
{
    /// <summary>
    /// 获取所有消费物品列表
    /// </summary>
    public static List<ConsumableItem> GetAllItems()
    {
        var items = new List<ConsumableItem>();

        // ===== 食物分类 (Food) =====
        items.Add(GetFoodConvenience());
        items.Add(GetFoodRestaurant());
        items.Add(GetFoodInstant());

        // ===== 休息分类 (Rest) =====
        items.Add(GetRestSleep());  // ✨ 修改后的睡觉功能
        items.Add(GetRestNap());
        items.Add(GetRestMeditation());

        // ===== 娱乐分类 (Entertainment) =====
        items.Add(GetEntertainmentMovie());
        items.Add(GetEntertainmentCoffee());
        items.Add(GetEntertainmentGame());

        // ===== 工具分类 (Tool) =====
        items.Add(GetToolBicycle());
        items.Add(GetToolVitamins());

        return items;
    }

    /// <summary>
    /// 按分类获取物品列表
    /// </summary>
    public static List<ConsumableItem> GetItemsByCategory(string category)
    {
        var allItems = GetAllItems();
        var filtered = allItems.FindAll(item => item.category == category);
        return filtered;
    }

    /// <summary>
    /// 根据 ID 获取物品
    /// </summary>
    public static ConsumableItem GetItemById(string itemId)
    {
        var allItems = GetAllItems();
        return allItems.Find(item => item.itemId == itemId);
    }

    // ========== 食物分类 (Food) ==========

    private static ConsumableItem GetFoodConvenience()
{
    return new ConsumableItem(
        id: "food_convenience",
        nameKey: "item_food_convenience",
        categoryType: "food",
        costGold: 15f,
        timeHours: 0.5f,
        health: 10f,
        v: 1f,
        a: -0.5f,
        descKey: "desc_food_convenience"
    )
    {
        itemColor = new Color(1f, 0.8f, 0.4f),
        hungerRestore = 30f 
    };
}

    private static ConsumableItem GetFoodRestaurant()
    {
        return new ConsumableItem(
            id: "food_restaurant",
            nameKey: "item_food_restaurant",
            categoryType: "food",
            costGold: 50f,
            timeHours: 1.5f,
            health: 30f,
            v: 2f,
            a: 0f,
            descKey: "desc_food_restaurant"
        )
        {
            itemColor = new Color(1f, 0.8f, 0.4f),
            hungerRestore = 50f
        };
    }

    private static ConsumableItem GetFoodInstant()
    {
        return new ConsumableItem(
            id: "food_instant",
            nameKey: "item_food_instant",
            categoryType: "food",
            costGold: 5f,
            timeHours: 0.25f,
            health: 3f,
            v: -0.5f,
            a: 0f,
            descKey: "desc_food_instant"
        )
        {
            itemColor = new Color(1f, 0.8f, 0.4f),
            hungerRestore = 20f,
        };
    }

    // ========== 休息分类 (Rest) ==========

    /// <summary>
    /// ✨ 修改后的充分睡眠 - 睡到第二天
    /// 标记为特殊物品，需要在ConsumeSystem中特殊处理
    /// </summary>
    private static ConsumableItem GetRestSleep()
    {
        return new ConsumableItem(
            id: "rest_sleep",
            nameKey: "item_rest_sleep",
            categoryType: "rest",
            costGold: 0f,
            timeHours: -1f,  // ✨ 特殊标记：-1表示"睡到第二天"
            health: 50f,
            v: 1f,
            a: -2f,
            descKey: "desc_rest_sleep"
        )
        {
            itemColor = new Color(0.5f, 1f, 0.5f),
            isSleepToNextDay = true  // ✨ 新增标记
        };
    }

    private static ConsumableItem GetRestNap()
    {
        return new ConsumableItem(
            id: "rest_nap",
            nameKey: "item_rest_nap",
            categoryType: "rest",
            costGold: 0f,
            timeHours: 1f,
            health: 10f,
            v: 0.5f,
            a: -1f,
            descKey: "desc_rest_nap"
        )
        {
            itemColor = new Color(0.5f, 1f, 0.5f)
        };
    }

    private static ConsumableItem GetRestMeditation()
    {
        return new ConsumableItem(
            id: "rest_meditation",
            nameKey: "item_rest_meditation",
            categoryType: "rest",
            costGold: 0f,
            timeHours: 0.5f,
            health: 5f,
            v: 0.5f,
            a: -1.5f,
            descKey: "desc_rest_meditation"
        )
        {
            itemColor = new Color(0.5f, 1f, 0.5f)
        };
    }

    // ========== 娱乐分类 (Entertainment) ==========

    private static ConsumableItem GetEntertainmentMovie()
    {
        return new ConsumableItem(
            id: "ent_movie",
            nameKey: "item_ent_movie",
            categoryType: "entertainment",
            costGold: 30f,
            timeHours: 2.5f,
            health: -5f,
            v: 2f,
            a: 0.5f,
            descKey: "desc_ent_movie"
        )
        {
            itemColor = new Color(1f, 0.5f, 1f)
        };
    }

    private static ConsumableItem GetEntertainmentCoffee()
    {
        return new ConsumableItem(
            id: "ent_coffee",
            nameKey: "item_ent_coffee",
            categoryType: "entertainment",
            costGold: 20f,
            timeHours: 1.5f,
            health: 0f,
            v: 1.5f,
            a: -0.5f,
            descKey: "desc_ent_coffee"
        )
        {
            itemColor = new Color(1f, 0.5f, 1f)
        };
    }

    private static ConsumableItem GetEntertainmentGame()
    {
        return new ConsumableItem(
            id: "ent_game",
            nameKey: "item_ent_game",
            categoryType: "entertainment",
            costGold: 0f,
            timeHours: 2f,
            health: -3f,
            v: 1.5f,
            a: 1f,
            descKey: "desc_ent_game"
        )
        {
            itemColor = new Color(1f, 0.5f, 1f)
        };
    }

    // ========== 工具分类 (Tool) ==========

    private static ConsumableItem GetToolBicycle()
    {
        return new ConsumableItem(
            id: "tool_bicycle",
            nameKey: "item_tool_bicycle",
            categoryType: "tool",
            costGold: 200f,
            timeHours: 0.5f,
            health: 0f,
            v: 1f,
            a: 0f,
            descKey: "desc_tool_bicycle"
        )
        {
            itemColor = new Color(0.5f, 0.8f, 1f)
        };
    }

    private static ConsumableItem GetToolVitamins()
    {
        return new ConsumableItem(
            id: "tool_vitamins",
            nameKey: "item_tool_vitamins",
            categoryType: "tool",
            costGold: 80f,
            timeHours: 0.25f,
            health: 20f,
            v: 0.5f,
            a: 0f,
            descKey: "desc_tool_vitamins"
        )
        {
            itemColor = new Color(0.5f, 0.8f, 1f)
        };
    }

    // ========== 调试方法 ==========

    public static void DebugPrintAllItems()
    {
        var items = GetAllItems();
        Debug.Log($"\n========== 消费物品数据库 ({items.Count} 个物品) ==========");

        var categories = new Dictionary<string, List<ConsumableItem>>();
        foreach (var item in items)
        {
            if (!categories.ContainsKey(item.category))
                categories[item.category] = new List<ConsumableItem>();
            categories[item.category].Add(item);
        }

        foreach (var kvp in categories)
        {
            Debug.Log($"\n--- {kvp.Key.ToUpper()} ({kvp.Value.Count}) ---");
            foreach (var item in kvp.Value)
            {
                Debug.Log($"  • {item.itemId}: ¥{item.cost} | {item.timeRequired}h | " +
                         $"HP{item.healthGain:+0;-0;0} | V{item.vChange:+0.0;-0.0;0} A{item.aChange:+0.0;-0.0;0}");
            }
        }

        Debug.Log($"\n=====================================================\n");
    }

    public static void DebugPrintCategory(string category)
    {
        var items = GetItemsByCategory(category);
        Debug.Log($"\n========== {category.ToUpper()} 分类 ({items.Count} 个物品) ==========");
        foreach (var item in items)
        {
            Debug.Log($"  • {item.itemId}: {item.itemNameKey}");
            item.DebugPrint();
        }
        Debug.Log($"=====================================================\n");
    }
}