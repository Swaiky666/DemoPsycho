using UnityEditor;
using UnityEngine;

/// <summary>
/// 本地化设置编辑器脚本 - 最终版本
/// 包含所有必需的 Key，包括情绪分类标签
/// 
/// 使用方法:
/// 1. 将此文件放入 Assets/Editor 文件夹
/// 2. 菜单 → Tools → Localization → Setup All Keys
/// 3. 完成！
/// </summary>
public class LocalizationSetupEditor
{
    [MenuItem("Tools/Localization/Setup All Keys")]
    public static void SetupAllKeys()
    {
        LocalizationConfig config = Resources.Load<LocalizationConfig>("LocalizationConfig");
        
        if (config == null)
        {
            string[] guids = AssetDatabase.FindAssets("LocalizationConfig t:LocalizationConfig");
            
            if (guids.Length == 0)
            {
                EditorUtility.DisplayDialog("错误", "找不到 LocalizationConfig！\n\n请确保:\n1. LocalizationConfig ScriptableObject 存在\n2. 如果不在 Resources 文件夹中，请手动选择", "确定");
                return;
            }
            
            string configPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            config = AssetDatabase.LoadAssetAtPath<LocalizationConfig>(configPath);
        }

        if (config == null)
        {
            EditorUtility.DisplayDialog("错误", "无法加载 LocalizationConfig", "确定");
            return;
        }

        var allStrings = CreateAllLocalizedStrings();
        config.strings = allStrings.ToArray();
        config.Initialize();

        EditorUtility.SetDirty(config);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("成功", 
            $"已添加 {allStrings.Count} 个本地化 Key 到 LocalizationConfig\n\n保存位置: {AssetDatabase.GetAssetPath(config)}", 
            "确定");

        Debug.Log($"[LocalizationSetup] 成功添加 {allStrings.Count} 个 Key");
    }

