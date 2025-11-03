using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// 工作卡牌 - 支持中英文本地化
/// 显示单个工作的信息
/// 用户可点击选择
/// </summary>
public class JobCard : MonoBehaviour
{
    [Header("UI 组件")]
    [SerializeField] private TextMeshProUGUI jobNameText;      // 工作名称
    [SerializeField] private TextMeshProUGUI timeSlotText;     // 时间段
    [SerializeField] private TextMeshProUGUI skillRequiredText; // 技能要求
    [SerializeField] private TextMeshProUGUI payText;          // 薪资
    [SerializeField] private TextMeshProUGUI descriptionText;  // 描述
    [SerializeField] private Image cardImage;                  // 卡牌背景
    [SerializeField] private Image cardIcon;                   // 工作图标
    [SerializeField] private Button selectButton;              // 选择按钮
    [SerializeField] private CanvasGroup canvasGroup;          // Canvas Group（用于淡入淡出）

    [Header("卡牌状态")]
    [SerializeField] private Color enabledColor = Color.white; // 可用时颜色
    [SerializeField] private Color disabledColor = Color.gray; // 不可用时颜色

    [Header("动画设置")]
    [SerializeField] private float slideDistance = 100f;       // 滑动距离
    [SerializeField] private float slideDuration = 0.5f;       // 滑动时长
    [SerializeField] private bool useAnimationCurve = true;
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private JobData jobData;
    private bool isAvailable = true;
    private RectTransform rectTransform;
    private Vector2 originalPosition;

    // 事件
    public delegate void OnCardSelectedDelegate(JobData job);
    public static event OnCardSelectedDelegate OnCardSelected;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalPosition = rectTransform.anchoredPosition;

