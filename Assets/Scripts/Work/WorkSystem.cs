using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 工作系统
/// 管理工作相关逻辑
/// 与 UI 系统（JobSelectionUI）协同工作
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
    /// 使用新的 JobData 类定义
    /// </summary>
    private void InitializeJobs()
    {
        availableJobs.Clear();
        
        availableJobs.Add(new JobData(
            id: "job_delivery",
            name: "送快递",
            slot: "早上",
            time: 2f,
            pay: 50f,
            skill: 10f,
            v: -0.5f,
            a: 0.5f,
            health: 10f,
            desc: "骑自行车送快递，虽然有点累但收入不错"
        ));

        availableJobs.Add(new JobData(
            id: "job_restaurant",
            name: "餐厅端盘子",
            slot: "中午",
            time: 3f,
            pay: 40f,
            skill: 8f,
            v: 0f,
            a: 1f,
            health: 15f,
            desc: "在餐厅端盘子，工作繁忙但能学到不少"
        ));

        availableJobs.Add(new JobData(
            id: "job_leaflet",
            name: "发传单",
            slot: "下午",
            time: 2.5f,
            pay: 35f,
            skill: 5f,
            v: 0.5f,
            a: -0.5f,
            health: 8f,
            desc: "在街头发传单，虽然烦人但相对轻松"
        ));

        availableJobs.Add(new JobData(
            id: "job_bar",
            name: "酒馆帮工",
            slot: "晚上",
            time: 1.5f,
            pay: 80f,
            skill: 25f,
            v: -1f,
            a: 1.5f,
            health: 20f,
            desc: "在酒馆打工，高收入但要求技能高"
        ));

        Debug.Log("[WorkSystem] 工作列表初始化完成 ✓");
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
        float income = job.basePay * skillBonus;

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
        playerExperience += job.requiredSkill * 0.5f;
        float skillGain = job.requiredSkill * 0.1f;
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
        Debug.Log($"  • 经验获得: {job.requiredSkill * 0.5f}");
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

    /// <summary>
    /// 执行指定的工作
    /// 供新 UI 系统（JobSelectionUI）调用
    /// </summary>
    public void ExecuteJob(JobData jobData)
    {
        if (jobData == null)
        {
            Debug.LogError("[WorkSystem] 工作数据不能为空！");
            return;
        }

        DoWorkWithJobData(jobData);
    }

    /// <summary>
    /// 使用 JobData 对象执行工作的内部方法
    /// </summary>
    private void DoWorkWithJobData(JobData job)
    {
        Debug.Log($"[WorkSystem] 开始执行工作: {job.jobName}");

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
            Debug.LogWarning($"[WorkSystem] ✗ 时间不足！需要 {job.timeRequired} 小时");
            return;  // 时间不足，工作失败
        }

        // 3) 计算收入（根据技能等级加成）
        float skillBonus = 1f + (playerSkill / 100f);
        float income = job.basePay * skillBonus;

        // 4) 应用所有效果到 GameState
        if (gameState != null)
        {
            var effects = new List<string>
            {
                $"gold+{income:F0}",      // 增加金币
                $"V{(job.vChange > 0 ? "+" : "")}{job.vChange}",  // 情绪变化
                $"A{(job.aChange > 0 ? "+" : "")}{job.aChange}"   // 唤醒度变化
            };
            gameState.ApplyEffect(effects);
        }

        // 5) 增加经验和技能
        playerExperience += job.requiredSkill * 0.5f;  // 经验取决于技能要求
        float skillGain = job.requiredSkill * 0.1f;
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
        Debug.Log($"  • 技能提升: +{skillGain:F2}");
        Debug.Log($"  • 当前技能: {playerSkill:F1}, 经验: {playerExperience:F1}");
        Debug.Log($"  • 剩余时间: {timeResult.remainingHours:F1} 小时\n");
    }
}