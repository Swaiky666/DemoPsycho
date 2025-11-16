using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 完整版本地化编辑器 - 追加模式
/// 包含所有原有Key + 新的事件故事Key
/// 只添加不存在的Key，保留所有原有Key
/// 
/// 使用方法:
/// 1. 菜单 → Tools → Localization → Setup Complete Localization
/// 2. 自动添加所有缺少的Key
/// 3. 完成！
/// </summary>
public class LocalizationCompleteEditor
{
    [MenuItem("Tools/Localization/Setup Complete Localization")]
    public static void SetupCompleteLocalization()
    {
        LocalizationConfig config = Resources.Load<LocalizationConfig>("LocalizationConfig");
        
        if (config == null)
        {
            string[] guids = AssetDatabase.FindAssets("LocalizationConfig t:LocalizationConfig");
            
            if (guids.Length == 0)
            {
                EditorUtility.DisplayDialog("错误", "找不到 LocalizationConfig！", "确定");
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

        // 获取现有的所有 Key
        var existingKeys = new HashSet<string>(config.strings.Select(s => s.key));
        
        int beforeCount = config.strings.Length;
        
        // 生成所有Key（包括原有的和新的）
        var allKeys = CreateAllLocalizationKeys();
        
        // 过滤掉已存在的Key
        var keysToAdd = allKeys.Where(k => !existingKeys.Contains(k.key)).ToList();
        
        // 将新Key添加到现有列表
        var allStrings = new List<LocalizationConfig.LocalizedString>(config.strings);
        allStrings.AddRange(keysToAdd);
        
        config.strings = allStrings.ToArray();
        config.Initialize();

        EditorUtility.SetDirty(config);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("成功", 
            $"本地化设置完成！\n\n" +
            $"原有 Key 数: {beforeCount}\n" +
            $"新增 Key 数: {keysToAdd.Count}\n" +
            $"总 Key 数: {config.strings.Length}", 
            "确定");

        Debug.Log($"[LocalizationSetup] 成功添加 {keysToAdd.Count} 个 Key，总数: {config.strings.Length}");
    }

    /// <summary>
    /// 生成所有本地化Key（原有的 + 事件的）
    /// </summary>
    private static List<LocalizationConfig.LocalizedString> CreateAllLocalizationKeys()
    {
        var strings = new List<LocalizationConfig.LocalizedString>();

        // ===== 原有的基础Key (保留所有) =====
        
        // 基础属性 (5 个)
        strings.Add(new LocalizationConfig.LocalizedString { key = "health", chinese = "健康", english = "Health" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "gold", chinese = "金币", english = "Gold" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "skill", chinese = "技能", english = "Skill" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "valence", chinese = "情绪V", english = "Emotion V" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "arousal", chinese = "情绪A", english = "Emotion A" });

        // 时间系统 - 周期
        strings.Add(new LocalizationConfig.LocalizedString { key = "week", chinese = "周", english = "Week" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "day", chinese = "天", english = "Day" });

        // 时间系统 - 时段名称 (4 个)
        strings.Add(new LocalizationConfig.LocalizedString { key = "morning", chinese = "早上", english = "Morning" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "noon", chinese = "中午", english = "Noon" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "afternoon", chinese = "下午", english = "Afternoon" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "night", chinese = "晚上", english = "Night" });

        // 时间系统 - UI 标签
        strings.Add(new LocalizationConfig.LocalizedString { key = "time_slot", chinese = "时间段", english = "Time Slot" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "progress", chinese = "进度", english = "Progress" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "remaining", chinese = "剩余", english = "Remaining" });

        // 工作系统 - 工作名称
        strings.Add(new LocalizationConfig.LocalizedString { key = "job_job_delivery", chinese = "送快递", english = "Delivery" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "job_job_restaurant", chinese = "餐厅端盘子", english = "Restaurant" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "job_job_leaflet", chinese = "发传单", english = "Leaflet" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "job_job_bar", chinese = "酒馆帮工", english = "Bar Work" });

        // 工作系统 - 工作描述
        strings.Add(new LocalizationConfig.LocalizedString { key = "job_desc_job_delivery", chinese = "骑车送快递，体力活。获得丰厚报酬。", english = "Deliver packages by bike. Good payment." });
        strings.Add(new LocalizationConfig.LocalizedString { key = "job_desc_job_restaurant", chinese = "在餐厅端盘子，需要一定技能。", english = "Wait tables at restaurant. Requires skill." });
        strings.Add(new LocalizationConfig.LocalizedString { key = "job_desc_job_leaflet", chinese = "在街头发放传单，轻松工作。", english = "Distribute leaflets. Light work." });
        strings.Add(new LocalizationConfig.LocalizedString { key = "job_desc_job_bar", chinese = "在酒馆帮工，需要较高的技能。", english = "Work at bar. Requires high skill." });

        // 工作系统 - 卡牌标签
        strings.Add(new LocalizationConfig.LocalizedString { key = "skill_required", chinese = "技能要求", english = "Skill Required" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "pay", chinese = "薪资", english = "Pay" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "job_description", chinese = "工作描述", english = "Job Description" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "available", chinese = "可用", english = "Available" });

        // UI 按钮标签
        strings.Add(new LocalizationConfig.LocalizedString { key = "close", chinese = "关闭", english = "Close" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "confirm", chinese = "确认", english = "Confirm" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "cancel", chinese = "取消", english = "Cancel" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "back", chinese = "返回", english = "Back" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "next", chinese = "下一个", english = "Next" });

        // 游戏状态标签
        strings.Add(new LocalizationConfig.LocalizedString { key = "game_start", chinese = "游戏开始", english = "Game Start" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "game_end", chinese = "游戏结束", english = "Game End" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "game_over", chinese = "游戏结束", english = "Game Over" });

        // 系统消息
        strings.Add(new LocalizationConfig.LocalizedString { key = "loading", chinese = "加载中", english = "Loading" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "error", chinese = "错误", english = "Error" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "success", chinese = "成功", english = "Success" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "warning", chinese = "警告", english = "Warning" });

        // 通用词汇
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

        // 消费系统 - 卡牌标签
        strings.Add(new LocalizationConfig.LocalizedString { key = "time_required", chinese = "消耗时间", english = "Time Required" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "health_change", chinese = "健康变化", english = "Health Change" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "category", chinese = "分类", english = "Category" });

        // 消费物品分类
        strings.Add(new LocalizationConfig.LocalizedString { key = "category_food", chinese = "食物", english = "Food" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "category_rest", chinese = "休息", english = "Rest" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "category_entertainment", chinese = "娱乐", english = "Entertainment" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "category_tool", chinese = "工具", english = "Tool" });

        // 消费物品名称
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

        // 物品描述
        strings.Add(new LocalizationConfig.LocalizedString { key = "desc_food_convenience", chinese = "简单填饱肚子", english = "Quick and filling" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "desc_food_restaurant", chinese = "高质量的餐厅用餐体验", english = "Quality dining experience" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "desc_food_instant", chinese = "快速充饥但营养不足", english = "Fast but nutritious" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "desc_rest_sleep", chinese = "恢复精神", english = "Restore spirit" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "desc_rest_nap", chinese = "快速恢复", english = "Quick recovery" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "desc_rest_meditation", chinese = "心灵放松", english = "Mental relaxation" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "desc_ent_movie", chinese = "放松身心", english = "Relax yourself" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "desc_ent_coffee", chinese = "舒适的环境", english = "Comfortable place" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "desc_ent_game", chinese = "尽情娱乐", english = "Pure entertainment" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "desc_tool_bicycle", chinese = "增加移动效率", english = "Increase efficiency" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "desc_tool_vitamins", chinese = "补充营养", english = "Boost health" });

        // 情绪标签
        strings.Add(new LocalizationConfig.LocalizedString { key = "emotion_happy", chinese = "愉快", english = "Happy" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "emotion_sad", chinese = "悲伤", english = "Sad" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "emotion_excited", chinese = "兴奋", english = "Excited" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "emotion_unhappy", chinese = "不愉快", english = "Unhappy" });

        // ===== 新增：事件故事Key =====

        // 过劳事件
        strings.Add(new LocalizationConfig.LocalizedString { key = "story_overwork", chinese = "你感到浑身疲惫，眼睛酸涩，身体沉重得无法做任何事情。\n一个声音在告诉你，也许是时候休息一下了...", english = "You feel exhausted all over, with sore eyes and a heavy body. A voice tells you maybe it's time to rest..." });
        strings.Add(new LocalizationConfig.LocalizedString { key = "choice_overwork_rest", chinese = "好吧，今天就好好休息吧", english = "Okay, let me rest today" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "result_overwork_rest", chinese = "你躺在床上，很快就陷入了深度睡眠。醒来时，感到精神焕发。", english = "You fell into a deep sleep. Waking up, you feel refreshed." });
        strings.Add(new LocalizationConfig.LocalizedString { key = "choice_overwork_continue", chinese = "咬牙坚持，再工作一下", english = "Grit teeth and work more" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "result_overwork_continue", chinese = "你勉强撑过了这一段，但身体的抗议声越来越大...", english = "You pushed through, but your body protests louder..." });

        // 抑郁事件
        strings.Add(new LocalizationConfig.LocalizedString { key = "story_depression", chinese = "一种莫名的悲伤笼罩了你。\n天空似乎都变成了灰色，所有的事物都失去了颜色。", english = "An inexplicable sadness covers you. The sky turns gray and everything loses its color." });
        strings.Add(new LocalizationConfig.LocalizedString { key = "choice_depression_seek_help", chinese = "找朋友倾诉，寻求帮助", english = "Talk to a friend for help" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "result_depression_seek_help", chinese = "倾诉后，你感到了一些释然。也许，你不用一个人承担所有的重担。", english = "After talking, you feel some relief. Maybe you don't have to bear everything alone." });
        strings.Add(new LocalizationConfig.LocalizedString { key = "choice_depression_endure", chinese = "独自承受，相信时间会治愈", english = "Endure alone, trust time will heal" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "result_depression_endure", chinese = "黑暗似乎在逐渐褪去，但这个过程很漫长...", english = "The darkness seems to fade gradually, but it takes time..." });

        // 财富事件
        strings.Add(new LocalizationConfig.LocalizedString { key = "story_wealth", chinese = "看着银行账户上的数字，你突然意识到自己存了不少钱。\n一种陌生的满足感涌上心头。", english = "Looking at your account balance, you realize you've saved quite a bit. A strange sense of satisfaction fills you." });
        strings.Add(new LocalizationConfig.LocalizedString { key = "choice_wealth_save", chinese = "继续存钱，为未来计划", english = "Keep saving for the future" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "result_wealth_save", chinese = "你感到一种安全感。也许存够了钱，就能做自己想做的事...", english = "You feel secure. Perhaps with enough money, you can do what you want..." });
        strings.Add(new LocalizationConfig.LocalizedString { key = "choice_wealth_spend", chinese = "犒劳自己，好好消费一番", english = "Treat yourself and enjoy" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "result_wealth_spend", chinese = "购物给了你暂时的快乐，虽然钱包瘪了一些，但心情确实变好了。", english = "Shopping gave you temporary joy. Your wallet is lighter, but your mood is better." });

        // 健康危机事件
        strings.Add(new LocalizationConfig.LocalizedString { key = "story_health_crisis", chinese = "突然的不适让你意识到，也许长期的透支终于开始报应了。\n你需要做出选择。", english = "A sudden discomfort makes you realize long-term overwork is catching up. You must choose." });
        strings.Add(new LocalizationConfig.LocalizedString { key = "choice_see_doctor", chinese = "立即去看医生", english = "Go see a doctor immediately" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "result_see_doctor", chinese = "医生的诊断让你松了一口气——还不是太严重。但确实需要好好休养。", english = "The doctor's diagnosis relieves you - it's not too serious. But you need proper rest." });
        strings.Add(new LocalizationConfig.LocalizedString { key = "choice_ignore", chinese = "吃点药硬撑，继续工作", english = "Take medicine and push through" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "result_ignore", chinese = "你试图忽视身体的信号，但这似乎不是个明智的决定...", english = "You try to ignore your body, but this seems unwise..." });

        // 工作机会事件
        strings.Add(new LocalizationConfig.LocalizedString { key = "story_job_opportunity", chinese = "一个新的工作机会出现了。\n机会难得，但你不确定自己是否能胜任...", english = "A new job opportunity appears. It's rare, but you're not sure if you can handle it..." });
        strings.Add(new LocalizationConfig.LocalizedString { key = "choice_take_chance", chinese = "冒险一试，挑战自己", english = "Take the risk and challenge yourself" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "result_take_chance", chinese = "虽然有些吃力，但你在挑战中学到了很多。", english = "It's challenging, but you learn a lot from it." });
        strings.Add(new LocalizationConfig.LocalizedString { key = "choice_play_safe", chinese = "选择安全，坚持原来的工作", english = "Play it safe and stay" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "result_play_safe", chinese = "至少不会失败，但你多少有些遗憾...", english = "At least you won't fail, but you feel some regret..." });

        // 孤独感事件
        strings.Add(new LocalizationConfig.LocalizedString { key = "story_loneliness", chinese = "夜幕降临时，一种强烈的孤独感笼罩了你。\n你想起了朋友，想起了陪伴。", english = "At nightfall, a strong sense of loneliness covers you. You think of friends and companionship." });
        strings.Add(new LocalizationConfig.LocalizedString { key = "choice_reach_out", chinese = "主动联系朋友", english = "Reach out to friends" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "result_reach_out", chinese = "朋友的热情回应让你感到温暖。也许有人在乎你。", english = "Your friend's warm response makes you feel loved. Maybe someone cares." });
        strings.Add(new LocalizationConfig.LocalizedString { key = "choice_withdraw", chinese = "继续独处，沉浸在思绪中", english = "Stay alone in your thoughts" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "result_withdraw", chinese = "孤独依旧...", english = "The loneliness remains..." });

        // 新友谊事件
        strings.Add(new LocalizationConfig.LocalizedString { key = "story_friendship", chinese = "在某个不期而至的时刻，一个新的朋友进入了你的生活。\n也许这会改变些什么...", english = "At an unexpected moment, a new friend enters your life. Maybe this will change things..." });
        strings.Add(new LocalizationConfig.LocalizedString { key = "choice_embrace", chinese = "热情地拥抱这段新的友谊", english = "Embrace this new friendship" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "result_embrace", chinese = "新朋友给了你许多欢笑。生活顿时充满了新的可能。", english = "Your new friend brings laughter. Life suddenly has new possibilities." });
        strings.Add(new LocalizationConfig.LocalizedString { key = "choice_hesitate", chinese = "保持距离，谨慎相处", english = "Keep distance and be cautious" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "result_hesitate", chinese = "也许这是个不错的开始，但你还需要时间来确定...", english = "Maybe this is a good start, but you need time..." });

        // 第一周完成事件
        strings.Add(new LocalizationConfig.LocalizedString { key = "story_first_week", chinese = "第一周结束了。\n回顾这一周，你学到了很多，也经历了不少。\n恭喜你坚持到了现在。", english = "The first week is over. You learned a lot and experienced much. Congratulations on persisting!" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "choice_reflect", chinese = "整理思绪，为新的一周做准备", english = "Reflect and prepare for next week" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "result_reflect", chinese = "你已经踏出了第一步。继续加油！", english = "You've taken the first step. Keep going!" });

        // 一月纪念事件
        strings.Add(new LocalizationConfig.LocalizedString { key = "story_month_anniversary", chinese = "一个月已经过去了。\n这一个月里，你经历了欢笑，也经历了挫折。\n但你一直在坚持...", english = "A month has passed. You've experienced both joy and setback. But you persisted..." });
        strings.Add(new LocalizationConfig.LocalizedString { key = "choice_celebrate", chinese = "好好庆祝一下自己的坚持", english = "Celebrate your persistence" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "result_celebrate", chinese = "你值得这份庆祝。继续为更好的明天努力吧。", english = "You deserve this celebration. Keep working for a better tomorrow." });

        // 随机相遇
        strings.Add(new LocalizationConfig.LocalizedString { key = "story_random_meeting", chinese = "在街角咖啡馆，你巧遇了一个熟悉的面孔。\n该打招呼吗？", english = "At a corner cafe, you bump into a familiar face. Should you greet them?" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "choice_talk", chinese = "主动打招呼，聊一会儿", english = "Greet and chat" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "result_talk", chinese = "谈话比你想象的要愉快得多。也许这会成为新的开始。", english = "The conversation is more pleasant than expected. Maybe this is a new beginning." });
        strings.Add(new LocalizationConfig.LocalizedString { key = "choice_avoid", chinese = "假装没看到，匆匆离开", english = "Pretend not to see and leave" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "result_avoid", chinese = "你快速走开了，但心中留下了一丝遗憾。", english = "You quickly leave, but a hint of regret remains." });

        // 幸运事件
        strings.Add(new LocalizationConfig.LocalizedString { key = "story_serendipity", chinese = "一个意想不到的好事发生了。\n也许是命运的安排，也许只是巧合。", english = "Something unexpected and good happens. Maybe fate, maybe coincidence." });
        strings.Add(new LocalizationConfig.LocalizedString { key = "choice_accept_luck", chinese = "欣然接受这份幸运", english = "Happily accept this fortune" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "result_accept_luck", chinese = "好事已经降临。享受这一刻，也为未来的挑战积蓄力量。", english = "Good fortune arrived. Enjoy this moment and gather strength for future challenges." });

        // 周末休闲
        strings.Add(new LocalizationConfig.LocalizedString { key = "story_weekend", chinese = "周末到了。你有整个周末的时间。\n该怎么度过这个难得的休闲时刻呢？", english = "The weekend is here. You have the whole weekend. How will you spend this rare leisure time?" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "choice_relax", chinese = "好好放松，享受休闲时光", english = "Relax and enjoy leisure time" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "result_relax", chinese = "这样的休息正是你所需要的。", english = "This rest is exactly what you need." });
        strings.Add(new LocalizationConfig.LocalizedString { key = "choice_work_weekend", chinese = "利用周末继续工作，赚取更多金币", english = "Work and earn more money" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "result_work_weekend", chinese = "金币增加了，但疲惫也随之而来...", english = "Your money increased, but so did your fatigue..." });

        // 工作冲突
        strings.Add(new LocalizationConfig.LocalizedString { key = "story_work_conflict", chinese = "工作中发生了不愉快的冲突。\n客人的无理取闹让你感到委屈和愤怒。\n你该如何应对？", english = "An unpleasant conflict happened at work. The customer's unreasonableness upset you. How will you respond?" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "choice_resolve", chinese = "冷静下来，尝试理解和沟通", english = "Calm down and try to communicate" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "result_resolve", chinese = "虽然很难，但你最终化解了这个冲突。你成长了。", english = "Though difficult, you resolved the conflict. You've grown." });
        strings.Add(new LocalizationConfig.LocalizedString { key = "choice_conflict_ignore", chinese = "假装什么都没发生，继续工作", english = "Ignore and continue working" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "result_conflict_ignore", chinese = "你压抑了情绪，但心中的不满在积累...", english = "You suppressed your emotions, but dissatisfaction accumulates..." });

        // 升迁事件
        strings.Add(new LocalizationConfig.LocalizedString { key = "story_promotion", chinese = "你的上司找到了你。\n\"我们想提拔你担任更重要的职位...\"", english = "Your boss finds you. \"We want to promote you to a more important position...\"" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "choice_accept_promotion", chinese = "欣然接受这个晋升机会", english = "Accept the promotion" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "result_accept_promotion", chinese = "你的努力得到了认可。新的挑战等待着你。", english = "Your effort is recognized. New challenges await." });
        strings.Add(new LocalizationConfig.LocalizedString { key = "choice_decline_promotion", chinese = "婉言拒绝，坚持现在的工作", english = "Politely decline" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "result_decline_promotion", chinese = "也许这不是现在的你需要的。但机会也许会再来。", english = "Maybe this isn't what you need now. But opportunities may come again." });

        // 失业危机
        strings.Add(new LocalizationConfig.LocalizedString { key = "story_job_loss", chinese = "不幸的消息传来：你被解雇了。\n原因有很多，但这改变了什么。\n你需要重新开始。", english = "Bad news: You've been fired. For various reasons. You need to start over." });
        strings.Add(new LocalizationConfig.LocalizedString { key = "choice_find_new_job", chinese = "立即开始寻找新工作", english = "Start job hunting" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "result_find_new_job", chinese = "求职之路充满挑战，但你决不放弃。", english = "Job hunting is challenging, but you won't give up." });
        strings.Add(new LocalizationConfig.LocalizedString { key = "choice_take_break_job", chinese = "暂时休息，好好调整自己", english = "Take a break and adjust" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "result_take_break_job", chinese = "这段休息时间让你重新思考了生活的意义。", english = "This break made you reconsider life's meaning." });

        // 倦怠事件
        strings.Add(new LocalizationConfig.LocalizedString { key = "story_burnout", chinese = "工作、生活、一切似乎都失去了颜色。\n你感到疲惫、迷茫、无助。\n这不能再继续了...", english = "Work, life, everything has lost its color. You feel tired, lost, helpless. This can't continue..." });
        strings.Add(new LocalizationConfig.LocalizedString { key = "choice_take_vacation", chinese = "申请假期，来一次长假", english = "Take a long vacation" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "result_take_vacation", chinese = "远离喧嚣，你终于可以好好休息了。也许这正是你所需要的。", english = "Away from chaos, you can finally rest. Maybe this is what you needed." });
        strings.Add(new LocalizationConfig.LocalizedString { key = "choice_push_through_burnout", chinese = "咬牙坚持，继续前进", english = "Grit teeth and push through" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "result_push_through_burnout", chinese = "你坚持了下来，但代价很大...", english = "You persisted, but the cost is high..." });

        // 贫困事件
        strings.Add(new LocalizationConfig.LocalizedString { key = "story_poverty", chinese = "你的钱快没了。\n账户里只剩下一点点，足够维持几天的生活。\n你需要尽快赚钱。", english = "You're running out of money. Only enough for a few days left. You need to earn money urgently." });
        strings.Add(new LocalizationConfig.LocalizedString { key = "choice_work_hard_poverty", chinese = "加倍努力工作，尽快脱困", english = "Work harder to escape poverty" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "result_work_hard_poverty", chinese = "疲惫但充满希望。你相信这不会持续太久。", english = "Tired but hopeful. You believe this won't last long." });
        strings.Add(new LocalizationConfig.LocalizedString { key = "choice_get_help_poverty", chinese = "寻求帮助，也许朋友可以借给你一些钱", english = "Seek help from friends" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "result_get_help_poverty", chinese = "朋友伸出了援手。也许你不必独自面对这一切。", english = "Friends offered help. Maybe you don't have to face this alone." });

        // 意外开支
        strings.Add(new LocalizationConfig.LocalizedString { key = "story_unexpected_expense", chinese = "突然的事故导致了一笔意外开支。\n钱包瘪下来了不少。\n你该如何应对？", english = "An accident causes unexpected expenses. Your wallet shrinks. How will you handle this?" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "choice_expense_pay", chinese = "咬牙付款，接受这个现实", english = "Pay and accept reality" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "result_expense_pay", chinese = "虽然心痛，但至少事情得到了解决。", english = "Though painful, at least the problem is solved." });
        strings.Add(new LocalizationConfig.LocalizedString { key = "choice_expense_defer", chinese = "暂时延迟付款，希望能过这个难关", english = "Defer payment temporarily" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "result_expense_defer", chinese = "短期内你缓解了压力，但问题还是存在...", english = "You relieved immediate pressure, but the problem remains..." });

        // 投资机会
        strings.Add(new LocalizationConfig.LocalizedString { key = "story_investment", chinese = "一个投资的好机会摆在你面前。\n高收益也意味着高风险。\n你该冒这个险吗？", english = "A good investment opportunity appears. High returns mean high risk. Will you take it?" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "choice_invest_risky", chinese = "冒险投资，赌一把", english = "Invest and take a gamble" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "result_invest_risky", chinese = "赌对了！你的资产大幅增加。但下次呢？", english = "You won! Your assets increased. But next time?" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "choice_invest_safe", chinese = "保守投资，稳妥增长", english = "Conservative investment" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "result_invest_safe", chinese = "虽然收益不多，但至少没有风险。", english = "Modest returns, but at least no risk." });

        // 人生转折
        strings.Add(new LocalizationConfig.LocalizedString { key = "story_life_change", chinese = "一个转折点出现了。\n也许是机遇，也许是挑战。\n这会改变你的人生轨迹。", english = "A turning point appears. Maybe opportunity, maybe challenge. It will change your life." });
        strings.Add(new LocalizationConfig.LocalizedString { key = "choice_embrace_change", chinese = "勇敢拥抱这个变化", english = "Embrace the change bravely" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "result_embrace_change", chinese = "虽然充满了不确定，但你感到了新生的希望。", english = "Though uncertain, you feel hope for renewal." });
        strings.Add(new LocalizationConfig.LocalizedString { key = "choice_resist_change", chinese = "抗拒这个变化，维持现状", english = "Resist and maintain status quo" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "result_resist_change", chinese = "也许现在还不是改变的时候。", english = "Maybe now is not the time for change." });

        return strings;
    }

    [MenuItem("Tools/Localization/Check Duplicate Keys")]
    public static void CheckDuplicateKeys()
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

        var keySet = new HashSet<string>();
        var duplicates = new List<string>();

        foreach (var str in config.strings)
        {
            if (string.IsNullOrEmpty(str.key)) continue;

            if (keySet.Contains(str.key))
            {
                if (!duplicates.Contains(str.key))
                    duplicates.Add(str.key);
            }
            else
            {
                keySet.Add(str.key);
            }
        }

        if (duplicates.Count > 0)
        {
            string dupList = string.Join("\n", duplicates);
            EditorUtility.DisplayDialog("警告", $"发现 {duplicates.Count} 个重复 Key:\n\n{dupList}", "确定");
        }
        else
        {
            EditorUtility.DisplayDialog("检查完成", $"✓ 无重复 Key\n总 Key 数: {config.strings.Length}", "确定");
        }
    }
}