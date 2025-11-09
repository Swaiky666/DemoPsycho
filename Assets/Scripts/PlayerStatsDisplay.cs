using UnityEngine;
using TMPro;

/// <summary>
/// 玩家属性显示系统 - 支持中英文本地化
/// 修复版本：正确处理时间段本地化
/// </summary>
public class PlayerStatsDisplay : MonoBehaviour
{
    [Header("UI 引用")]
    [SerializeField] private TextMeshProUGUI healthText;      // 健康值
    [SerializeField] private TextMeshProUGUI goldText;        // 金币
    [SerializeField] private TextMeshProUGUI skillText;       // 技能
    [SerializeField] private TextMeshProUGUI valenceText;     // 情绪效价
    [SerializeField] private TextMeshProUGUI arousalText;     // 情绪唤醒
    [SerializeField] private TextMeshProUGUI timeText;        // 当前时间
    [SerializeField] private TextMeshProUGUI hungerText;     // 饥饿值

    [Header("系统引用")]
    [SerializeField] private AffectGameState gameState;
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private WorkSystem workSystem;

    [Header("显示设置")]
    [SerializeField] private bool autoUpdate = true;          // 是否自动更新

    private float lastUpdateTime = 0f;
    [SerializeField] private float updateInterval = 0.1f;    // 更新间隔（秒）

    void Start()
    {
        if (gameState == null)
            gameState = FindObjectOfType<AffectGameState>();
        if (timeManager == null)
            timeManager = FindObjectOfType<TimeManager>();
        if (workSystem == null)
            workSystem = FindObjectOfType<WorkSystem>();

        // 订阅事件
        if (gameState != null)
            gameState.OnEffectApplied += OnGameStateChanged;

        if (timeManager != null)
        {
            timeManager.onTimeUpdated += OnTimeUpdated;
            timeManager.onDayChanged += OnDayChanged;
        }

        // 订阅语言改变事件
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged += OnLanguageChanged;
        }

        // 初始显示
        UpdateAllDisplays();
    }

    void OnDestroy()
    {
        if (gameState != null)
            gameState.OnEffectApplied -= OnGameStateChanged;

        if (timeManager != null)
        {
            timeManager.onTimeUpdated -= OnTimeUpdated;
            timeManager.onDayChanged -= OnDayChanged;
        }

        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged -= OnLanguageChanged;
        }
    }

    void Update()
    {
        if (autoUpdate && Time.time - lastUpdateTime > updateInterval)
        {
            lastUpdateTime = Time.time;
            UpdateAllDisplays();
        }
    }

    /// <summary>
    /// 游戏状态改变时的回调
    /// </summary>
    private void OnGameStateChanged(System.Collections.Generic.List<string> effects)
    {
        UpdateAllDisplays();
    }

    /// <summary>
    /// 时间更新时的回调（有参数版本）
    /// </summary>
    private void OnTimeUpdated(float remainTime, float usedTime, float totalTime)
    {
        UpdateTimeDisplay();
    }

    /// <summary>
    /// 日期改变时的回调（无参数版本）
    /// </summary>
    private void OnDayChanged()
    {
        UpdateAllDisplays();
    }

    /// <summary>
    /// 语言改变时的回调
    /// </summary>
    private void OnLanguageChanged(LocalizationConfig.Language language)
    {
        UpdateAllDisplays();
    }

    /// <summary>
    /// 更新所有显示
    /// </summary>
    public void UpdateAllDisplays()
    {
        if (gameState == null) return;

        UpdateHealthDisplay();
        UpdateGoldDisplay();
        UpdateSkillDisplay();
        UpdateValenceDisplay();
        UpdateArousalDisplay();
        UpdateTimeDisplay();
        UpdateHungerDisplay();
    }

    /// <summary>
