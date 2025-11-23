using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 故事弧系统 - 管理5条主故事线
/// 追踪每条线的进度、阶段、关键指标
/// 
/// 故事线：
/// 1. 职业发展 - 工作机会→升迁→转折→收获
/// 2. 心理健康 - 稳定→波动→危机→恢复
/// 3. 关系 - 孤独→连接→深化→新关系
/// 4. 财务 - 贫困→稳定→舒适→新疑问
/// 5. 自我认知 - 迷茫→反思→认知→前路
/// </summary>
public class StoryArcSystem : MonoBehaviour
{
    [Header("系统配置")]
    [SerializeField] private bool debugMode = true;
    
    // 5条故事线的进度数据
    private CareerStoryLine careerStory;
    private MentalHealthStoryLine mentalHealthStory;
    private RelationshipStoryLine relationshipStory;
    private FinancialStoryLine financialStory;
    private SelfAwarenessStoryLine selfAwarenessStory;
    
    // Week 3是转折点
    [SerializeField] private int turningPointWeek = 3;
    
    // 事件
    public event Action<StoryPhase> OnStoryPhaseChanged;
    public event Action<string> OnMilestoneReached;
    
    public static StoryArcSystem Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        InitializeStoryLines();
        Debug.Log("[StoryArcSystem] ✅ 故事系统初始化完成");
    }

    /// <summary>
    /// 初始化所有故事线
    /// </summary>
    private void InitializeStoryLines()
    {
        careerStory = new CareerStoryLine();
        mentalHealthStory = new MentalHealthStoryLine();
        relationshipStory = new RelationshipStoryLine();
        financialStory = new FinancialStoryLine();
        selfAwarenessStory = new SelfAwarenessStoryLine();
    }

    /// <summary>
    /// 每天调用一次，更新故事线进度
    /// </summary>
    public void UpdateDailyProgress(int dayOfMonth)
    {
        int currentWeek = (dayOfMonth - 1) / 7 + 1;
        
        // 根据当前周数更新每条线
        UpdateCareerStory(currentWeek, dayOfMonth);
        UpdateMentalHealthStory(currentWeek, dayOfMonth);
        UpdateRelationshipStory(currentWeek, dayOfMonth);
        UpdateFinancialStory(currentWeek, dayOfMonth);
        UpdateSelfAwarenessStory(currentWeek, dayOfMonth);
        
        // 检查Week 3的转折点
        if (currentWeek == turningPointWeek && dayOfMonth % 7 == 0)
        {
            CheckTurningPointMilestone();
        }
    }

    // ===== 职业发展线 =====
    
    private void UpdateCareerStory(int week, int dayOfMonth)
    {
        if (week == 1 && dayOfMonth == 1)
        {
            careerStory.currentPhase = StoryPhase.Exploration;
            OnStoryPhaseChanged?.Invoke(StoryPhase.Exploration);
            OnMilestoneReached?.Invoke("career_exploration_begins");
            Debug.Log("[StoryArcSystem] 🎯 职业线：探索阶段开始");
        }
        else if (week == 2)
        {
            careerStory.currentPhase = StoryPhase.Development;
            OnMilestoneReached?.Invoke("career_development_begins");
            Debug.Log("[StoryArcSystem] 🎯 职业线：发展阶段");
        }
        else if (week == turningPointWeek)
        {
            careerStory.currentPhase = StoryPhase.CriticalTurning;
            OnMilestoneReached?.Invoke("career_turning_point");
            Debug.Log("[StoryArcSystem] ⚡ 职业线：转折点！");
        }
        else if (week == 4)
        {
            careerStory.currentPhase = StoryPhase.Harvest;
            OnMilestoneReached?.Invoke("career_harvest");
            Debug.Log("[StoryArcSystem] 🎯 职业线：收获阶段");
        }
        
        // 更新职业线的核心指标
        careerStory.workSkillProgression += CalculateSkillGain();
        careerStory.jobSatisfactionTrend = EvaluateJobSatisfaction();
    }

    private float CalculateSkillGain()
    {
        // 基础技能提升速度
        return UnityEngine.Random.Range(0.5f, 1.5f);
    }

    private float EvaluateJobSatisfaction()
    {
        // 根据工作状态评估满意度（-10 ~ +10）
        return UnityEngine.Random.Range(-2f, 2f);
    }

    // ===== 心理健康线 =====
    
    private void UpdateMentalHealthStory(int week, int dayOfMonth)
    {
        if (week == 1 && dayOfMonth == 1)
        {
            mentalHealthStory.currentPhase = StoryPhase.Stable;
            OnMilestoneReached?.Invoke("mental_health_baseline");
            Debug.Log("[StoryArcSystem] 🧠 心理线：建立基线");
        }
        else if (week == 2)
        {
            mentalHealthStory.currentPhase = StoryPhase.Fluctuation;
            OnMilestoneReached?.Invoke("mental_health_fluctuation_begins");
            Debug.Log("[StoryArcSystem] 🧠 心理线：波动阶段");
        }
        else if (week == turningPointWeek)
        {
            mentalHealthStory.currentPhase = StoryPhase.CriticalTurning;
            OnMilestoneReached?.Invoke("mental_health_crisis_or_recovery");
            Debug.Log("[StoryArcSystem] ⚡ 心理线：危机/恢复的关键时刻！");
        }
        else if (week == 4)
        {
            mentalHealthStory.currentPhase = StoryPhase.NewBalance;
            OnMilestoneReached?.Invoke("mental_health_new_balance");
            Debug.Log("[StoryArcSystem] 🧠 心理线：新平衡建立");
        }
    }

    // ===== 关系线 =====
    
    private void UpdateRelationshipStory(int week, int dayOfMonth)
    {
        if (week == 1 && dayOfMonth == 1)
        {
            relationshipStory.currentPhase = StoryPhase.Isolation;
            OnMilestoneReached?.Invoke("relationship_starts_alone");
            Debug.Log("[StoryArcSystem] 💔 关系线：孤独开始");
        }
        else if (week == 2)
        {
            relationshipStory.currentPhase = StoryPhase.Connection;
            OnMilestoneReached?.Invoke("relationship_first_connection");
            Debug.Log("[StoryArcSystem] 💔 关系线：第一次连接");
        }
        else if (week == turningPointWeek)
        {
            relationshipStory.currentPhase = StoryPhase.CriticalTurning;
            OnMilestoneReached?.Invoke("relationship_deepening_or_breaking");
            Debug.Log("[StoryArcSystem] ⚡ 关系线：深化/破裂的转折！");
        }
        else if (week == 4)
        {
            relationshipStory.currentPhase = StoryPhase.NewRelationship;
            OnMilestoneReached?.Invoke("relationship_new_bonds");
            Debug.Log("[StoryArcSystem] 💔 关系线：新关系建立");
        }
    }

    // ===== 财务线 =====
    
    private void UpdateFinancialStory(int week, int dayOfMonth)
    {
        if (week == 1 && dayOfMonth == 1)
        {
            financialStory.currentPhase = StoryPhase.Poverty;
            OnMilestoneReached?.Invoke("financial_tight_budget");
            Debug.Log("[StoryArcSystem] 💰 财务线：紧张预算");
        }
        else if (week == 2)
        {
            financialStory.currentPhase = StoryPhase.Stability;
            OnMilestoneReached?.Invoke("financial_stabilizing");
            Debug.Log("[StoryArcSystem] 💰 财务线：稳定阶段");
        }
        else if (week == turningPointWeek)
        {
            financialStory.currentPhase = StoryPhase.CriticalTurning;
            OnMilestoneReached?.Invoke("financial_opportunity_or_crisis");
            Debug.Log("[StoryArcSystem] ⚡ 财务线：机遇/危机的转折！");
        }
        else if (week == 4)
        {
            financialStory.currentPhase = StoryPhase.Comfort;
            OnMilestoneReached?.Invoke("financial_comfortable");
            Debug.Log("[StoryArcSystem] 💰 财务线：舒适阶段");
        }
    }

    // ===== 自我认知线 =====
    
    private void UpdateSelfAwarenessStory(int week, int dayOfMonth)
    {
        if (week == 1 && dayOfMonth == 1)
        {
            selfAwarenessStory.currentPhase = StoryPhase.Confusion;
            OnMilestoneReached?.Invoke("self_awareness_confusion");
            Debug.Log("[StoryArcSystem] 🔍 自我线：迷茫开始");
        }
        // Day 7是反思日
        else if (dayOfMonth == 7)
        {
            selfAwarenessStory.currentPhase = StoryPhase.Reflection;
            OnMilestoneReached?.Invoke("self_awareness_first_reflection");
            Debug.Log("[StoryArcSystem] 🔍 自我线：第一次反思");
        }
        else if (week == turningPointWeek)
        {
            selfAwarenessStory.currentPhase = StoryPhase.Recognition;
            OnMilestoneReached?.Invoke("self_awareness_recognition");
            Debug.Log("[StoryArcSystem] ⚡ 自我线：自我认知突破！");
        }
        // Day 28是最终反思
        else if (dayOfMonth == 28)
        {
            selfAwarenessStory.currentPhase = StoryPhase.Future;
            OnMilestoneReached?.Invoke("self_awareness_final_reflection");
            Debug.Log("[StoryArcSystem] 🔍 自我线：最终反思与前路");
        }
    }

    /// <summary>
    /// 检查Week 3的转折点 - 所有故事线的高潮
    /// </summary>
    private void CheckTurningPointMilestone()
    {
        OnMilestoneReached?.Invoke("story_week3_all_lines_climax");
        Debug.Log("\n[StoryArcSystem] ⚡⚡⚡ 第3周：所有故事线的转折点！⚡⚡⚡\n");
    }

    /// <summary>
    /// 获取当前故事阶段
    /// </summary>
    public StoryPhase GetCareerPhase() => careerStory.currentPhase;
    public StoryPhase GetMentalHealthPhase() => mentalHealthStory.currentPhase;
    public StoryPhase GetRelationshipPhase() => relationshipStory.currentPhase;
    public StoryPhase GetFinancialPhase() => financialStory.currentPhase;
    public StoryPhase GetSelfAwarenessPhase() => selfAwarenessStory.currentPhase;

    /// <summary>
    /// 打印故事线状态
    /// </summary>
    [ContextMenu("DEBUG: 打印故事线状态")]
    public void DebugPrintStoryStatus()
    {
        Debug.Log("\n========== 故事线状态 ==========");
        Debug.Log($"职业: {careerStory.currentPhase}");
        Debug.Log($"心理: {mentalHealthStory.currentPhase}");
        Debug.Log($"关系: {relationshipStory.currentPhase}");
        Debug.Log($"财务: {financialStory.currentPhase}");
        Debug.Log($"自我: {selfAwarenessStory.currentPhase}");
        Debug.Log("================================\n");
    }
}

