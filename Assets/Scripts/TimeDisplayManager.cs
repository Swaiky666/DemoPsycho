using UnityEngine;
using TMPro;

/// <summary>
/// 时间显示管理器
/// 在 UI 上实时显示当前的时刻、天数、周数
/// </summary>
public class TimeDisplayManager : MonoBehaviour
{
    [Header("UI 引用")]
    [SerializeField] private TextMeshProUGUI timeDisplay;      // 显示时刻 "6:00 - 10:00" 或 "当前: 早上"
    [SerializeField] private TextMeshProUGUI dayDisplay;       // 显示天数 "第 3 天"
    [SerializeField] private TextMeshProUGUI weekDisplay;      // 显示周数 "第 1 周"
    [SerializeField] private TextMeshProUGUI progressDisplay;  // 显示当天进度百分比

    [Header("系统引用")]
    [SerializeField] private TimeManager timeManager;

    [Header("显示格式（可选）")]
    [SerializeField] private bool show24HourFormat = true;     // 是否显示 24 小时制
    [SerializeField] private bool showTimeSlotName = true;     // 是否显示时间段名称

    void Start()
    {
        if (timeManager == null)
            timeManager = FindObjectOfType<TimeManager>();

        if (timeManager == null)
        {
            Debug.LogError("[TimeDisplayManager] 找不到 TimeManager！");
            return;
        }

        // 订阅时间更新事件
        timeManager.onTimeUpdated += OnTimeUpdated;
        timeManager.onDayChanged += OnDayChanged;
        timeManager.onWeekChanged += OnWeekChanged;

        // 使用延迟更新，确保 TimeManager 已完全初始化
        Invoke(nameof(UpdateAllDisplays), 0.1f);
    }

    void OnDestroy()
    {
        if (timeManager != null)
        {
            timeManager.onTimeUpdated -= OnTimeUpdated;
            timeManager.onDayChanged -= OnDayChanged;
            timeManager.onWeekChanged -= OnWeekChanged;
        }
    }

    /// <summary>
    /// 时间更新时的回调
    /// </summary>
    private void OnTimeUpdated(float remainTime, float usedTime, float totalTime)
    {
        UpdateTimeDisplay();
        UpdateProgressDisplay(usedTime, totalTime);
    }

    /// <summary>
    /// 天改变时的回调
    /// </summary>
    private void OnDayChanged()
    {
        UpdateDayDisplay();
        UpdateProgressDisplay(0, timeManager.GetTotalDayTime());
    }

    /// <summary>
    /// 周改变时的回调
    /// </summary>
    private void OnWeekChanged()
    {
        UpdateWeekDisplay();
    }

    /// <summary>
    /// 更新所有显示（在 Start 时调用）
    /// </summary>
    private void UpdateAllDisplays()
    {
        if (timeManager == null)
        {
            Debug.LogWarning("[TimeDisplayManager] TimeManager 未初始化！");
            return;
        }

        UpdateTimeDisplay();
        UpdateDayDisplay();
        UpdateWeekDisplay();
        UpdateProgressDisplay(timeManager.GetUsedTime(), timeManager.GetTotalDayTime());
    }

    /// <summary>
    /// 更新时间显示
    /// 显示当前时刻、时间段、剩余时间等
    /// </summary>
    private void UpdateTimeDisplay()
    {
        if (timeDisplay == null || timeManager == null) return;

        try
        {
            string timeSlotName = timeManager.GetCurrentTimeSlotName();
            float usedTime = timeManager.GetUsedTime();
            float remainTime = timeManager.GetRemainTime();
            float totalTime = timeManager.GetTotalDayTime();

            // 计算当前时刻
            string currentTime = CalculateCurrentTime(usedTime);

            // 组合显示文本
            string displayText = "";

            if (showTimeSlotName)
            {
                displayText = $"{timeSlotName} {currentTime}\n";
            }
            else
            {
                displayText = $"{currentTime}\n";
            }

            displayText += $"剩余: {remainTime:F1}h / 已用: {usedTime:F1}h";

            timeDisplay.text = displayText;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[TimeDisplayManager] 更新时间显示出错: {ex.Message}");
            if (timeDisplay != null)
                timeDisplay.text = "时间显示错误";
        }
    }

    /// <summary>
    /// 更新天数显示
    /// </summary>
    private void UpdateDayDisplay()
    {
        if (dayDisplay == null) return;

        int currentDay = timeManager.GetCurrentDay();
        dayDisplay.text = $"第 {currentDay} 天";
    }

    /// <summary>
    /// 更新周数显示
    /// </summary>
    private void UpdateWeekDisplay()
    {
        if (weekDisplay == null) return;

        int currentWeek = timeManager.GetCurrentWeek();
        int totalWeeks = timeManager.GetTotalWeeks();
        weekDisplay.text = $"第 {currentWeek} / {totalWeeks} 周";
    }

    /// <summary>
    /// 更新当天进度显示
    /// </summary>
    private void UpdateProgressDisplay(float usedTime, float totalTime)
    {
        if (progressDisplay == null) return;

        float percent = (usedTime / totalTime) * 100f;
        progressDisplay.text = $"进度: {percent:F0}%";
    }

    /// <summary>
    /// 根据已用时间计算当前时刻
    /// </summary>
    private string CalculateCurrentTime(float usedHours)
    {
        // 初始时间：6:00 (早上开始)
        int startHour = 6;
        int totalMinutes = (int)(startHour * 60 + usedHours * 60);

        int hour = (totalMinutes / 60) % 24;
        int minute = totalMinutes % 60;

        if (show24HourFormat)
        {
            return $"{hour:D2}:{minute:D2}";
        }
        else
        {
            // 12 小时制
            string period = hour < 12 ? "AM" : "PM";
            int hour12 = hour % 12;
            if (hour12 == 0) hour12 = 12;
            return $"{hour12:D2}:{minute:D2} {period}";
        }
    }

    /// <summary>
    /// 获取格式化的时间字符串（可在其他地方调用）
    /// </summary>
    public string GetFormattedTimeString()
    {
        if (timeManager == null) return "时间未初始化";

        int week = timeManager.GetCurrentWeek();
        int day = timeManager.GetCurrentDay();
        string timeSlot = timeManager.GetCurrentTimeSlotName();
        string time = CalculateCurrentTime(timeManager.GetUsedTime());

        return $"第{week}周 第{day}天 {timeSlot} {time}";
    }

    /// <summary>
    /// 快速调试：打印时间信息
    /// </summary>
    [ContextMenu("DEBUG: 打印时间信息")]
    public void DebugPrintTimeInfo()
    {
        Debug.Log($"\n========== 时间信息 ==========");
        Debug.Log($"周数: {timeManager.GetCurrentWeek()} / {timeManager.GetTotalWeeks()}");
        Debug.Log($"天数: 第 {timeManager.GetCurrentDay()} 天");
        Debug.Log($"时间段: {timeManager.GetCurrentTimeSlotName()}");
        Debug.Log($"当前时刻: {CalculateCurrentTime(timeManager.GetUsedTime())}");
        Debug.Log($"已用时间: {timeManager.GetUsedTime():F1}h");
        Debug.Log($"剩余时间: {timeManager.GetRemainTime():F1}h");
        Debug.Log($"进度: {timeManager.GetTimeUsagePercent() * 100:F0}%");
        Debug.Log($"完整信息: {GetFormattedTimeString()}");
        Debug.Log($"==============================\n");
    }
}