    private static System.Collections.Generic.List<LocalizationConfig.LocalizedString> CreateAllLocalizedStrings()
    {
        var strings = new System.Collections.Generic.List<LocalizationConfig.LocalizedString>();

        // ===== 基础属性 (5 个) =====
        strings.Add(new LocalizationConfig.LocalizedString { key = "health", chinese = "健康", english = "Health" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "gold", chinese = "金币", english = "Gold" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "skill", chinese = "技能", english = "Skill" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "valence", chinese = "情绪V", english = "Emotion V" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "arousal", chinese = "情绪A", english = "Emotion A" });

        // ===== 时间系统 - 周期 (2 个) =====
        strings.Add(new LocalizationConfig.LocalizedString { key = "week", chinese = "周", english = "Week" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "day", chinese = "天", english = "Day" });

        // ===== 时间系统 - 时段名称 (4 个) =====
        strings.Add(new LocalizationConfig.LocalizedString { key = "morning", chinese = "早上", english = "Morning" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "noon", chinese = "中午", english = "Noon" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "afternoon", chinese = "下午", english = "Afternoon" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "night", chinese = "晚上", english = "Night" });

        // ===== 时间系统 - UI 标签 (3 个) =====
        strings.Add(new LocalizationConfig.LocalizedString { key = "time_slot", chinese = "时间段", english = "Time Slot" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "progress", chinese = "进度", english = "Progress" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "remaining", chinese = "剩余", english = "Remaining" });

        // ===== 工作系统 - 工作名称 (4 个) =====
        strings.Add(new LocalizationConfig.LocalizedString { key = "job_delivery", chinese = "送快递", english = "Delivery" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "job_restaurant", chinese = "餐厅端盘子", english = "Restaurant" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "job_leaflet", chinese = "发传单", english = "Leaflet" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "job_bar", chinese = "酒馆帮工", english = "Bar Work" });

        // ===== 工作系统 - 卡牌标签 (4 个) =====
        strings.Add(new LocalizationConfig.LocalizedString { key = "skill_required", chinese = "技能要求", english = "Skill Required" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "pay", chinese = "薪资", english = "Pay" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "job_description", chinese = "工作描述", english = "Job Description" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "available", chinese = "可用", english = "Available" });

        // ===== UI 按钮标签 (5 个) =====
        strings.Add(new LocalizationConfig.LocalizedString { key = "close", chinese = "关闭", english = "Close" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "confirm", chinese = "确认", english = "Confirm" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "cancel", chinese = "取消", english = "Cancel" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "back", chinese = "返回", english = "Back" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "next", chinese = "下一个", english = "Next" });

        // ===== 游戏状态标签 (3 个) =====
        strings.Add(new LocalizationConfig.LocalizedString { key = "game_start", chinese = "游戏开始", english = "Game Start" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "game_end", chinese = "游戏结束", english = "Game End" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "game_over", chinese = "游戏结束", english = "Game Over" });

        // ===== 系统消息 (4 个) =====
        strings.Add(new LocalizationConfig.LocalizedString { key = "loading", chinese = "加载中", english = "Loading" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "error", chinese = "错误", english = "Error" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "success", chinese = "成功", english = "Success" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "warning", chinese = "警告", english = "Warning" });

        // ===== 通用词汇 (10 个) =====
        strings.Add(new LocalizationConfig.LocalizedString { key = "name", chinese = "名称", english = "Name" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "description", chinese = "描述", english = "Description" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "time", chinese = "时间", english = "Time" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "status", chinese = "状态", english = "Status" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "level", chinese = "等级", english = "Level" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "experience", chinese = "经验", english = "Experience" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "requirement", chinese = "需求", english = "Requirement" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "reward", chinese = "奖励", english = "Reward" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "cost", chinese = "花费", english = "Cost" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "value", chinese = "数值", english = "Value" });

        // ===== 消费物品名称 (11 个) =====
        strings.Add(new LocalizationConfig.LocalizedString { key = "item_food_convenience", chinese = "便利店便当", english = "Convenience Store Bento" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "item_food_restaurant", chinese = "餐厅大餐", english = "Restaurant Meal" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "item_food_instant", chinese = "速冻面", english = "Instant Noodles" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "item_rest_sleep", chinese = "充足睡眠", english = "Full Sleep" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "item_rest_nap", chinese = "小憩", english = "Power Nap" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "item_rest_meditation", chinese = "冥想", english = "Meditation" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "item_ent_movie", chinese = "看电影", english = "Watch Movie" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "item_ent_coffee", chinese = "咖啡馆闲逛", english = "Coffee Shop" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "item_ent_game", chinese = "玩游戏", english = "Play Game" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "item_tool_bicycle", chinese = "购买自行车", english = "Buy Bicycle" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "item_tool_vitamins", chinese = "购买维生素", english = "Buy Vitamins" });

        // ===== 物品描述 (11 个) =====
        strings.Add(new LocalizationConfig.LocalizedString { key = "desc_food_convenience", chinese = "简单填饱肚子", english = "Quick and filling" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "desc_food_restaurant", chinese = "高质量的餐厅用餐体验", english = "Quality dining experience" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "desc_food_instant", chinese = "快速充饥但营养不足", english = "Fast but nutritious" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "desc_rest_sleep", chinese = "恢复精神", english = "Restore spirit" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "desc_rest_nap", chinese = "快速恢复", english = "Quick recovery" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "desc_rest_meditation", chinese = "平静心绪", english = "Calm mind" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "desc_ent_movie", chinese = "娱乐身心", english = "Entertainment" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "desc_ent_coffee", chinese = "舒适的环境", english = "Comfortable atmosphere" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "desc_ent_game", chinese = "沉浸式娱乐", english = "Immersive entertainment" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "desc_tool_bicycle", chinese = "提升送快递效率减20%时间", english = "Improve delivery efficiency -20% time" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "desc_tool_vitamins", chinese = "提升身体状态", english = "Improve physical condition" });

        // ===== 分类 (4 个) =====
        strings.Add(new LocalizationConfig.LocalizedString { key = "category_food", chinese = "食物", english = "Food" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "category_rest", chinese = "休息", english = "Rest" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "category_entertainment", chinese = "娱乐", english = "Entertainment" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "category_tool", chinese = "工具", english = "Tool" });

        // ===== 消费系统消息 (8 个) =====
        strings.Add(new LocalizationConfig.LocalizedString { key = "msg_insufficient_gold", chinese = "金币不足", english = "Insufficient Gold" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "msg_consume_success", chinese = "成功使用", english = "Successfully used" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "msg_time_insufficient", chinese = "时间不足", english = "Insufficient Time" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "msg_cost_gold", chinese = "花费金币", english = "Cost Gold" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "msg_consume_time", chinese = "消耗时间", english = "Time Required" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "msg_health_change", chinese = "健康变化", english = "Health Change" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "msg_emotion_change", chinese = "情绪变化", english = "Emotion Change" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "msg_remaining_gold", chinese = "剩余金币", english = "Remaining Gold" });

        // ===== 时间系统显示 (5 个) =====
        strings.Add(new LocalizationConfig.LocalizedString { key = "display_time_used", chinese = "已用", english = "Used" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "display_time_percent", chinese = "进度", english = "Progress" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "display_elapsed_time", chinese = "消耗时间", english = "Elapsed Time" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "display_remaining_time", chinese = "剩余时间", english = "Remaining Time" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "display_hours", chinese = "小时", english = "Hours" });

        // ===== 情绪系统 - 强度等级 (4 个) =====
        strings.Add(new LocalizationConfig.LocalizedString { key = "emotion_intensity_mild", chinese = "轻度", english = "Mild" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "emotion_intensity_moderate", chinese = "中度", english = "Moderate" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "emotion_intensity_intense", chinese = "强烈", english = "Intense" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "neutral", chinese = "中性", english = "Neutral" });

        // ===== 情绪系统 - 情绪名称 (8 个) =====
        strings.Add(new LocalizationConfig.LocalizedString { key = "emotion_Pleasant", chinese = "快乐", english = "Pleasant" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "emotion_Excited", chinese = "兴奋", english = "Excited" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "emotion_Angry", chinese = "愤怒", english = "Angry" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "emotion_Sad", chinese = "悲伤", english = "Sad" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "emotion_Content", chinese = "满足", english = "Content" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "emotion_Nervous", chinese = "紧张", english = "Nervous" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "emotion_Depressed", chinese = "沮丧", english = "Depressed" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "emotion_Calm", chinese = "平静", english = "Calm" });

        // ===== 情绪系统 - 分类标签 (4 个) ★★★ 最关键的部分 ★★★ =====
        strings.Add(new LocalizationConfig.LocalizedString { key = "emotion_category_activation", chinese = "活跃", english = "Energetic" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "emotion_category_pleasant", chinese = "愉快", english = "Pleasant" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "emotion_category_deactivation", chinese = "平静", english = "Calm" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "emotion_category_unpleasant", chinese = "不愉快", english = "Unpleasant" });

        // ===== 调试标签 (6 个) =====
        strings.Add(new LocalizationConfig.LocalizedString { key = "debug_valence", chinese = "V", english = "V" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "debug_arousal", chinese = "A", english = "A" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "debug_radius", chinese = "r", english = "r" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "debug_angle", chinese = "θ", english = "θ" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "debug_intensity", chinese = "强度", english = "Intensity" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "debug_sector", chinese = "扇区", english = "Sector" });

        // ===== 其他 (2 个) =====
        strings.Add(new LocalizationConfig.LocalizedString { key = "stats_health_low", chinese = "健康过低", english = "Health Too Low" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "lang_chinese", chinese = "中文", english = "Chinese" });

        return strings;
    }

