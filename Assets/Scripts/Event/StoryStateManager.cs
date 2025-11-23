using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 故事状态管理器
/// 追踪每条故事线的详细状态、分支选择、关键时刻等
/// 
/// 功能：
/// 1. 保存/加载故事线状态
/// 2. 记录重要的选择时刻
/// 3. 评估当前的故事状态（好/坏/中立）
/// 4. 生成故事进度报告
/// </summary>
public class StoryStateManager : MonoBehaviour
{
    [Header("系统引用")]
    [SerializeField] private GameFlagManager flagManager;
    [SerializeField] private AffectGameState gameState;
    
    [Header("调试模式")]
    [SerializeField] private bool debugMode = true;
    
    // 故事线的选择记录
    private StoryLineChoiceHistory careerChoices = new();
    private StoryLineChoiceHistory mentalHealthChoices = new();
    private StoryLineChoiceHistory relationshipChoices = new();
    private StoryLineChoiceHistory financialChoices = new();
    private StoryLineChoiceHistory selfAwarenessChoices = new();
    
    // 故事分支评估
    private Dictionary<string, StoryLineStatus> storyStatuses = new();
    
    public static StoryStateManager Instance { get; private set; }
    
    // 事件
    public event Action<string, StoryLineStatus> OnStoryLineStatusChanged;

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
        if (flagManager == null) flagManager = FindObjectOfType<GameFlagManager>();
        if (gameState == null) gameState = FindObjectOfType<AffectGameState>();
        
