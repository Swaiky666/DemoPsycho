using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 本地化设置编辑器 - 包含事件故事和选项
/// 在原有的基础上添加所有事件相关的 Key
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

        var allStrings = CreateAllLocalizedStrings();
        config.strings = allStrings.ToArray();
        config.Initialize();

        EditorUtility.SetDirty(config);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("成功", 
            $"已添加 {allStrings.Count} 个本地化 Key 到 LocalizationConfig", 
            "确定");

        Debug.Log($"[LocalizationSetup] 成功添加 {allStrings.Count} 个 Key");
    }

    private static List<LocalizationConfig.LocalizedString> CreateAllLocalizedStrings()
    {
        var strings = new List<LocalizationConfig.LocalizedString>();

        // ===== 基础属性 =====
        strings.Add(new LocalizationConfig.LocalizedString { key = "health", chinese = "健康", english = "Health" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "gold", chinese = "金币", english = "Gold" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "skill", chinese = "技能", english = "Skill" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "valence", chinese = "情绪V", english = "Emotion V" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "arousal", chinese = "情绪A", english = "Emotion A" });

        // ===== 时间系统 =====
        strings.Add(new LocalizationConfig.LocalizedString { key = "morning", chinese = "早上", english = "Morning" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "noon", chinese = "中午", english = "Noon" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "afternoon", chinese = "下午", english = "Afternoon" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "night", chinese = "晚上", english = "Night" });

        // ===== 工作系统 =====
        strings.Add(new LocalizationConfig.LocalizedString { key = "job_job_delivery", chinese = "送快递", english = "Delivery" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "job_job_restaurant", chinese = "餐厅端盘子", english = "Restaurant" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "job_job_leaflet", chinese = "发传单", english = "Leaflet" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "job_job_bar", chinese = "酒馆帮工", english = "Bar Work" });

        // ===== 消费物品 =====
        strings.Add(new LocalizationConfig.LocalizedString { key = "item_food_convenience", chinese = "便利店便当", english = "Convenience Store Bento" });
        strings.Add(new LocalizationConfig.LocalizedString { key = "item_rest_sleep", chinese = "充足睡眠", english = "Full Sleep" });

        // ===== ✨ 事件故事文本 =====
        
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
}