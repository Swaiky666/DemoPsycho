using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 事件 UI 面板 - 改进版
/// ✨ 新增功能：
/// 1. Inspector中可调整按钮间距
/// 2. 按钮大小完全由Prefab控制
/// 3. 支持连续事件显示
/// </summary>
public class EventUIPanel : MonoBehaviour
{
    [Header("UI 组件")]
    [SerializeField] private Canvas eventCanvas;
    [SerializeField] private TextMeshProUGUI eventTitleText;
    [SerializeField] private TextMeshProUGUI storyText;
    [SerializeField] private RectTransform choicesContainer;
    [SerializeField] private Button choiceButtonPrefab;
    
    [Header("按钮布局设置 ✨")]
    [SerializeField] private float buttonSpacing = 20f;  // ✨ 可在Inspector中调整
    [SerializeField] private bool useVerticalLayout = true;
    
    [Header("动画设置")]
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float delayBetweenChoices = 0.1f;
    
    [Header("系统引用")]
    [SerializeField] private EventManager eventManager;
    [SerializeField] private EventStoryProvider storyProvider;
    
    [Header("调试模式")]
    [SerializeField] private bool debugMode = true;
    
    private CanvasGroup canvasGroup;
    private Button[] choiceButtons;
    private bool isDisplaying = false;
    private VerticalLayoutGroup verticalLayout;
    private HorizontalLayoutGroup horizontalLayout;

    void Awake()
    {
        canvasGroup = eventCanvas.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = eventCanvas.gameObject.AddComponent<CanvasGroup>();
        }
        
        // 初始隐藏
        eventCanvas.enabled = false;
        