        InitializeStoryStatuses();
        Debug.Log("[StoryStateManager] ✅ 故事状态管理器已初始化");
    }

    /// <summary>
    /// 初始化故事线状态
    /// </summary>
    private void InitializeStoryStatuses()
    {
        storyStatuses["career"] = StoryLineStatus.Neutral;
        storyStatuses["mental_health"] = StoryLineStatus.Neutral;
        storyStatuses["relationship"] = StoryLineStatus.Neutral;
        storyStatuses["financial"] = StoryLineStatus.Neutral;
        storyStatuses["self_awareness"] = StoryLineStatus.Neutral;
    }

    /// <summary>
    /// 记录一个重要的故事选择
    /// </summary>
    public void RecordStoryChoice(string storyLine, string choiceId, string choiceText, Dictionary<string, float> effects)
    {
        var history = GetStoryChoiceHistory(storyLine);
        if (history == null) return;
        
        var choice = new StoryChoice
        {
            choiceId = choiceId,
            choiceText = choiceText,
            timestamp = Time.time,
            dayOccurred = GetCurrentDay(),
            effects = new Dictionary<string, float>(effects)
        };
        
        history.choices.Add(choice);
        
        // 根据选择更新故事线状态
        UpdateStoryLineStatusFromChoice(storyLine, choice);
        
        if (debugMode)
        {
            Debug.Log($"[StoryStateManager] 📝 记录选择: {storyLine} -> {choiceText}");
        }
    }

    /// <summary>
    /// 根据选择更新故事线状态
    /// </summary>
    private void UpdateStoryLineStatusFromChoice(string storyLine, StoryChoice choice)
    {
        if (!storyStatuses.ContainsKey(storyLine))
            return;
        
        // 计算选择的整体影响
        float valenceChange = 0;
        if (choice.effects.ContainsKey("V"))
        {
            valenceChange = choice.effects["V"];
        }
        
        // 根据Valence变化评估选择的好坏
        StoryLineStatus newStatus = StoryLineStatus.Neutral;
        
        if (valenceChange > 1)
            newStatus = StoryLineStatus.Positive;
        else if (valenceChange < -1)
            newStatus = StoryLineStatus.Negative;
        else
            newStatus = StoryLineStatus.Neutral;
        
        storyStatuses[storyLine] = newStatus;
        OnStoryLineStatusChanged?.Invoke(storyLine, newStatus);
        
        if (debugMode)
        {
            Debug.Log($"[StoryStateManager] 📊 {storyLine} 状态更新: {newStatus}");
        }
    }

    /// <summary>
    /// 获取故事线的选择历史
    /// </summary>
    private StoryLineChoiceHistory GetStoryChoiceHistory(string storyLine)
    {
        return storyLine switch
        {
            "career" => careerChoices,
            "mental_health" => mentalHealthChoices,
            "relationship" => relationshipChoices,
            "financial" => financialChoices,
            "self_awareness" => selfAwarenessChoices,
            _ => null
        };
    }

    /// <summary>
    /// 评估故事线的当前状态
    /// </summary>
    public StoryLineStatus GetStoryLineStatus(string storyLine)
    {
        if (storyStatuses.ContainsKey(storyLine))
            return storyStatuses[storyLine];
        return StoryLineStatus.Neutral;
    }

    /// <summary>
    /// 获取故事线的选择数
    /// </summary>
    public int GetChoiceCount(string storyLine)
    {
        var history = GetStoryChoiceHistory(storyLine);
        return history?.choices.Count ?? 0;
    }

    /// <summary>
    /// 获取故事线的所有选择
    /// </summary>
    public List<StoryChoice> GetAllChoices(string storyLine)
    {
        var history = GetStoryChoiceHistory(storyLine);
        return history?.choices ?? new List<StoryChoice>();
    }

    /// <summary>
    /// 检查玩家是否做过特定的故事选择
    /// </summary>
    public bool HasMadeChoice(string storyLine, string choiceId)
    {
        var history = GetStoryChoiceHistory(storyLine);
        if (history == null) return false;
        
        return history.choices.Exists(c => c.choiceId == choiceId);
    }

    /// <summary>
    /// 获取所有故事线的综合状态
    /// </summary>
    public OverallStoryStatus GetOverallStatus()
    {
        int positiveCount = 0;
        int negativeCount = 0;
        
        foreach (var status in storyStatuses.Values)
        {
            if (status == StoryLineStatus.Positive) positiveCount++;
            else if (status == StoryLineStatus.Negative) negativeCount++;
        }
        
        if (positiveCount >= 3)
            return OverallStoryStatus.VeryPositive;
        else if (positiveCount > negativeCount)
            return OverallStoryStatus.Positive;
        else if (negativeCount > positiveCount)
            return OverallStoryStatus.Negative;
        else
            return OverallStoryStatus.Balanced;
    }

    /// <summary>
    /// 生成故事进度报告
    /// </summary>
    public string GenerateStoryReport()
    {
        var report = new System.Text.StringBuilder();
        
        report.AppendLine("\n========== 故事进度报告 ==========\n");
        
        report.AppendLine("【故事线状态】");
        foreach (var kvp in storyStatuses)
        {
            report.AppendLine($"  {kvp.Key}: {kvp.Value}");
        }
        
        report.AppendLine($"\n【综合评估】{GetOverallStatus()}");
        
        report.AppendLine("\n【选择历史】");
        report.AppendLine($"  职业线: {GetChoiceCount("career")} 个选择");
        report.AppendLine($"  心理线: {GetChoiceCount("mental_health")} 个选择");
        report.AppendLine($"  关系线: {GetChoiceCount("relationship")} 个选择");
        report.AppendLine($"  财务线: {GetChoiceCount("financial")} 个选择");
        report.AppendLine($"  自我线: {GetChoiceCount("self_awareness")} 个选择");
        
        report.AppendLine("\n==================================\n");
        
        return report.ToString();
    }

    /// <summary>
    /// 计算故事线的积极程度（0-100）
    /// </summary>
    public float CalculateStoryLinePositivity(string storyLine)
    {
        var history = GetStoryChoiceHistory(storyLine);
        if (history == null || history.choices.Count == 0)
            return 50f;  // 中立
        
        float totalValence = 0;
        foreach (var choice in history.choices)
        {
            if (choice.effects.ContainsKey("V"))
            {
                totalValence += choice.effects["V"];
            }
        }
        
        float averageValence = totalValence / history.choices.Count;
        // 转换为 0-100 范围
        return Mathf.Clamp(50f + (averageValence * 5), 0, 100);
    }

    /// <summary>
    /// 获取故事线中最有影响力的选择
    /// </summary>
    public StoryChoice GetMostInfluentialChoice(string storyLine)
    {
        var history = GetStoryChoiceHistory(storyLine);
        if (history == null || history.choices.Count == 0)
            return null;
        
        StoryChoice mostInfluential = history.choices[0];
        float maxImpact = CalculateChoiceImpact(mostInfluential);
        
        foreach (var choice in history.choices)
        {
            float impact = CalculateChoiceImpact(choice);
            if (impact > maxImpact)
            {
                maxImpact = impact;
                mostInfluential = choice;
            }
        }
        
        return mostInfluential;
    }

    /// <summary>
    /// 计算选择的影响程度
    /// </summary>
    private float CalculateChoiceImpact(StoryChoice choice)
    {
        float impact = 0;
        foreach (var effect in choice.effects.Values)
        {
            impact += Mathf.Abs(effect);
        }
        return impact;
    }

    /// <summary>
    /// 获取当前游戏日期
    /// </summary>
    private int GetCurrentDay()
    {
        // 这里应该从TimeManager获取
        return 1;  // 示例
    }

    /// <summary>
    /// 导出故事线状态（用于存档）
    /// </summary>
    public StoryStateSaveData ExportStateData()
    {
        return new StoryStateSaveData
        {
            careerChoices = careerChoices.ExportData(),
            mentalHealthChoices = mentalHealthChoices.ExportData(),
            relationshipChoices = relationshipChoices.ExportData(),
            financialChoices = financialChoices.ExportData(),
            selfAwarenessChoices = selfAwarenessChoices.ExportData(),
            storyStatuses = new Dictionary<string, int>(
                new Dictionary<string, int>
                {
                    { "career", (int)storyStatuses["career"] },
                    { "mental_health", (int)storyStatuses["mental_health"] },
                    { "relationship", (int)storyStatuses["relationship"] },
                    { "financial", (int)storyStatuses["financial"] },
                    { "self_awareness", (int)storyStatuses["self_awareness"] }
                }
            )
        };
    }

    /// <summary>
    /// 导入故事线状态（用于读档）
    /// </summary>
    public void ImportStateData(StoryStateSaveData data)
    {
        careerChoices.ImportData(data.careerChoices);
        mentalHealthChoices.ImportData(data.mentalHealthChoices);
        relationshipChoices.ImportData(data.relationshipChoices);
        financialChoices.ImportData(data.financialChoices);
        selfAwarenessChoices.ImportData(data.selfAwarenessChoices);
        
        if (debugMode)
        {
            Debug.Log("[StoryStateManager] ✅ 故事线状态已导入");
        }
    }

    /// <summary>
    /// 打印故事报告
    /// </summary>
    [ContextMenu("DEBUG: 打印故事报告")]
    public void DebugPrintReport()
    {
        Debug.Log(GenerateStoryReport());
    }

    /// <summary>
    /// 打印详细的故事线状态
    /// </summary>
    [ContextMenu("DEBUG: 打印故事线状态")]
    public void DebugPrintStoryLineDetails()
    {
        Debug.Log("\n========== 故事线详细状态 ==========");
        
        string[] storyLines = { "career", "mental_health", "relationship", "financial", "self_awareness" };
        foreach (var line in storyLines)
        {
            var positivity = CalculateStoryLinePositivity(line);
            var mostInfluential = GetMostInfluentialChoice(line);
            
            Debug.Log($"\n【{line}】");
            Debug.Log($"  状态: {storyStatuses[line]}");
            Debug.Log($"  积极度: {positivity:F1}%");
            Debug.Log($"  选择数: {GetChoiceCount(line)}");
            if (mostInfluential != null)
            {
                Debug.Log($"  最有影响的选择: {mostInfluential.choiceText}");
            }
        }
        
        Debug.Log($"\n综合状态: {GetOverallStatus()}");
        Debug.Log("==================================\n");
    }
}

