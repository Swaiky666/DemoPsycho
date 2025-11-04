using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// 消费物品卡牌 - 支持中英文本地化
/// 显示单个消费物品的信息
/// 用户可点击选择
/// 支持条件判断和警告颜色显示
/// 模仿 JobCard 的设计模式
/// </summary>
public class ConsumableCard : MonoBehaviour
{
    [Header("UI 组件")]
    [SerializeField] private TextMeshProUGUI itemNameText;       // 物品名称
    [SerializeField] private TextMeshProUGUI categoryText;       // 分类
    [SerializeField] private TextMeshProUGUI costText;           // 费用（金币）
    [SerializeField] private TextMeshProUGUI timeText;           // 消耗时间
    [SerializeField] private TextMeshProUGUI healthText;         // 健康变化
    [SerializeField] private TextMeshProUGUI descriptionText;    // 描述
    [SerializeField] private Image cardImage;                   // 卡牌背景
    [SerializeField] private Image cardIcon;                    // 物品图标
    [SerializeField] private Button selectButton;               // 选择按钮
    [SerializeField] private CanvasGroup canvasGroup;           // Canvas Group（用于淡入淡出）

    [Header("卡牌状态颜色设置")]
    [SerializeField] private Color enabledColor = Color.white;    // 可用时颜色
    [SerializeField] private Color disabledColor = Color.gray;    // 不可用时颜色
    [SerializeField] private Color warningColor = Color.red;      // 不符合条件时的警告颜色
    [SerializeField] private Color categoryColors = Color.cyan;   // 分类标签颜色

    [Header("透明度设置")]
    [SerializeField] private float enabledAlpha = 1f;             // 可用时的透明度 (0-1)
    [SerializeField] private float disabledAlpha = 0.6f;          // 不可用时的透明度 (0-1)

    [Header("分类颜色")]
    [SerializeField] private Color foodColor = new Color(1f, 0.8f, 0.4f);           // 食物 - 黄色
    [SerializeField] private Color restColor = new Color(0.5f, 1f, 0.5f);           // 休息 - 绿色
    [SerializeField] private Color entertainmentColor = new Color(1f, 0.5f, 1f);   // 娱乐 - 紫色
    [SerializeField] private Color toolColor = new Color(0.5f, 0.8f, 1f);           // 工具 - 蓝色

    [Header("动画设置")]
    [SerializeField] private float slideDistance = 100f;        // 滑动距离
    [SerializeField] private float slideDuration = 0.5f;        // 滑动时长
    [SerializeField] private bool useAnimationCurve = true;
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private ConsumableItem itemData;
    private bool isAvailable = true;
    private bool hasEnoughGold = true;        // 金币是否充足
    private bool hasEnoughTime = true;        // 时间是否充足
    private RectTransform rectTransform;
    private Vector2 originalPosition;

    // 事件
    public delegate void OnCardSelectedDelegate(ConsumableItem item);
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
    public void SetItemData(ConsumableItem item, bool available = true)
    {
        itemData = item;
        isAvailable = available;

        // 更新 UI
        UpdateCardUI();

        // 更新交互状态
        UpdateInteractable();

        Debug.Log($"[ConsumableCard] 卡牌已设置: {item.itemNameKey}, 可用: {available}");
    }

    /// <summary>
    /// 设置物品的各项条件是否符合
    /// </summary>
    public void SetConditions(bool goldValid, bool timeValid)
    {
        hasEnoughGold = goldValid;
        hasEnoughTime = timeValid;
        
        // 重新更新UI颜色
        UpdateInteractable();
    }

    /// <summary>
    /// 更新卡牌 UI - 支持本地化
    /// </summary>
    private void UpdateCardUI()
    {
        if (itemData == null) return;

        // 更新物品名称（使用本地化）
        if (itemNameText != null)
        {
            string itemName = GetLocalizedString(itemData.itemNameKey, itemData.itemNameKey);
            itemNameText.text = itemName;
        }

        // 更新分类
        if (categoryText != null)
        {
            string categoryLabel = GetLocalizedString("category", "分类");
            string categoryLocalized = GetLocalizedString($"category_{itemData.category}", itemData.category);
            categoryText.text = $"{categoryLabel}: {categoryLocalized}";
            categoryText.color = GetCategoryColor(itemData.category);
        }

        // 更新费用（金币）
        if (costText != null)
        {
            string costLabel = GetLocalizedString("cost", "费用");
            costText.text = $"{costLabel}: ${itemData.cost:F0}";
        }

        // 更新消耗时间
        if (timeText != null)
        {
            string timeLabel = GetLocalizedString("time_required", "时间");
            timeText.text = $"{timeLabel}: {itemData.timeRequired:F1}h";
        }

        // 更新健康变化
        if (healthText != null)
        {
            string healthLabel = GetLocalizedString("health_change", "健康");
            string sign = itemData.healthGain > 0 ? "+" : "";
            healthText.text = $"{healthLabel}: {sign}{itemData.healthGain:F1}";
        }

        // 更新描述
        if (descriptionText != null)
        {
            string description = GetLocalizedString(itemData.descriptionKey, itemData.descriptionKey);
            descriptionText.text = description;
        }

        // 更新图标（可选）
        if (cardIcon != null && itemData != null)
        {
            // 可以根据 itemData 来设置图标
            // cardIcon.sprite = GetIconForItem(itemData.itemId);
        }

        // 更新卡牌背景颜色（根据分类）
        if (cardImage != null)
        {
            cardImage.color = GetCategoryColor(itemData.category);
        }
    }