    [MenuItem("Tools/Localization/Verify Keys")]
    public static void VerifyKeys()
    {
        LocalizationConfig config = Resources.Load<LocalizationConfig>("LocalizationConfig");

        if (config == null)
        {
            string[] guids = AssetDatabase.FindAssets("LocalizationConfig t:LocalizationConfig");
            if (guids.Length > 0)
            {
                string configPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                config = AssetDatabase.LoadAssetAtPath<LocalizationConfig>(configPath);
            }
        }

        if (config == null)
        {
            EditorUtility.DisplayDialog("错误", "找不到 LocalizationConfig", "确定");
            return;
        }

        var keySet = new System.Collections.Generic.HashSet<string>();
        var duplicates = new System.Collections.Generic.List<string>();
        var missingTranslations = new System.Collections.Generic.List<string>();

        foreach (var str in config.strings)
        {
            if (string.IsNullOrEmpty(str.key))
            {
                missingTranslations.Add("空 Key");
                continue;
            }

            if (keySet.Contains(str.key))
            {
                duplicates.Add(str.key);
            }
            else
            {
                keySet.Add(str.key);
            }

            if (string.IsNullOrEmpty(str.chinese) || string.IsNullOrEmpty(str.english))
            {
                missingTranslations.Add($"{str.key} (缺少翻译)");
            }
        }

        string message = $"总计 Key 数: {config.strings.Length}\n\n";

        if (duplicates.Count > 0)
        {
            message += $"重复 Key ({duplicates.Count}):\n";
            foreach (var dup in duplicates)
                message += $"  - {dup}\n";
        }
        else
        {
            message += "✓ 无重复 Key\n";
        }

        if (missingTranslations.Count > 0)
        {
            message += $"\n缺少翻译 ({missingTranslations.Count}):\n";
            foreach (var miss in missingTranslations)
                message += $"  - {miss}\n";
        }
        else
        {
            message += "\n✓ 所有翻译完整\n";
        }

        EditorUtility.DisplayDialog("验证结果", message, "确定");
        Debug.Log(message);
    }