/// <summary>
/// 故事线选择历史
/// </summary>
[System.Serializable]
public class StoryLineChoiceHistory
{
    public List<StoryChoice> choices = new();
    
    public string ExportData()
    {
        return JsonUtility.ToJson(new ChoiceListWrapper { choices = choices });
    }
    
    public void ImportData(string json)
    {
        var wrapper = JsonUtility.FromJson<ChoiceListWrapper>(json);
        choices = wrapper.choices;
    }
    
    [System.Serializable]
    public class ChoiceListWrapper
    {
        public List<StoryChoice> choices;
    }
}

/// <summary>
/// 单个故事选择
/// </summary>
[System.Serializable]
public class StoryChoice
{
    public string choiceId;
    public string choiceText;
    public float timestamp;
    public int dayOccurred;
    public Dictionary<string, float> effects = new();
}

/// <summary>
/// 故事线状态枚举
/// </summary>
public enum StoryLineStatus
{
    VeryNegative = -2,
    Negative = -1,
    Neutral = 0,
    Positive = 1,
    VeryPositive = 2
}

/// <summary>
/// 整体故事状态
/// </summary>
public enum OverallStoryStatus
{
    VeryNegative,
    Negative,
    Balanced,
    Positive,
    VeryPositive
}

/// <summary>
/// 故事线状态保存数据
/// </summary>
[System.Serializable]
public class StoryStateSaveData
{
    public string careerChoices;
    public string mentalHealthChoices;
    public string relationshipChoices;
    public string financialChoices;
    public string selfAwarenessChoices;
    public Dictionary<string, int> storyStatuses;
}