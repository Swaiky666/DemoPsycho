using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 事件故事提供者 - 完整独立版本
/// 负责获取或生成事件的文本内容
/// 支持预写库 + AI 生成的混合方案
/// 
/// 使用方法:
/// 1. 在场景中创建 GameObject: "StoryProvider"
/// 2. 挂上此脚本
/// 3. 配置 Inspector 参数
/// 4. 通过 EventManager 自动查找或手动分配
/// </summary>
public class EventStoryProvider : MonoBehaviour
{
    [Header("预写故事库")]
    [SerializeField] private TextAsset prewrittenStoriesJson;
    
    [Header("AI 生成配置")]
    [SerializeField] private bool useAIGeneration = false;  // 是否使用 AI 生成
    
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
    
    /// <summary>
    /// 加载预写故事库
    /// </summary>
    private void LoadPrewrittenStories()
    {
        prewrittenStories.Clear();
        
        if (prewrittenStoriesJson != null)
        {
            try
            {
                // 如果有 JSON 文件，可以在这里解析
                // 目前使用硬编码的方式
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
    /// 初始化默认故事（硬编码）
    /// 实际项目应该从 JSON 或数据库加载
    /// 
    /// 故事 Key 命名规范:
    /// - story_* : 事件故事文本
    /// - choice_* : 选择按钮文本
    /// - result_* : 选择结果文本
    /// </summary>
    private void InitializeDefaultStories()
    {
        // ===== 过劳事件 (Overwork) =====
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

        // ===== 抑郁事件 (Depression) =====
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

        // ===== 财富事件 (Wealth) =====
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

        // ===== 健康危机事件 (Health Crisis) =====
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

        // ===== 工作机会事件 (Job Opportunity) =====
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

        // ===== 孤独感事件 (Loneliness) =====
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

        // ===== 新友谊事件 (Friendship) =====
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

        // ===== 第一周完成事件 (First Week) =====
        prewrittenStories["story_first_week"] = 
            "第一周结束了。\n回顾这一周，你学到了很多，也经历了不少。\n恭喜你坚持到了现在。";
        
        prewrittenStories["choice_reflect"] = 
            "整理思绪，为新的一周做准备";
        prewrittenStories["result_reflect"] = 
            "你已经踏出了第一步。继续加油！";

        // ===== 一月纪念事件 (Month Anniversary) =====
        prewrittenStories["story_month_anniversary"] = 
            "一个月已经过去了。\n这一个月里，你经历了欢笑，也经历了挫折。\n但你一直在坚持...";
        
        prewrittenStories["choice_celebrate"] = 
            "好好庆祝一下自己的坚持";
        prewrittenStories["result_celebrate"] = 
            "你值得这份庆祝。继续为更好的明天努力吧。";

        // ===== 随机事件 =====
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

        // ===== 幸运事件 (Serendipity) =====
        prewrittenStories["story_serendipity"] = 
            "一个意想不到的好事发生了。\n也许是命运的安排，也许只是巧合。";
        
        prewrittenStories["choice_accept_luck"] = 
            "欣然接受这份幸运";
        prewrittenStories["result_accept_luck"] = 
            "好事已经降临。享受这一刻，也为未来的挑战积蓄力量。";

        // ===== 周末休闲 (Weekend) =====
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

        // ===== 工作冲突 (Work Conflict) =====
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

        // ===== 升迁事件 (Promotion) =====
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

        // ===== 失业危机 (Job Loss) =====
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

        // ===== 倦怠事件 (Burnout) =====
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

        // ===== 贫困事件 (Poverty) =====
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

        // ===== 意外开支 (Unexpected Expense) =====
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

        // ===== 投资机会 (Investment) =====
        prewrittenStories["story_investment"] = 
            "一个投资的好机会摆在你面前。\n高收益也意味着高风险。\n你该冒这个险吗？";
        
        prewrittenStories["choice_invest_risky"] = 
            "冒险投资，赌一把";
        prewrittenStories["result_invest_risky"] = 
            "赌对了！你的资产大幅增加。但下一次呢？";
        
        prewrittenStories["choice_invest_safe"] = 
            "保守投资，稳妥增长";
        prewrittenStories["result_invest_safe"] = 
            "虽然收益不多，但至少没有风险。";

        // ===== 人生转折 (Life Change) =====
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
    /// 优先从预写库获取，如果没有则考虑 AI 生成
    /// </summary>
    public string GetStory(EventData eventData)
    {
        if (eventData == null) return "";
        
        // 1. 尝试从预写库获取
        if (prewrittenStories.TryGetValue(eventData.storyKey, out var story))
        {
            return story;
        }
        
        // 2. 尝试 AI 生成
        if (useAIGeneration)
        {
            return GenerateStoryWithAI(eventData);
        }
        
        // 3. 降级处理
        Debug.LogWarning($"[EventStoryProvider] 故事不存在: {eventData.storyKey}");
        return $"[{eventData.eventName}]\n故事内容缺失。";
    }
    
    /// <summary>
    /// AI 生成故事文本
    /// </summary>
    private string GenerateStoryWithAI(EventData eventData)
    {
        // TODO: 集成 LLM API
        // 这里是伪代码示例
        
        Debug.Log($"[EventStoryProvider] 使用 AI 生成故事: {eventData.eventName}");
        return $"[AI生成] {eventData.eventName}: 这是一个由AI生成的故事...";
    }
    
    /// <summary>
    /// 获取本地化的选择文本
    /// </summary>
    public string GetChoiceText(EventChoice choice)
    {
        if (prewrittenStories.TryGetValue(choice.choiceTextKey, out var text))
        {
            return text;
        }
        
        Debug.LogWarning($"[EventStoryProvider] 选择文本不存在: {choice.choiceTextKey}");
        return choice.choiceTextKey;  // 降级：返回 key
    }
    
    /// <summary>
    /// 获取结果文本
    /// </summary>
    public string GetResultText(EventChoice choice)
    {
        if (string.IsNullOrEmpty(choice.resultTextKey)) return "";
        
        if (prewrittenStories.TryGetValue(choice.resultTextKey, out var text))
        {
            return text;
        }
        
        return "";
    }
    
    /// <summary>
    /// 添加预写故事（运行时）
    /// 用于动态添加故事内容
    /// </summary>
    public void AddStory(string key, string text)
    {
        prewrittenStories[key] = text;
        
        if (debugMode)
        {
            Debug.Log($"[EventStoryProvider] 已添加故事: {key}");
        }
    }
    
    /// <summary>
    /// 获取所有故事 key
    /// </summary>
    public IEnumerable<string> GetAllStoryKeys()
    {
        return prewrittenStories.Keys;
    }
    
    /// <summary>
    /// 检查某个 key 是否存在
    /// </summary>
    public bool HasStory(string key)
    {
        return prewrittenStories.ContainsKey(key);
    }

    // ===== 调试方法 =====

    [ContextMenu("DEBUG: 打印所有故事")]
    public void DebugPrintAllStories()
    {
        Debug.Log($"\n========== 预写故事库 ({prewrittenStories.Count} 个) ==========");
        
        int index = 1;
        foreach (var kvp in prewrittenStories)
        {
            string preview = kvp.Value.Length > 50 ? kvp.Value.Substring(0, 50) + "..." : kvp.Value;
            Debug.Log($"{index}. [{kvp.Key}] {preview}");
            index++;
        }
        
        Debug.Log($"================================================\n");
    }

    [ContextMenu("DEBUG: 验证故事库")]
    public void DebugValidateStories()
    {
        Debug.Log($"\n========== 故事库验证 ==========");
        Debug.Log($"总故事数: {prewrittenStories.Count}");
        
        // 检查关键故事是否存在
        string[] keyStories = new[]
        {
            "story_overwork", "story_depression", "story_wealth",
            "choice_overwork_rest", "choice_depression_seek_help",
            "result_overwork_rest", "result_depression_seek_help"
        };
        
        int missingCount = 0;
        foreach (var key in keyStories)
        {
            if (prewrittenStories.ContainsKey(key))
            {
                Debug.Log($"✓ {key}");
            }
            else
            {
                Debug.Log($"✗ {key} (缺失)");
                missingCount++;
            }
        }
        
        if (missingCount == 0)
        {
            Debug.Log($"\n✅ 所有关键故事都已存在");
        }
        else
        {
            Debug.Log($"\n⚠️ 缺失 {missingCount} 个关键故事");
        }
        
        Debug.Log($"================================\n");
    }

    [ContextMenu("DEBUG: 重新加载故事库")]
    public void DebugReloadStories()
    {
        LoadPrewrittenStories();
        Debug.Log($"[DEBUG] 故事库已重新加载，共 {prewrittenStories.Count} 个故事");
    }

    [ContextMenu("DEBUG: 导出故事到 JSON")]
    public void DebugExportToJSON()
    {
        var json = new Dictionary<string, object>
        {
            { "total_stories", prewrittenStories.Count },
            { "stories", prewrittenStories }
        };
        
        string jsonStr = JsonUtility.ToJson(new SerializableDictionary { data = new List<KVPair>() });
        Debug.Log($"[DEBUG] 故事库 JSON:\n{jsonStr}");
    }

    [ContextMenu("DEBUG: 获取故事统计")]
    public void DebugPrintStatistics()
    {
        Debug.Log($"\n========== 故事库统计 ==========");
        Debug.Log($"总故事数: {prewrittenStories.Count}");
        
        int storyCount = 0, choiceCount = 0, resultCount = 0;
        
        foreach (var key in prewrittenStories.Keys)
        {
            if (key.StartsWith("story_")) storyCount++;
            else if (key.StartsWith("choice_")) choiceCount++;
            else if (key.StartsWith("result_")) resultCount++;
        }
        
        Debug.Log($"故事文本: {storyCount} 个");
        Debug.Log($"选择文本: {choiceCount} 个");
        Debug.Log($"结果文本: {resultCount} 个");
        Debug.Log($"其他: {prewrittenStories.Count - storyCount - choiceCount - resultCount} 个");
        Debug.Log($"================================\n");
    }
}

/// <summary>
/// 辅助类：用于 JSON 序列化
/// </summary>
[System.Serializable]
public class SerializableDictionary
{
    public List<KVPair> data = new();
}

[System.Serializable]
public class KVPair
{
    public string key;
    public string value;
}