        // 设置布局组件
        SetupLayoutGroup();
    }

    void Start()
    {
        if (eventManager == null)
            eventManager = FindObjectOfType<EventManager>();
        if (storyProvider == null)
            storyProvider = FindObjectOfType<EventStoryProvider>();
    }

    /// <summary>
    /// ✨ 设置布局组件
    /// </summary>
    private void SetupLayoutGroup()
    {
        if (choicesContainer == null) return;
        
        // 移除旧的布局组件
        var oldVertical = choicesContainer.GetComponent<VerticalLayoutGroup>();
        var oldHorizontal = choicesContainer.GetComponent<HorizontalLayoutGroup>();
        if (oldVertical != null) Destroy(oldVertical);
        if (oldHorizontal != null) Destroy(oldHorizontal);
        
        if (useVerticalLayout)
        {
            verticalLayout = choicesContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            verticalLayout.spacing = buttonSpacing;
            verticalLayout.childAlignment = TextAnchor.MiddleCenter;
            verticalLayout.childControlWidth = false;  // ✨ 不控制按钮宽度
            verticalLayout.childControlHeight = false;  // ✨ 不控制按钮高度
            verticalLayout.childForceExpandWidth = false;
            verticalLayout.childForceExpandHeight = false;
        }
        else
        {
            horizontalLayout = choicesContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
            horizontalLayout.spacing = buttonSpacing;
            horizontalLayout.childAlignment = TextAnchor.MiddleCenter;
            horizontalLayout.childControlWidth = false;
            horizontalLayout.childControlHeight = false;
            horizontalLayout.childForceExpandWidth = false;
            horizontalLayout.childForceExpandHeight = false;
        }
        
        if (debugMode)
        {
            Debug.Log($"[EventUIPanel] 布局设置完成: {(useVerticalLayout ? "垂直" : "水平")}, 间距: {buttonSpacing}");
        }
    }

    /// <summary>
    /// 显示事件
    /// </summary>
    public void ShowEvent(EventData eventData)
    {
        if (isDisplaying)
        {
            Debug.LogWarning("[EventUIPanel] 已有事件在显示");
            return;
        }
        
        isDisplaying = true;
        StartCoroutine(ShowEventCoroutine(eventData));
    }

    private IEnumerator ShowEventCoroutine(EventData eventData)
    {
        // 1. 激活 Canvas
        eventCanvas.enabled = true;
        canvasGroup.alpha = 0f;
        
        // 2. 设置标题
        if (eventTitleText != null)
        {
            eventTitleText.text = eventData.eventName;
        }
        
        // 3. 设置故事文本
        if (storyText != null)
        {
            string storyContent = storyProvider.GetStory(eventData);
            storyText.text = storyContent;
        }
        
        // 4. 清除旧的选择按钮
        ClearChoiceButtons();
        
        // 5. 刷新布局设置（确保间距正确）
        SetupLayoutGroup();
        
        // 6. 创建选择按钮
        CreateChoiceButtons(eventData);
        
        // 7. 淡入动画
        float timer = 0f;
        while (timer < fadeInDuration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeInDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;
        
        if (debugMode)
        {
            Debug.Log("[EventUIPanel] 事件显示完成");
        }
    }

    /// <summary>
    /// ✨ 创建选择按钮 - 改进版
    /// </summary>
    private void CreateChoiceButtons(EventData eventData)
    {
        if (choiceButtonPrefab == null)
        {
            Debug.LogError("[EventUIPanel] 选择按钮 Prefab 未指定");
            return;
        }
        
        choiceButtons = new Button[eventData.choices.Length];
        
        for (int i = 0; i < eventData.choices.Length; i++)
        {
            var choice = eventData.choices[i];
            
            // 创建按钮
            var buttonObj = Instantiate(choiceButtonPrefab, choicesContainer);
            var button = buttonObj.GetComponent<Button>();
            
            // ✨ 不修改按钮大小，完全由Prefab决定
            
            // 设置按钮文本
            var buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = storyProvider.GetChoiceText(choice);
            }
            
            // 设置点击事件
            int choiceIndex = i;
            button.onClick.AddListener(() => OnChoiceSelected(choiceIndex));
            
            choiceButtons[i] = button;
            
            // 动画延迟
            StartCoroutine(ShowChoiceButtonWithDelay(button, i * delayBetweenChoices));
        }
        
        // 强制刷新布局
        if (useVerticalLayout && verticalLayout != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(choicesContainer);
        }
        else if (!useVerticalLayout && horizontalLayout != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(choicesContainer);
        }
    }

    /// <summary>
    /// 带延迟的选择按钮显示动画
    /// </summary>
    private IEnumerator ShowChoiceButtonWithDelay(Button button, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // 缩放动画
        var transform = button.transform as RectTransform;
        float timer = 0f;
        float duration = 0.3f;
        
        transform.localScale = Vector3.zero;
        
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;
            transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, progress);
            yield return null;
        }
        
        transform.localScale = Vector3.one;
    }

    /// <summary>
    /// 选择被选中时的回调
    /// </summary>
    private void OnChoiceSelected(int choiceIndex)
    {
        if (eventManager != null)
        {
            eventManager.OnPlayerChoice(choiceIndex);
        }
        
        HideEvent();
    }

    /// <summary>
    /// 隐藏事件 UI
    /// </summary>
    private void HideEvent()
    {
        StartCoroutine(HideEventCoroutine());
    }

    private IEnumerator HideEventCoroutine()
    {
        // 淡出动画
        float timer = 0f;
        while (timer < fadeInDuration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeInDuration);
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
        eventCanvas.enabled = false;
        isDisplaying = false;
        
        if (debugMode)
        {
            Debug.Log("[EventUIPanel] 事件已隐藏");
        }
    }

    /// <summary>
    /// 清除所有选择按钮
    /// </summary>
    private void ClearChoiceButtons()
    {
        if (choicesContainer == null) return;
        
        foreach (Transform child in choicesContainer)
        {
            Destroy(child.gameObject);
        }
        
        choiceButtons = null;
    }

    /// <summary>
    /// 检查是否正在显示事件
    /// </summary>
    public bool IsDisplaying()
    {
        return isDisplaying;
    }

    /// <summary>
    /// ✨ 运行时调整按钮间距
    /// </summary>
    public void SetButtonSpacing(float spacing)
    {
        buttonSpacing = spacing;
        SetupLayoutGroup();
        
        if (choicesContainer != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(choicesContainer);
        }
    }

    /// <summary>
    /// 快速调试：显示测试事件
    /// </summary>
    [ContextMenu("DEBUG: 显示测试事件")]
    public void DebugShowTestEvent()
    {
        var testEvent = new EventData
        {
            eventId = "test_event",
            eventName = "测试事件",
            category = EventCategory.Random,
            storyKey = "story_overwork",
            choices = new EventChoice[]
            {
                new EventChoice
                {
                    choiceId = "test_choice_1",
                    choiceTextKey = "choice_overwork_rest",
                    effects = new string[] { "V+1" }
                },
                new EventChoice
                {
                    choiceId = "test_choice_2",
                    choiceTextKey = "choice_overwork_continue",
                    effects = new string[] { "V-1" }
                },
                new EventChoice
                {
                    choiceId = "test_choice_3",
                    choiceTextKey = "choice_overwork_rest",
                    effects = new string[] { "health+10" }
                }
            }
        };
        
        ShowEvent(testEvent);
    }
}