    [MenuItem("Tools/Localization/Clear All Keys")]
    public static void ClearAllKeys()
    {
        if (!EditorUtility.DisplayDialog("警告", "确定要删除所有 Key 吗？\n\n此操作无法撤销！", "删除", "取消"))
            return;

        LocalizationConfig config = Resources.Load<LocalizationConfig>("LocalizationConfig");

        if (config == null)
        {
            string[] guids = AssetDatabase.FindAssets("LocalizationConfig t:LocalizationConfig");
            if (guids.Length > 0)
            {
                string configPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                config = AssetDatabase.LoadAssetAtPath<LocalizationConfig>(configPath);
            }
        }

        if (config == null)
        {
            EditorUtility.DisplayDialog("错误", "找不到 LocalizationConfig", "确定");
            return;
        }

        config.strings = new LocalizationConfig.LocalizedString[0];
        config.Initialize();

        EditorUtility.SetDirty(config);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("完成", "已清除所有 Key", "确定");
    }

    [MenuItem("Tools/Localization/Export Keys to CSV")]
    public static void ExportToCSV()
    {
        LocalizationConfig config = Resources.Load<LocalizationConfig>("LocalizationConfig");

        if (config == null)
        {
            string[] guids = AssetDatabase.FindAssets("LocalizationConfig t:LocalizationConfig");
            if (guids.Length > 0)
            {
                string configPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                config = AssetDatabase.LoadAssetAtPath<LocalizationConfig>(configPath);
            }
        }

        if (config == null)
        {
            EditorUtility.DisplayDialog("错误", "找不到 LocalizationConfig", "确定");
            return;
        }

        string csv = "Key,Chinese,English\n";
        foreach (var str in config.strings)
        {
            csv += $"{str.key},{str.chinese},{str.english}\n";
        }

        string exportPath = EditorUtility.SaveFilePanel("导出 CSV", "", "localization_keys", "csv");
        if (string.IsNullOrEmpty(exportPath))
            return;

        System.IO.File.WriteAllText(exportPath, csv);
        EditorUtility.DisplayDialog("成功", $"已导出到:\n{exportPath}", "确定");
        Debug.Log($"已导出 {config.strings.Length} 个 Key 到 CSV");
    }
}