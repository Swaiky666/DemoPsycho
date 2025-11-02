using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 每日循环管理器
/// 协调 TimeManager、WorkSystem、ConsumeSystem、AffectGameState 的交互
/// </summary>
public class DailyFlowManager : MonoBehaviour
{
    [Header("系统参考")]
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private WorkSystem workSystem;
    [SerializeField] private ConsumeSystem consumeSystem;
    
    // 直接引用 AffectGameState
    private AffectGameState gameState;
    private EmotionClassifier emotionClassifier;

    void Awake()
    {
        if (gameState == null)
            gameState = FindObjectOfType<AffectGameState>();
        if (emotionClassifier == null)
            emotionClassifier = FindObjectOfType<EmotionClassifier>();
    }

    [Header("每日参数")]
    [SerializeField] private float healthDecayPerDay = 5f;         // 每天自然健康衰减
    [SerializeField] private float emotionDecayStrength = 0.1f;    // 日末情绪回弹强度
    [SerializeField] private float healthToEmotionFactor = 0.5f;   // 健康过低对情绪的影响系数

    private bool isDayOngoing = true;

    void Start()
    {
        if (timeManager == null) timeManager = GetComponent<TimeManager>();
        
        // 订阅 TimeManager 的回调
        timeManager.onTimeUpdated += OnTimeUpdated;
        timeManager.onDayChanged += OnDayEnded;
        timeManager.onWeekChanged += OnWeekEnded;
        timeManager.onGameEnd += OnGameEnded;

        Debug.Log("[DailyFlowManager] 系统已初始化，准备开始游戏");
    }

    void OnDestroy()
    {
        if (timeManager != null)
        {
            timeManager.onTimeUpdated -= OnTimeUpdated;
            timeManager.onDayChanged -= OnDayEnded;
            timeManager.onWeekChanged -= OnWeekEnded;
            timeManager.onGameEnd -= OnGameEnded;
        }
    }

    /// <summary>
    /// 时间更新回调（每次消耗时间时调用）
    /// </summary>
    private void OnTimeUpdated(float remainTime, float usedTime, float totalTime)
    {
        float usagePercent = (usedTime / totalTime) * 100f;
        Debug.Log($"[DailyFlowManager] 📊 时间进度: {usagePercent:F1}% ({usedTime:F1}h / {totalTime}h) | 剩余: {remainTime:F1}h");

        // 当天时间快满时提示
        if (remainTime < 0.5f && remainTime > 0f)
            Debug.LogWarning("[DailyFlowManager] ⚠️ 今日时间即将用完，准备进入夜间结算!");
    }

    /// <summary>
    /// 一天结束时的回调（由 AdvanceToNextDay() 触发）
    /// </summary>
    private void OnDayEnded()
    {
        Debug.Log($"\n[DailyFlowManager] ========== 第 {timeManager.GetCurrentWeek()} 周 第 {timeManager.GetCurrentDay()-1} 天 结束 ==========");
        
        ExecuteEndOfDayLogic();

        Debug.Log($"[DailyFlowManager] ========== 进入新的一天 ==========\n");
    }

    /// <summary>
    /// 一周结束时的回调
    /// </summary>
    private void OnWeekEnded()
    {
        Debug.Log($"\n[DailyFlowManager] 🎉 第 {timeManager.GetCurrentWeek()-1} 周已结束，进入第 {timeManager.GetCurrentWeek()} 周");
        ExecuteEndOfWeekLogic();
    }

    /// <summary>
    /// 游戏结束回调
    /// </summary>
    private void OnGameEnded()
    {
        Debug.Log("\n[DailyFlowManager] 🏁 游戏已结束！");
        isDayOngoing = false;
    }

    /// <summary>
    /// 执行每日末结算逻辑
    /// </summary>
    private void ExecuteEndOfDayLogic()
    {
        Debug.Log("[DailyFlowManager] 🌙 执行夜间结算...");

        if (gameState == null) return;

        // 1) 自然健康衰减
        Debug.Log($"[DailyFlowManager] 📉 自然健康衰减: -{healthDecayPerDay}");
        gameState.ApplyEffect(new List<string> { $"health-{healthDecayPerDay}" });

        // 2) 健康过低 → 情绪负面
        if (gameState.health < 40f)
        {
            float emotionPenalty = (40f - gameState.health) * healthToEmotionFactor;
            Debug.LogWarning($"[DailyFlowManager] ⚠️ 健康过低 ({gameState.health:F1}) → 情绪负面调整");
            gameState.ApplyEffect(new List<string> 
            { 
                $"V-{emotionPenalty:F1}",
                "A+1"  // 焦虑
            });
        }

        // 3) 情绪坐标回弹
        Debug.Log("[DailyFlowManager] 💭 情绪回弹到原点...");
        if (emotionClassifier != null)
        {
            emotionClassifier.EndOfDayDrift();
        }

        // 4) 打印当前状态总结
        PrintDayEndSummary();

        // 5) 触发随机事件（可选）
        // TryTriggerRandomEvent();

        // 6) 存档
        // SaveManager.Save();
    }

