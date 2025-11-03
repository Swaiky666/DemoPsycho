using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 时间管理系统
/// 控制周期、日期、时间段、时间消耗追踪
/// </summary>
public partial class TimeManager : MonoBehaviour
{
    [Header("周期设置")]
    [SerializeField] private int totalWeeks = 4;  // 总周数（可在 Inspector 中设置）
    [SerializeField] private int currentWeek = 1; // 当前周数（从 1 开始）
    [SerializeField] private int currentDay = 1;  // 当前天数（1-7）

    // ===== 时间段定义 =====
    public struct TimeSlot
    {
        public string name;        // 时间段名称（早/中/晚/夜）
        public float hours;        // 该时间段总时长（小时）
        public float startHour;    // 开始小时
        public float endHour;      // 结束小时
    }

    private List<TimeSlot> timeSlots = new();
    private float currentDayTimeUsed = 0f;  // 当前天已消耗时间
    private float currentDayTotalTime = 0f; // 当前天总时间

    [Header("时间消耗历史")]
    private List<string> dailyLog = new();  // 记录当天所有时间消耗

    // ===== 回调定义 =====
    public delegate void OnTimeUpdated(float remainTime, float usedTime, float totalTime);
    public delegate void OnDayChanged();
    public delegate void OnWeekChanged();
    public delegate void OnGameEnd();

    public event OnTimeUpdated onTimeUpdated;
    public event OnDayChanged onDayChanged;
    public event OnWeekChanged onWeekChanged;
    public event OnGameEnd onGameEnd;

    void Start()
    {
        InitializeTimeSlots();
        ResetDailyTime();
    }

    /// <summary>
    /// 初始化时间段（早/中/晚/夜）
    /// </summary>
    private void InitializeTimeSlots()
    {
        timeSlots.Clear();
        timeSlots.Add(new TimeSlot 
        { 
            name = "早上", 
            hours = 4f,      // 6:00-10:00
            startHour = 6f, 
            endHour = 10f 
        });
        timeSlots.Add(new TimeSlot 
        { 
            name = "中午", 
            hours = 3f,      // 11:00-14:00
            startHour = 11f, 
            endHour = 14f 
        });
        timeSlots.Add(new TimeSlot 
        { 
            name = "下午", 
            hours = 3f,      // 15:00-18:00
            startHour = 15f, 
            endHour = 18f 
        });
        timeSlots.Add(new TimeSlot 
        { 
            name = "晚上", 
            hours = 5f,      // 19:00-24:00
            startHour = 19f, 
            endHour = 24f 
        });

        // 计算全天总时间
        currentDayTotalTime = 0f;
        foreach (var slot in timeSlots)
            currentDayTotalTime += slot.hours;
    }

    /// <summary>
    /// 重置当天时间（新的一天开始）
    /// </summary>
    public void ResetDailyTime()
    {
        currentDayTimeUsed = 0f;
        dailyLog.Clear();
        Debug.Log($"[TimeManager] 新的一天开始 - 周 {currentWeek}，第 {currentDay} 天，总时间 {currentDayTotalTime} 小时");
    }

    /// <summary>
    /// 尝试消耗时间（主要调用接口）
    /// </summary>
    /// <param name="hours">要消耗的小时数</param>
    /// <param name="action">行为名称（例如"送快递"）</param>
    /// <returns>成功消耗返回 true，失败返回 false</returns>
    public bool TryConsumeTime(float hours, string action = "未命名行为")
    {
        float remainTime = GetRemainTime();

        // 检查时间是否足够
        if (remainTime < hours)
        {
            Debug.LogWarning($"[TimeManager] 时间不足！请求消耗 {hours} 小时，剩余仅 {remainTime:F1} 小时");
            return false;  // 消耗被驳回
        }

        // 消耗时间
        currentDayTimeUsed += hours;
        dailyLog.Add($"{action}: -{hours} 小时 (剩余: {GetRemainTime():F1}h)");

        Debug.Log($"[TimeManager] ✓ {action} 消耗 {hours} 小时 | 今日已用: {currentDayTimeUsed:F1}h / {currentDayTotalTime}h");

        // 广播事件
        onTimeUpdated?.Invoke(GetRemainTime(), currentDayTimeUsed, currentDayTotalTime);

        // 检查当天是否已满
        if (Mathf.Approximately(GetRemainTime(), 0f))
        {
            Debug.Log("[TimeManager] 今日时间已满！进入晚上结算阶段");
        }

        return true;  // 消耗成功
    }