        if (selectButton != null)
        {
            selectButton.onClick.AddListener(OnCardClicked);
        }

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // 订阅语言改变事件
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged += OnLanguageChanged;
        }
    }

    void OnDestroy()
    {
        if (selectButton != null)
        {
            selectButton.onClick.RemoveListener(OnCardClicked);
        }

        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged -= OnLanguageChanged;
        }
    }

    /// <summary>
    /// 设置卡牌数据
    /// </summary>
    public void SetJobData(JobData job, bool available = true)
    {
        jobData = job;
        isAvailable = available;

        // 更新 UI
        UpdateCardUI();

        // 更新交互状态
        UpdateInteractable();

        Debug.Log($"[JobCard] 卡牌已设置: {job.jobName}, 可用: {available}");
    }

    /// <summary>
    /// 更新卡牌 UI - 支持本地化
    /// </summary>
    private void UpdateCardUI()
    {
        if (jobData == null) return;

        // 更新工作名称（使用本地化）
        if (jobNameText != null)
        {
            string jobKey = $"job_{jobData.jobId}";
            string jobName = GetLocalizedString(jobKey, jobData.jobName);
            jobNameText.text = jobName;
        }

        // 更新时间段（使用本地化）
        if (timeSlotText != null)
        {
            string timeSlotLabel = GetLocalizedString("time_slot", "时间段");
            string timeSlotValue = GetLocalizedString(jobData.timeSlot.ToLower(), jobData.timeSlot);
            timeSlotText.text = $"{timeSlotLabel}: {timeSlotValue}";
        }

        // 更新技能要求（使用本地化）
        if (skillRequiredText != null)
        {
            string skillLabel = GetLocalizedString("skill_required", "技能要求");
            skillRequiredText.text = $"{skillLabel}: {jobData.requiredSkill:F1}";
        }

        // 更新薪资（使用本地化）
        if (payText != null)
        {
            string payLabel = GetLocalizedString("pay", "薪资");
            payText.text = $"{payLabel}: {jobData.basePay:F0}";
        }

        // 更新描述
        if (descriptionText != null)
        {
            descriptionText.text = jobData.description;
        }

        // 更新图标
        if (cardIcon != null && jobData.jobIcon != null)
        {
            cardIcon.sprite = jobData.jobIcon;
        }

        // 更新卡牌颜色
        if (cardImage != null)
        {
            cardImage.color = jobData.jobColor;
        }
    }

    /// <summary>
    /// 语言改变时的回调
    /// </summary>
    private void OnLanguageChanged(LocalizationConfig.Language language)
    {
        UpdateCardUI();
    }

    /// <summary>
    /// 更新交互状态
    /// </summary>
    private void UpdateInteractable()
    {
        bool interactable = isAvailable;

        if (selectButton != null)
        {
            selectButton.interactable = interactable;
        }

        // 更新颜色
        Color targetColor = interactable ? enabledColor : disabledColor;
        
        if (jobNameText != null)
            jobNameText.color = targetColor;
        if (timeSlotText != null)
            timeSlotText.color = targetColor;
        if (skillRequiredText != null)
            skillRequiredText.color = targetColor;
        if (payText != null)
            payText.color = targetColor;

        // 不可用时降低透明度
        if (canvasGroup != null)
        {
            canvasGroup.alpha = interactable ? 1f : 0.6f;
        }

        Debug.Log($"[JobCard] 更新交互状态: {jobData?.jobName}, 可交互: {interactable}");
    }

    /// <summary>
    /// 卡牌被点击
    /// </summary>
    private void OnCardClicked()
    {
        if (!isAvailable)
        {
            Debug.LogWarning($"[JobCard] 无法选择不可用的工作: {jobData?.jobName}");
            return;
        }

        Debug.Log($"[JobCard] 选择了工作: {jobData?.jobName}");

        // 触发事件
        OnCardSelected?.Invoke(jobData);

        // 播放选择动画
        StartCoroutine(PlaySelectionAnimation());
    }

    /// <summary>
    /// 播放进入动画
    /// 从右边滑入
    /// </summary>
    public void PlayEnterAnimation()
    {
        StartCoroutine(AnimateSlide(slideDistance, 0, slideDuration));
    }

    /// <summary>
    /// 播放退出动画
    /// 淡出
    /// </summary>
    public void PlayExitAnimation()
    {
        StartCoroutine(AnimateFadeOut());
    }

    /// <summary>
    /// 播放选择动画
    /// 卡牌缩小并淡出
    /// </summary>
    private System.Collections.IEnumerator PlaySelectionAnimation()
    {
        float timer = 0f;
        float duration = 0.3f;
        Vector3 originalScale = transform.localScale;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;

            // 缩小
            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, progress);

            // 淡出
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, progress);
            }

            yield return null;
        }

        // 销毁卡牌
        Destroy(gameObject);
    }

    /// <summary>
    /// 滑动动画
    /// </summary>
    private System.Collections.IEnumerator AnimateSlide(float startX, float endX, float duration)
    {
        float timer = 0f;
        Vector2 startPosition = new Vector2(startX, rectTransform.anchoredPosition.y);
        Vector2 endPosition = new Vector2(endX, rectTransform.anchoredPosition.y);

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;

            // 使用动画曲线
            if (useAnimationCurve)
            {
                progress = animationCurve.Evaluate(progress);
            }

            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, progress);
            yield return null;
        }

        rectTransform.anchoredPosition = endPosition;
    }

    /// <summary>
    /// 淡出动画
    /// </summary>
    private System.Collections.IEnumerator AnimateFadeOut()
    {
        float timer = 0f;
        float duration = 0.3f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, progress);
            }

            yield return null;
        }

        Destroy(gameObject);
    }

    /// <summary>
    /// 获取本地化字符串（带默认值）
    /// </summary>
    private string GetLocalizedString(string key, string defaultValue)
    {
        if (LocalizationManager.Instance != null)
        {
            string result = LocalizationManager.Instance.GetString(key);
            // 如果返回的是 key（说明没找到），使用默认值
            return result == key ? defaultValue : result;
        }
        return defaultValue;
    }

    /// <summary>
    /// 获取卡牌的工作数据
    /// </summary>
    public JobData GetJobData()
    {
        return jobData;
    }

    /// <summary>
    /// 检查卡牌是否可用
    /// </summary>
    public bool IsAvailable()
    {
        return isAvailable;
    }

    /// <summary>
    /// 快速调试：打印卡牌信息
    /// </summary>
    [ContextMenu("DEBUG: 打印卡牌信息")]
    public void DebugPrintInfo()
    {
        if (jobData == null) return;

        Debug.Log($"\n========== JobCard 信息 ==========");
        Debug.Log($"工作名称: {jobData.jobName}");
        Debug.Log($"时间段: {jobData.timeSlot}");
        Debug.Log($"技能要求: {jobData.requiredSkill}");
        Debug.Log($"薪资: {jobData.basePay}");
        Debug.Log($"可用: {isAvailable}");
        Debug.Log($"================================\n");
    }
}