    /// <summary>
    /// 执行每周末结算逻辑
    /// </summary>
    private void ExecuteEndOfWeekLogic()
    {
        Debug.Log("[DailyFlowManager] 📋 执行周末结算...");

        float totalGold = gameState.res.gold;
        float totalTime = timeManager.GetTotalDayTime() * 7f;  // 一周的总时间
        float avgHealth = gameState.health;

        Debug.Log($"[DailyFlowManager] 📊 周总结:");
        Debug.Log($"  • 累计金币: {totalGold:F0}");
        Debug.Log($"  • 周总时数: {totalTime:F1} 小时");
        Debug.Log($"  • 当前健康: {avgHealth:F1}");
        Debug.Log($"  • 当前情绪: V={gameState.valence:F2}, A={gameState.arousal:F2}");
    }

    /// <summary>
    /// 打印当天结束总结
    /// </summary>
    private void PrintDayEndSummary()
    {
        if (gameState == null) return;

        Debug.Log("\n[DailyFlowManager] 📌 当天数据总结:");
        Debug.Log($"  💰 金币: {gameState.res.gold:F0}");
        Debug.Log($"  ❤️ 健康: {gameState.health:F1}");
        Debug.Log($"  😊 情绪: V={gameState.valence:F2}, A={gameState.arousal:F2}");
        Debug.Log($"  🎯 周期: 第 {timeManager.GetCurrentWeek()} 周 / 第 {timeManager.GetCurrentDay()} 天");
    }

    /// <summary>
    /// 玩家手动进入下一天（通常通过"睡觉"或"进入下一天"按钮调用）
    /// </summary>
    public void SkipToDayEnd()
    {
        if (!isDayOngoing)
        {
            Debug.LogWarning("[DailyFlowManager] 游戏已结束，无法继续");
            return;
        }

        // 强制消耗剩余时间
        float remainTime = timeManager.GetRemainTime();
        if (remainTime > 0)
        {
            Debug.Log($"[DailyFlowManager] 强制消耗剩余时间: {remainTime:F1}h");
            timeManager.TryConsumeTime(remainTime, "当日剩余时间");
        }

        // 进入下一天
        timeManager.AdvanceToNextDay();
    }

    /// <summary>
    /// 快速调试命令：打印当前状态
    /// </summary>
    [ContextMenu("DEBUG: 打印当前状态")]
    public void DebugPrintCurrentState()
    {
        if (gameState == null) return;

        Debug.Log("\n========== 当前游戏状态 ==========");
        Debug.Log($"周期: 第 {timeManager.GetCurrentWeek()} 周 / 第 {timeManager.GetCurrentDay()} 天");
        Debug.Log($"时间: 已用 {timeManager.GetUsedTime():F1}h / 总计 {timeManager.GetTotalDayTime()}h (剩余 {timeManager.GetRemainTime():F1}h)");
        Debug.Log($"资源: 💰 {gameState.res.gold:F0} 金币");
        Debug.Log($"属性: ❤️ {gameState.health:F1} 健康");
        Debug.Log($"情绪: V={gameState.valence:F2}, A={gameState.arousal:F2}");
        Debug.Log("==================================\n");
    }

    /// <summary>
    /// 快速调试命令：模拟完整一天
    /// </summary>
    [ContextMenu("DEBUG: 模拟一个工作日")]
    public void DebugSimulateWorkday()
    {
        Debug.Log("\n[DEBUG] 开始模拟一个工作日...");

        // 早上：送快递
        workSystem.DoWork("job_delivery");

        // 中午：吃饭
        consumeSystem.UseItem("food_restaurant");

        // 下午：再做一个工作
        workSystem.DoWork("job_leaflet");

        // 晚上：休息
        consumeSystem.UseItem("rest_nap");

        // 进入下一天
        SkipToDayEnd();
    }
}