/// <summary>
/// 故事阶段枚举
/// </summary>
public enum StoryPhase
{
    // 通用阶段
    Exploration,        // 探索
    Development,        // 发展
    Stable,             // 稳定
    Fluctuation,        // 波动
    CriticalTurning,    // 转折
    Harvest,            // 收获
    
    // 关系线特定
    Isolation,          // 孤独
    Connection,         // 连接
    Deepening,          // 深化
    Breaking,           // 破裂
    NewRelationship,    // 新关系
    
    // 财务线特定
    Poverty,            // 贫困
    Stability,          // 稳定
    Comfort,            // 舒适
    
    // 心理线特定
    NewBalance,         // 新平衡
    Crisis,             // 危机
    Recovery,           // 恢复
    
    // 自我线特定
    Confusion,          // 迷茫
    Reflection,         // 反思
    Recognition,        // 认知
    Future              // 前路
}

/// <summary>
/// 职业发展故事线
/// </summary>
[System.Serializable]
public class CareerStoryLine
{
    public StoryPhase currentPhase = StoryPhase.Exploration;
    public float workSkillProgression = 0f;      // 工作技能累积
    public float jobSatisfactionTrend = 0f;      // 工作满意度趋势
    public int jobChanges = 0;                    // 工作变化次数
    public bool gotPromoted = false;              // 是否升迁
}