/// ✨ 新增：更新饥饿值显示
/// </summary>
private void UpdateHungerDisplay()
{
    if (hungerText == null || gameState == null) return;

    string hunger = $"{gameState.hunger:F1}";
    string hungerLabel = GetLocalizedLabel("hunger", "饥饿");
    
    // 根据饥饿值改变颜色
    Color color = Color.white;
    if (gameState.hunger < gameState.hungerCriticalThreshold)
        color = Color.red;
    else if (gameState.hunger < gameState.hungerWarningThreshold)
        color = new Color(1f, 0.5f, 0f); // 橙色

    hungerText.text = $"{hungerLabel}: {hunger}";
    hungerText.color = color;
}


    /// <summary>
    /// 更新健康值显示
    /// </summary>
    private void UpdateHealthDisplay()
    {
        if (healthText == null || gameState == null) return;

        string health = $"{gameState.health:F1}";
        
        // 使用本地化标签
        string healthLabel = GetLocalizedLabel("health", "健康");
        
        // 根据健康值改变颜色
        Color color = Color.white;
        if (gameState.health < 30)
            color = Color.red;
        else if (gameState.health < 60)
            color = new Color(1f, 0.5f, 0f); // 橙色

        healthText.text = $"{healthLabel}: {health}";
        healthText.color = color;
    }

    /// <summary>
    /// 更新金币显示
    /// </summary>
    private void UpdateGoldDisplay()
    {
        if (goldText == null || gameState == null) return;

        string goldLabel = GetLocalizedLabel("gold", "金币");
        goldText.text = $"{goldLabel}: {gameState.res.gold:F0}";
        goldText.color = Color.white;
    }

    /// <summary>
    /// 更新技能显示
    /// </summary>
    private void UpdateSkillDisplay()
    {
        if (skillText == null || gameState == null) return;

        string skillLabel = GetLocalizedLabel("skill", "技能");
        skillText.text = $"{skillLabel}: {gameState.workSkill:F1}";
        skillText.color = Color.white;
    }

    /// <summary>
    /// 更新情绪效价显示
    /// </summary>
    private void UpdateValenceDisplay()
    {
        if (valenceText == null || gameState == null) return;

        string sign = gameState.valence > 0 ? "+" : "";
        string valenceLabel = GetLocalizedLabel("valence", "情绪V");
        
        // 根据情绪值改变颜色
        Color color = Color.white;
        if (gameState.valence > 5)
            color = Color.green;
        else if (gameState.valence < -5)
            color = Color.red;

        valenceText.text = $"{valenceLabel}: {sign}{gameState.valence:F2}";
        valenceText.color = color;
    }

    /// <summary>
    /// 更新情绪唤醒显示
    /// </summary>
    private void UpdateArousalDisplay()
    {
        if (arousalText == null || gameState == null) return;

        string sign = gameState.arousal > 0 ? "+" : "";
        string arousalLabel = GetLocalizedLabel("arousal", "情绪A");
        arousalText.text = $"{arousalLabel}: {sign}{gameState.arousal:F2}";
        arousalText.color = Color.white;
    }

    /// <summary>
    /// 更新时间显示
    /// 修复版本：正确处理时间段本地化
    /// </summary>
    private void UpdateTimeDisplay()
    {
        if (timeText == null || timeManager == null) return;

        int week = timeManager.GetCurrentWeek();
        int day = timeManager.GetCurrentDay();
        string timeSlotChineseName = timeManager.GetCurrentTimeSlotName();
        float usedTime = timeManager.GetUsedTime();

        string currentTime = CalculateCurrentTime(usedTime);
        
        // 获取当前语言
        LocalizationConfig.Language currentLanguage = LocalizationManager.Instance?.GetCurrentLanguage() 
            ?? LocalizationConfig.Language.Chinese;
        
        // 根据中文时段名称找到对应的 localization key
        string timeSlotKey = GetTimeSlotKey(timeSlotChineseName);
        
        // 获取本地化后的时间段名称
        string localizedTimeSlot = GetLocalizedLabel(timeSlotKey, timeSlotChineseName);
        
        string timeDisplay = "";
        
        if (currentLanguage == LocalizationConfig.Language.Chinese)
        {
            // 中文格式: 第x周y日早上06:00
            timeDisplay = $"第{week}周{day}日{localizedTimeSlot}{currentTime}";
        }
        else
        {
            // 英文格式: Week x Day y morning 06:00
            timeDisplay = $"Week {week} Day {day} {localizedTimeSlot} {currentTime}";
        }
        
        timeText.text = timeDisplay;
        timeText.color = Color.cyan;
    }

    /// <summary>
    /// 根据中文时段名称获取对应的 localization key
    /// </summary>
    private string GetTimeSlotKey(string chineseName)
    {
        switch (chineseName)
        {
            case "早上": return "morning";
            case "中午": return "noon";
            case "下午": return "afternoon";
            case "晚上": return "night";
            default: return chineseName.ToLower();
        }
    }

    /// <summary>
    /// 计算当前时刻 (HH:MM 格式)
    /// </summary>
    private string CalculateCurrentTime(float usedHours)
    {
        int startHour = 6;
        int totalMinutes = (int)(startHour * 60 + usedHours * 60);
        int hour = (totalMinutes / 60) % 24;
        int minute = totalMinutes % 60;
        return $"{hour:D2}:{minute:D2}";
    }

    /// <summary>
    /// 获取本地化标签（带默认值）
    /// </summary>
    private string GetLocalizedLabel(string key, string defaultValue)
    {
        if (LocalizationManager.Instance != null)
        {
            string result = LocalizationManager.Instance.GetString(key);
            
            // 如果返回的就是 key（说明没找到），使用默认值
            if (result == key)
            {
                return defaultValue;
            }
            
            return result;
        }
        return defaultValue;
    }

    /// <summary>
    /// 获取完整的属性字符串（可用于日志或保存）
    /// </summary>
    public string GetStatsString()
    {
        if (gameState == null) return "属性未初始化";

        return $"健康: {gameState.health:F1}, 金币: {gameState.res.gold:F0}, 技能: {gameState.workSkill:F1}, " +
               $"情绪V: {gameState.valence:F2}, 情绪A: {gameState.arousal:F2}";
    }

    /// <summary>
    /// 快速调试：打印当前属性
    /// </summary>
    [ContextMenu("DEBUG: 打印当前属性")]
    public void DebugPrintStats()
    {
        if (gameState == null) return;

        Debug.Log($"\n========== 玩家属性 ==========");
        Debug.Log($"健康: {gameState.health:F1}");
        Debug.Log($"金币: {gameState.res.gold:F0}");
        Debug.Log($"技能: {gameState.workSkill:F1}");
        Debug.Log($"情绪V: {gameState.valence:F2}");
        Debug.Log($"情绪A: {gameState.arousal:F2}");
        
        if (timeManager != null)
        {
            Debug.Log($"当前周: {timeManager.GetCurrentWeek()}");
            Debug.Log($"当前天: {timeManager.GetCurrentDay()}");
            Debug.Log($"当前时段: {timeManager.GetCurrentTimeSlotName()}");
        }

        Debug.Log($"==============================\n");
    }
}