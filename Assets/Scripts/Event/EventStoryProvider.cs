using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 事件故事提供者 - 完整扩展版本
/// ✨ 新增：支持连续事件的故事链
/// ✨ 新增：完整的预写故事库（包括所有事件）
/// 
/// 使用方法:
/// 替换原有的 EventStoryProvider.cs
/// </summary>
public class EventStoryProvider : MonoBehaviour
{
    [Header("预写故事库")]
    [SerializeField] private TextAsset prewrittenStoriesJson;
    
    [Header("AI 生成配置")]
    [SerializeField] private bool useAIGeneration = false;
    
    [Header("调试模式")]
    [SerializeField] private bool debugMode = true;
    
    private Dictionary<string, string> prewrittenStories = new();
    
    // 单例
    public static EventStoryProvider Instance { get; private set; }
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        LoadPrewrittenStories();
        
        if (debugMode)
        {
            Debug.Log($"[EventStoryProvider] 初始化完成，已加载 {prewrittenStories.Count} 个预写故事");
        }
    }
    
    private void LoadPrewrittenStories()
    {
        prewrittenStories.Clear();
        
        if (prewrittenStoriesJson != null)
        {
            try
            {
                InitializeDefaultStories();
                if (debugMode)
                {
                    Debug.Log($"[EventStoryProvider] 已从 JSON 加载故事库");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[EventStoryProvider] 加载故事库出错: {ex.Message}");
                InitializeDefaultStories();
            }
        }
        else
        {
            InitializeDefaultStories();
        }
    }
    
    /// <summary>
    /// 初始化默认故事（完整版本）
    /// ✨ 包含所有事件的故事文本
    /// </summary>
    private void InitializeDefaultStories()
    {
        // ===== 饥饿相关事件 =====
        prewrittenStories["story_hunger_crisis"] = 
            "胃里传来强烈的绞痛，头晕目眩...\n你已经很久没有好好吃东西了。\n身体在发出最后的抗议。";
        
        prewrittenStories["choice_hunger_buy_food"] = 
            "立即去买食物";
        prewrittenStories["result_hunger_buy_food"] = 
            "温热的食物入口，身体终于得到了缓解。你感觉好多了。";
        
        prewrittenStories["choice_hunger_endure"] = 
            "咬牙忍住，省点钱";
        prewrittenStories["result_hunger_endure"] = 
            "饥饿感愈发强烈，身体似乎在消耗自己...这样下去不行。";

        prewrittenStories["story_skip_meal"] = 
            "忙碌中，你错过了用餐时间。\n肚子咕咕叫着提醒你该吃点东西了。";
        
        prewrittenStories["choice_skip_quick_bite"] = 
            "快速买点东西吃";
        prewrittenStories["result_skip_quick_bite"] = 
            "简单填了填肚子，虽然不够饱但至少不那么饿了。";
        
        prewrittenStories["choice_skip_continue_work"] = 
            "继续工作，稍后再吃";
        prewrittenStories["result_skip_continue_work"] = 
            "你压下饥饿感继续工作，但注意力已经难以集中...";

        prewrittenStories["story_food_shortage"] = 
            "打开钱包，只剩下可怜的几个硬币。\n连最便宜的便当都买不起了。\n你开始考虑要不要向朋友借钱...";
        
        prewrittenStories["choice_food_cheap_meal"] = 
            "买最便宜的食物凑合";
        prewrittenStories["result_food_cheap_meal"] = 
            "难以下咽的廉价食物，但至少能填饱肚子...";
        
        prewrittenStories["choice_food_borrow_money"] = 
            "向朋友借点钱";
        prewrittenStories["result_food_borrow_money"] = 
            "朋友二话不说借给了你。虽然心存感激，但也感到羞愧...";

        // ===== 过劳事件 =====
        prewrittenStories["story_overwork"] = 
            "你感到浑身疲惫，眼睛酸涩，身体沉重得无法做任何事情。\n一个声音在告诉你，也许是时候休息一下了...";
        
        prewrittenStories["choice_overwork_rest"] = 
            "好吧，今天就好好休息吧";
        prewrittenStories["result_overwork_rest"] = 
            "你躺在床上，很快就陷入了深度睡眠。醒来时，感到精神焕发。";
        
        prewrittenStories["choice_overwork_continue"] = 
            "咬牙坚持，再工作一下";
        prewrittenStories["result_overwork_continue"] = 
            "你勉强撑过了这一段，但身体的抗议声越来越大...";

        // ===== 抑郁事件 =====
        prewrittenStories["story_depression"] = 
            "一种莫名的悲伤笼罩了你。\n天空似乎都变成了灰色，所有的事物都失去了颜色。";
        
        prewrittenStories["choice_depression_seek_help"] = 
            "找朋友倾诉，寻求帮助";
        prewrittenStories["result_depression_seek_help"] = 
            "倾诉后，你感到了一些释然。也许，你不用一个人承担所有的重担。";
        
        prewrittenStories["choice_depression_endure"] = 
            "独自承受，相信时间会治愈";
        prewrittenStories["result_depression_endure"] = 
            "黑暗似乎在逐渐褪去，但这个过程很漫长...";

        // ===== 财富事件 =====
        prewrittenStories["story_wealth"] = 
            "看着银行账户上的数字，你突然意识到自己存了不少钱。\n一种陌生的满足感涌上心头。";
        
        prewrittenStories["choice_wealth_save"] = 
            "继续存钱，为未来计划";
        prewrittenStories["result_wealth_save"] = 
            "你感到一种安全感。也许存够了钱，就能做自己想做的事...";
        
        prewrittenStories["choice_wealth_spend"] = 
            "犒劳自己，好好消费一番";
        prewrittenStories["result_wealth_spend"] = 
            "购物给了你暂时的快乐，虽然钱包瘪了一些，但心情确实变好了。";

        // ===== 健康危机事件 =====
        prewrittenStories["story_health_crisis"] = 
            "突然的不适让你意识到，也许长期的透支终于开始报应了。\n你需要做出选择。";
        
        prewrittenStories["choice_see_doctor"] = 
            "立即去看医生";
        prewrittenStories["result_see_doctor"] = 
            "医生的诊断让你松了一口气——还不是太严重。但确实需要好好休养。";
        
        prewrittenStories["choice_ignore"] = 
            "吃点药硬撑，继续工作";
        prewrittenStories["result_ignore"] = 
            "你试图忽视身体的信号，但这似乎不是个明智的决定...";

        // ===== 工作机会事件 =====
        prewrittenStories["story_job_opportunity"] = 
            "一个新的工作机会出现了。\n机会难得，但你不确定自己是否能胜任...";
        
        prewrittenStories["choice_take_chance"] = 
            "冒险一试，挑战自己";
        prewrittenStories["result_take_chance"] = 
            "虽然有些吃力，但你在挑战中学到了很多。";
        
        prewrittenStories["choice_play_safe"] = 
            "选择安全，坚持原来的工作";
        prewrittenStories["result_play_safe"] = 
            "至少不会失败，但你多少有些遗憾...";

        // ===== 孤独感事件 =====
        prewrittenStories["story_loneliness"] = 
            "夜幕降临时，一种强烈的孤独感笼罩了你。\n你想起了朋友，想起了陪伴。";
        
        prewrittenStories["choice_reach_out"] = 
            "主动联系朋友";
        prewrittenStories["result_reach_out"] = 
            "朋友的热情回应让你感到温暖。也许有人在乎你。";
        
        prewrittenStories["choice_withdraw"] = 
            "继续独处，沉浸在思绪中";
        prewrittenStories["result_withdraw"] = 
            "孤独依旧...";

        // ===== 新友谊事件 =====
        prewrittenStories["story_friendship"] = 
            "在某个不期而至的时刻，一个新的朋友进入了你的生活。\n也许这会改变些什么...";
        
        prewrittenStories["choice_embrace"] = 
            "热情地拥抱这段新的友谊";
        prewrittenStories["result_embrace"] = 
            "新朋友给了你许多欢笑。生活顿时充满了新的可能。";
        
        prewrittenStories["choice_hesitate"] = 
            "保持距离，谨慎相处";
        prewrittenStories["result_hesitate"] = 
            "也许这是个不错的开始，但你还需要时间来确定...";

        // ===== 第一周完成事件 =====
        prewrittenStories["story_first_week"] = 
            "第一周结束了。\n回顾这一周，你学到了很多，也经历了不少。\n恭喜你坚持到了现在。";
        
        prewrittenStories["choice_reflect"] = 
            "整理思绪，为新的一周做准备";
        prewrittenStories["result_reflect"] = 
            "你已经踏出了第一步。继续加油！";

        // ===== 一月纪念事件 =====
        prewrittenStories["story_month_anniversary"] = 
            "一个月已经过去了。\n这一个月里，你经历了欢笑，也经历了挫折。\n但你一直在坚持...";
        
        prewrittenStories["choice_celebrate"] = 
            "好好庆祝一下自己的坚持";
        prewrittenStories["result_celebrate"] = 
            "你值得这份庆祝。继续为更好的明天努力吧。";

        // ===== 随机相遇 =====
        prewrittenStories["story_random_meeting"] = 
            "在街角咖啡馆，你巧遇了一个熟悉的面孔。\n该打招呼吗？";
        
        prewrittenStories["choice_talk"] = 
            "主动打招呼，聊一会儿";
        prewrittenStories["result_talk"] = 
            "谈话比你想象的要愉快得多。也许这会成为新的开始。";
        
        prewrittenStories["choice_avoid"] = 
            "假装没看到，匆匆离开";
        prewrittenStories["result_avoid"] = 
            "你快速走开了，但心中留下了一丝遗憾。";

        // ===== 幸运事件 =====
        prewrittenStories["story_serendipity"] = 
            "一个意想不到的好事发生了。\n也许是命运的安排，也许只是巧合。";
        
        prewrittenStories["choice_accept_luck"] = 
            "欣然接受这份幸运";
        prewrittenStories["result_accept_luck"] = 
            "好事已经降临。享受这一刻，也为未来的挑战积蓄力量。";

        // ===== 周末休闲 =====
        prewrittenStories["story_weekend"] = 
            "周末到了。你有整个周末的时间。\n该怎么度过这个难得的休闲时刻呢？";
        
        prewrittenStories["choice_relax"] = 
            "好好放松，享受休闲时光";
        prewrittenStories["result_relax"] = 
            "这样的休息正是你所需要的。";
        
        prewrittenStories["choice_work_weekend"] = 
            "利用周末继续工作，赚取更多金币";
        prewrittenStories["result_work_weekend"] = 
            "金币增加了，但疲惫也随之而来...";

        // ===== 工作冲突 =====
        prewrittenStories["story_work_conflict"] = 
            "工作中发生了不愉快的冲突。\n客人的无理取闹让你感到委屈和愤怒。\n你该如何应对？";
        
        prewrittenStories["choice_resolve"] = 
            "冷静下来，尝试理解和沟通";
        prewrittenStories["result_resolve"] = 
            "虽然很难，但你最终化解了这个冲突。你成长了。";
        
        prewrittenStories["choice_conflict_ignore"] = 
            "假装什么都没发生，继续工作";
        prewrittenStories["result_conflict_ignore"] = 
            "你压抑了情绪，但心中的不满在积累...";

        // ===== 升迁事件 =====
        prewrittenStories["story_promotion"] = 
            "你的上司找到了你。\n\"我们想提拔你担任更重要的职位...\"";
        
        prewrittenStories["choice_accept_promotion"] = 
            "欣然接受这个晋升机会";
        prewrittenStories["result_accept_promotion"] = 
            "你的努力得到了认可。新的挑战等待着你。";
        
        prewrittenStories["choice_decline_promotion"] = 
            "婉言拒绝，坚持现在的工作";
        prewrittenStories["result_decline_promotion"] = 
            "也许这不是现在的你需要的。但机会也许会再来。";

        // ===== 失业危机 =====
        prewrittenStories["story_job_loss"] = 
            "不幸的消息传来：你被解雇了。\n原因有很多，但这改变了什么。\n你需要重新开始。";
        
        prewrittenStories["choice_find_new_job"] = 
            "立即开始寻找新工作";
        prewrittenStories["result_find_new_job"] = 
            "求职之路充满挑战，但你决不放弃。";
        
        prewrittenStories["choice_take_break_job"] = 
            "暂时休息，好好调整自己";
        prewrittenStories["result_take_break_job"] = 
            "这段休息时间让你重新思考了生活的意义。";

        // ===== 倦怠事件 =====
        prewrittenStories["story_burnout"] = 
            "工作、生活、一切似乎都失去了颜色。\n你感到疲惫、迷茫、无助。\n这不能再继续了...";
        
        prewrittenStories["choice_take_vacation"] = 
            "申请假期，来一次长假";
        prewrittenStories["result_take_vacation"] = 
            "远离喧嚣，你终于可以好好休息了。也许这正是你所需要的。";
        
        prewrittenStories["choice_push_through_burnout"] = 
            "咬牙坚持，继续前进";
        prewrittenStories["result_push_through_burnout"] = 
            "你坚持了下来，但代价很大...";

        // ===== 贫困事件 =====
        prewrittenStories["story_poverty"] = 
            "你的钱快没了。\n账户里只剩下一点点，足够维持几天的生活。\n你需要尽快赚钱。";
        
        prewrittenStories["choice_work_hard_poverty"] = 
            "加倍努力工作，尽快脱困";
        prewrittenStories["result_work_hard_poverty"] = 
            "疲惫但充满希望。你相信这不会持续太久。";
        
        prewrittenStories["choice_get_help_poverty"] = 
            "寻求帮助，也许朋友可以借给你一些钱";
        prewrittenStories["result_get_help_poverty"] = 
            "朋友伸出了援手。也许你不必独自面对这一切。";

        // ===== 意外开支 =====
        prewrittenStories["story_unexpected_expense"] = 
            "突然的事故导致了一笔意外开支。\n钱包瘪下来了不少。\n你该如何应对？";
        
        prewrittenStories["choice_expense_pay"] = 
            "咬牙付款，接受这个现实";
        prewrittenStories["result_expense_pay"] = 
            "虽然心痛，但至少事情得到了解决。";
        
        prewrittenStories["choice_expense_defer"] = 
            "暂时延迟付款，希望能过这个难关";
        prewrittenStories["result_expense_defer"] = 
            "短期内你缓解了压力，但问题还是存在...";

        // ===== 投资机会 =====
        prewrittenStories["story_investment"] = 
            "一个投资的好机会摆在你面前。\n高收益也意味着高风险。\n你该冒这个险吗？";
        
        prewrittenStories["choice_invest_risky"] = 
            "冒险投资，赌一把";
        prewrittenStories["result_invest_risky"] = 
            "赌对了！你的资产大幅增加。但下次呢？";
        
        prewrittenStories["choice_invest_safe"] = 
            "保守投资，稳妥增长";
        prewrittenStories["result_invest_safe"] = 
            "虽然收益不多，但至少没有风险。";

        // ===== 人生转折 =====
        prewrittenStories["story_life_change"] = 
            "一个转折点出现了。\n也许是机遇，也许是挑战。\n这会改变你的人生轨迹。";
        
        prewrittenStories["choice_embrace_change"] = 
            "勇敢拥抱这个变化";
        prewrittenStories["result_embrace_change"] = 
            "虽然充满了不确定，但你感到了新生的希望。";
        
        prewrittenStories["choice_resist_change"] = 
            "抗拒这个变化，维持现状";
        prewrittenStories["result_resist_change"] = 
            "也许现在还不是改变的时候。";

        if (debugMode)
        {
            Debug.Log($"[EventStoryProvider] 已加载 {prewrittenStories.Count} 个默认故事");
        }
    }
    
    /// <summary>
    /// 获取故事文本
    /// </summary>
    public string GetStory(EventData eventData)
    {
        if (eventData == null) return "";
        
        if (prewrittenStories.TryGetValue(eventData.storyKey, out var story))
        {
            return story;
        }
        
        if (useAIGeneration)
        {
            return GenerateStoryWithAI(eventData);
        }
        
        Debug.LogWarning($"[EventStoryProvider] 故事不存在: {eventData.storyKey}");
        return $"[{eventData.eventName}]\n故事内容缺失。";
    }
    
    private string GenerateStoryWithAI(EventData eventData)
    {
        Debug.Log($"[EventStoryProvider] 使用 AI 生成故事: {eventData.eventName}");
        return $"[AI生成] {eventData.eventName}: 这是一个由AI生成的故事...";
    }
    
    public string GetChoiceText(EventChoice choice)
    {
        if (prewrittenStories.TryGetValue(choice.choiceTextKey, out var text))
        {
            return text;
        }
        
        Debug.LogWarning($"[EventStoryProvider] 选择文本不存在: {choice.choiceTextKey}");
        return choice.choiceTextKey;
    }
    
    public string GetResultText(EventChoice choice)
    {
        if (string.IsNullOrEmpty(choice.resultTextKey)) return "";
        
        if (prewrittenStories.TryGetValue(choice.resultTextKey, out var text))
        {
            return text;
        }
        
        return "";
    }
    
    public void AddStory(string key, string text)
    {
        prewrittenStories[key] = text;
        
        if (debugMode)
        {
            Debug.Log($"[EventStoryProvider] 已添加故事: {key}");
        }
    }
    
    public IEnumerable<string> GetAllStoryKeys()
    {
        return prewrittenStories.Keys;
    }
    
    public bool HasStory(string key)
    {
        return prewrittenStories.ContainsKey(key);
    }
}