/// <summary>
/// 心理健康故事线
/// </summary>
[System.Serializable]
public class MentalHealthStoryLine
{
    public StoryPhase currentPhase = StoryPhase.Stable;
    public float emotionalVolatility = 0f;       // 情绪波动程度
    public float resilience = 50f;               // 韧性（0-100）
    public int crisisEvents = 0;                 // 危机事件数
    public bool hasRecovered = false;            // 是否恢复
}

/// <summary>
/// 关系故事线
/// </summary>
[System.Serializable]
public class RelationshipStoryLine
{
    public StoryPhase currentPhase = StoryPhase.Isolation;
    public int connectionsMade = 0;              // 建立的连接数
    public float relationshipDepth = 0f;         // 关系深度（0-100）
    public bool hasBrokenRelationship = false;   // 是否破裂过关系
    public int activeRelationships = 0;          // 当前活跃关系数
}

/// <summary>
/// 财务故事线
/// </summary>
[System.Serializable]
public class FinancialStoryLine
{
    public StoryPhase currentPhase = StoryPhase.Poverty;
    public float wealthProgression = 0f;         // 财富进度（0-100）
    public float financialStress = 50f;          // 财务压力（0-100）
    public int investmentDecisions = 0;          // 投资决策数
    public bool experiencedCrisis = false;       // 是否经历过财务危机
}

/// <summary>
/// 自我认知故事线
/// </summary>
[System.Serializable]
public class SelfAwarenessStoryLine
{
    public StoryPhase currentPhase = StoryPhase.Confusion;
    public float selfUnderstanding = 0f;         // 自我理解程度（0-100）
    public float valueClarification = 0f;        // 价值观澄清（0-100）
    public int reflectionMoments = 0;            // 反思时刻数
    public string[] discoveredValues = new string[0]; // 发现的核心价值观
}