    /// <summary>
    /// 根据分类获取颜色
    /// </summary>
    private Color GetCategoryColor(string category)
    {
        return category switch
        {
            "food" => foodColor,
            "rest" => restColor,
            "entertainment" => entertainmentColor,
            "tool" => toolColor,
            _ => Color.white
        };
    }

    /// <summary>
    /// 语言改变时的回调
    /// </summary>
    private void OnLanguageChanged(LocalizationConfig.Language language)
    {
        UpdateCardUI();
    }

    /// <summary>
    /// 更新交互状态和文字颜色
    /// </summary>
    private void UpdateInteractable()
    {
        bool interactable = isAvailable && hasEnoughGold && hasEnoughTime;

        if (selectButton != null)
        {
            selectButton.interactable = interactable;
        }

        // 根据条件判断目标颜色
        Color targetColor = enabledColor;

        if (!isAvailable)
        {
            // 完全不可用时使用禁用颜色
            targetColor = disabledColor;
        }
        else if (!hasEnoughGold || !hasEnoughTime)
        {
            // 有不符合的条件时使用警告颜色
            targetColor = warningColor;
        }

        // 应用颜色到所有文本组件
        if (itemNameText != null)
            itemNameText.color = targetColor;
        if (categoryText != null)
            categoryText.color = !hasEnoughGold && !hasEnoughTime ? warningColor : GetCategoryColor(itemData.category);
        if (costText != null)
            costText.color = !hasEnoughGold ? warningColor : targetColor;
        if (timeText != null)
            timeText.color = !hasEnoughTime ? warningColor : targetColor;
        if (healthText != null)
            healthText.color = targetColor;
        if (descriptionText != null)
            descriptionText.color = targetColor;

        // 不可用时降低透明度
        if (canvasGroup != null)
        {
            canvasGroup.alpha = interactable ? enabledAlpha : disabledAlpha;
        }

        Debug.Log($"[ConsumableCard] 更新交互状态: {itemData?.itemNameKey}, 可交互: {interactable}, " +
                  $"金币: {hasEnoughGold}, 时间: {hasEnoughTime}");
    }

    /// <summary>
    /// 卡牌被点击
    /// </summary>
    private void OnCardClicked()
    {
        if (!isAvailable)
        {
            Debug.LogWarning($"[ConsumableCard] 无法选择不可用的物品: {itemData?.itemNameKey}");
            return;
        }

        if (!hasEnoughGold || !hasEnoughTime)
        {
            Debug.LogWarning($"[ConsumableCard] 无法选择不符合条件的物品: {itemData?.itemNameKey}");
            return;
        }

        Debug.Log($"[ConsumableCard] 选择了物品: {itemData?.itemNameKey}");

        // 触发事件
        OnCardSelected?.Invoke(itemData);

        // 播放选择动画
        StartCoroutine(PlaySelectionAnimation());
    }

    /// <summary>
    /// 播放进入动画
    /// 从右边滑入
    /// </summary>
    public void PlayEnterAnimation()
    {
        // 从右边滑入到当前设置的位置
        float currentX = rectTransform.anchoredPosition.x;
        float startX = currentX + slideDistance;
        StartCoroutine(AnimateSlide(startX, currentX, slideDuration));
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
    /// 获取卡牌的物品数据
    /// </summary>
    public ConsumableItem GetItemData()
    {
        return itemData;
    }

    /// <summary>
    /// 检查卡牌是否可用
    /// </summary>
    public bool IsAvailable()
    {
        return isAvailable;
    }

    /// <summary>
    /// 检查卡牌是否完全可以选择（所有条件都符合）
    /// </summary>
    public bool IsSelectable()
    {
        return isAvailable && hasEnoughGold && hasEnoughTime;
    }

    /// <summary>
    /// 快速调试：打印卡牌信息
    /// </summary>
    [ContextMenu("DEBUG: 打印卡牌信息")]
    public void DebugPrintInfo()
    {
        if (itemData == null) return;

        Debug.Log($"\n========== ConsumableCard 信息 ==========");
        Debug.Log($"物品 ID: {itemData.itemId}");
        Debug.Log($"分类: {itemData.category}");
        Debug.Log($"费用: {itemData.cost}");
        Debug.Log($"时间: {itemData.timeRequired}");
        Debug.Log($"健康: {itemData.healthGain}");
        Debug.Log($"可用: {isAvailable}");
        Debug.Log($"金币充足: {hasEnoughGold}");
        Debug.Log($"时间充足: {hasEnoughTime}");
        Debug.Log($"完全可选: {IsSelectable()}");
        Debug.Log($"=======================================\n");
    }
}