    /// <summary>
    /// 获取当天剩余时间
    /// </summary>
    public float GetRemainTime() => currentDayTotalTime - currentDayTimeUsed;

    /// <summary>
    /// 获取当天剩余小时数（与 GetRemainTime 相同，用于兼容性）
    /// </summary>
    public float GetCurrentHours() => GetRemainTime();

    /// <summary>
    /// 获取当天已用时间
    /// </summary>
    public float GetUsedTime() => currentDayTimeUsed;

    /// <summary>
    /// 获取当天总时间
    /// </summary>
    public float GetTotalDayTime() => currentDayTotalTime;

    /// <summary>
    /// 获取当天使用百分比（0-1）
    /// </summary>
    public float GetTimeUsagePercent() => currentDayTimeUsed / currentDayTotalTime;

    /// <summary>
    /// 获取当前天数在周中的位置
    /// </summary>
    public int GetCurrentDay() => currentDay;

    /// <summary>
    /// 获取当前周数
    /// </summary>
    public int GetCurrentWeek() => currentWeek;

    /// <summary>
    /// 获取总周数
    /// </summary>
    public int GetTotalWeeks() => totalWeeks;

    /// <summary>
    /// 进入下一天（在晚间结算时调用）
    /// </summary>
    public void AdvanceToNextDay()
    {
        PrintDailyLog();  // 打印日志
        ResetDailyTime(); // 重置时间

        currentDay++;
        if (currentDay > 7)
        {
            currentDay = 1;
            currentWeek++;
            onWeekChanged?.Invoke();

            if (currentWeek > totalWeeks)
            {
                Debug.Log("[TimeManager] 游戏周期已结束！");
                onGameEnd?.Invoke();
                return;
            }
            Debug.Log($"[TimeManager] 进入新的一周：第 {currentWeek} 周");
        }

        onDayChanged?.Invoke();
    }

    /// <summary>
    /// 打印当天时间消耗日志
    /// </summary>
    private void PrintDailyLog()
    {
        Debug.Log($"\n========== 第 {currentWeek} 周 第 {currentDay} 天 时间总结 ==========");
        Debug.Log($"当日总时间: {currentDayTotalTime} 小时");
        Debug.Log($"当日已用: {currentDayTimeUsed:F1} 小时");
        Debug.Log($"当日剩余: {GetRemainTime():F1} 小时");
        Debug.Log("--- 消耗记录 ---");
        foreach (var log in dailyLog)
            Debug.Log($"  • {log}");
        Debug.Log("=".PadRight(50, '=') + "\n");
    }

    /// <summary>
    /// 获取当前时间段信息（早/中/晚/夜）
    /// </summary>
    public string GetCurrentTimeSlotName()
    {
        // 检查时间段列表是否为空
        if (timeSlots == null || timeSlots.Count == 0)
        {
            Debug.LogWarning("[TimeManager] timeSlots 未初始化！");
            return "未知";
        }

        // 根据已用时间计算当前在哪个时间段
        float accumulatedHours = 0f;
        for (int i = 0; i < timeSlots.Count; i++)
        {
            accumulatedHours += timeSlots[i].hours;
            if (currentDayTimeUsed < accumulatedHours)
            {
                return timeSlots[i].name;
            }
        }

        // 如果超过最后一个时间段，返回最后一个
        return timeSlots[timeSlots.Count - 1].name;
    }

    /// <summary>
    /// 获取指定时间段的总时长
    /// </summary>
    public float GetTimeSlotHours(string slotName)
    {
        foreach (var slot in timeSlots)
            if (slot.name == slotName)
                return slot.hours;
        return 0f;
    }

    /// <summary>
    /// 获取所有时间段信息（调试用）
    /// </summary>
    public List<TimeSlot> GetAllTimeSlots() => new(timeSlots);

    /// <summary>
    /// 强制重置（仅调试用）
    /// </summary>
    [ContextMenu("DEBUG: 重置当日时间")]
    public void DebugResetDay()
    {
        ResetDailyTime();
        Debug.Log("[DEBUG] 当日时间已重置");
    }

    /// <summary>
    /// 强制跳到下一天（仅调试用）
    /// </summary>
    [ContextMenu("DEBUG: 跳到下一天")]
    public void DebugAdvanceDay()
    {
        AdvanceToNextDay();
        Debug.Log($"[DEBUG] 已跳至周 {currentWeek} 第 {currentDay} 天");
    }

    /// <summary>
    /// 强制消耗指定时间（仅调试用）
    /// </summary>
    public void DebugConsumeTime(float hours)
    {
        TryConsumeTime(hours, "DEBUG 消耗");
    }
}