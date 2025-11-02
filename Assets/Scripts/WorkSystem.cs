using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 工作数据
/// </summary>
[Serializable]
public class JobData
{
    public string jobId = "job_001";
    public string jobName = "送快递";
    public float timeRequired = 2f;           // 需要多少小时
    public float baseSalary = 50f;            // 基础工资
    public float healthCost = 10f;            // 健康消耗
    public float requiredSkill = 10f;         // 所需技能等级
    public float expGain = 3f;                // 经验获得
    public float vChange = -0.5f;             // 情绪变化 V
    public float aChange = 0.5f;              // 情绪变化 A
    public string timeSlot = "早上";          // 时间段
}

/// <summary>
/// 工作系统
/// 演示如何与 TimeManager 交互
/// </summary>
public class WorkSystem : MonoBehaviour
{
    [Header("参考")]
    [SerializeField] private TimeManager timeManager;
    
    // 直接引用你的 AffectGameState（改成你实际的类名）
    private AffectGameState gameState;
    
    void Awake()
    {
        if (gameState == null)
            gameState = FindObjectOfType<AffectGameState>();
    }

    [Header("工作数据")]
    private List<JobData> availableJobs = new();

    [Header("玩家工作状态")]
    [SerializeField] private float playerSkill = 15f;
    [SerializeField] private float playerExperience = 0f;

    void Start()
    {
        if (timeManager == null)
            timeManager = GetComponent<TimeManager>();
        
        InitializeJobs();
    }

    /// <summary>
    /// 初始化可用工作列表
    /// </summary>
    private void InitializeJobs()
    {
        availableJobs.Clear();
        availableJobs.Add(new JobData
        {
            jobId = "job_delivery",
            jobName = "送快递",
            timeRequired = 2f,
            baseSalary = 50f,
            healthCost = 10f,
            requiredSkill = 10f,
            expGain = 3f,
            vChange = -0.5f,
            aChange = 0.5f,
            timeSlot = "早上"
        });

        availableJobs.Add(new JobData
        {
            jobId = "job_restaurant",
            jobName = "餐厅端盘子",
            timeRequired = 3f,
            baseSalary = 40f,
            healthCost = 15f,
            requiredSkill = 8f,
            expGain = 2f,
            vChange = 0f,
            aChange = 1f,
            timeSlot = "中午"
        });

        availableJobs.Add(new JobData
        {
            jobId = "job_leaflet",
            jobName = "发传单",
            timeRequired = 2.5f,
            baseSalary = 35f,
            healthCost = 8f,
            requiredSkill = 5f,
            expGain = 1.5f,
            vChange = 0.5f,
            aChange = -0.5f,
            timeSlot = "下午"
        });

        availableJobs.Add(new JobData
        {
            jobId = "job_bar",
            jobName = "酒馆帮工",
            timeRequired = 1.5f,
            baseSalary = 80f,
            healthCost = 20f,
            requiredSkill = 25f,
            expGain = 5f,
            vChange = -1f,
            aChange = 1.5f,
            timeSlot = "晚上"
        });
    }

    /// <summary>
    /// 玩家选择工作（主要调用接口）
    /// </summary>
    public void DoWork(string jobId)
    {
        var job = availableJobs.Find(j => j.jobId == jobId);
        if (job == null)
        {
            Debug.LogError($"[WorkSystem] 工作不存在: {jobId}");
            return;
        }

        // 1) 检查技能要求
        if (playerSkill < job.requiredSkill)
        {
            Debug.LogWarning($"[WorkSystem] ✗ {job.jobName} - 技能不足！需要 {job.requiredSkill}，你有 {playerSkill}");
            if (gameState != null)
            {
                gameState.ApplyEffect(new List<string> 
                { 
                    "V-2",  // 失败的挫折感
                    "A+1" 
                });
            }
            return;
        }

        // 2) 请求消耗时间
        var timeRequest = new TimeConsumeRequest(job.timeRequired, $"工作: {job.jobName}");
        var timeResult = timeManager.RequestTimeConsume(timeRequest);

        if (!timeResult.success)
        {
            Debug.LogWarning($"[WorkSystem] ✗ {timeResult}");
            return;  // 时间不足，工作失败
        }

        // 3) 计算收入（根据技能等级加成）
        float skillBonus = 1f + (playerSkill / 100f);  // 技能越高，收入越多
        float income = job.baseSalary * skillBonus;

        // 4) 应用所有效果到 GameState
        if (gameState != null)
        {
            var effects = new List<string>
            {
                $"gold+{income:F0}",      // 增加金币
                $"V{(job.vChange > 0 ? "+" : "")}{job.vChange}",  // 情绪变化
                $"A{(job.aChange > 0 ? "+" : "")}{job.aChange}"
            };
            gameState.ApplyEffect(effects);
        }

        // 5) 增加经验和技能
        playerExperience += job.expGain;
        float skillGain = job.expGain * 0.5f;  // 经验的一半转换为技能
        playerSkill += skillGain;

        // 6) 应用健康消耗
        if (gameState != null)
        {
            gameState.ApplyEffect(new List<string> { $"health-{job.healthCost}" });
        }

        // 打印成功日志
        Debug.Log($"[WorkSystem] ✓ {job.jobName} 成功完成！");
        Debug.Log($"  • 消耗时间: {job.timeRequired} 小时");
        Debug.Log($"  • 获得收入: {income:F0} 金币");
        Debug.Log($"  • 健康消耗: {job.healthCost}");
        Debug.Log($"  • 情绪变化: V{job.vChange:+0.0;-0.0;0}, A{job.aChange:+0.0;-0.0;0}");
        Debug.Log($"  • 经验获得: {job.expGain}");
        Debug.Log($"  • 当前技能: {playerSkill:F1}, 经验: {playerExperience:F1}");
        Debug.Log($"  • 剩余时间: {timeResult.remainingHours:F1} 小时\n");
    }

    /// <summary>
    /// 获取当前时间段可用的工作列表
    /// </summary>
    public List<JobData> GetAvailableJobsByTimeSlot()
    {
        string currentSlot = timeManager.GetCurrentTimeSlotName();
        var available = availableJobs.FindAll(j => j.timeSlot == currentSlot);
        return available;
    }

    /// <summary>
    /// 获取所有工作列表（调试用）
    /// </summary>
    public List<JobData> GetAllJobs() => new(availableJobs);

    /// <summary>
    /// 检查玩家是否能做某项工作（不会改变状态，仅检查）
    /// </summary>
    public bool CanDoWork(string jobId)
    {
        var job = availableJobs.Find(j => j.jobId == jobId);
        if (job == null) return false;

        // 检查技能
        if (playerSkill < job.requiredSkill) return false;

        // 检查时间
        if (!timeManager.HasEnoughTime(job.timeRequired)) return false;

        return true;
    }
}