using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 事件 UI 面板
/// 负责显示事件的故事和选择选项
/// </summary>
public class EventUIPanel : MonoBehaviour
{
    [Header("UI 组件")]
    [SerializeField] private Canvas eventCanvas;                // 事件画布
    [SerializeField] private TextMeshProUGUI eventTitleText;    // 事件标题
    [SerializeField] private TextMeshProUGUI storyText;         // 故事文本
    [SerializeField] private RectTransform choicesContainer;    // 选择容器
    [SerializeField] private Button choiceButtonPrefab;         // 选择按钮 Prefab
    
    [Header("动画设置")]
    [SerializeField] private float fadeInDuration = 0.5f;       // 淡入时长
    [SerializeField] private float delayBetweenChoices = 0.1f;  // 选择按钮出现间隔
    
    [Header("系统引用")]
    [SerializeField] private EventManager eventManager;
    [SerializeField] private EventStoryProvider storyProvider;
    
    [Header("调试模式")]
    [SerializeField] private bool debugMode = true;
    
    private CanvasGroup canvasGroup;
    private Button[] choiceButtons;
    private bool isDisplaying = false;

    void Awake()
    {
        canvasGroup = eventCanvas.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = eventCanvas.gameObject.AddComponent<CanvasGroup>();
        }
        
        // 初始隐藏
        eventCanvas.enabled = false;
    }

    void Start()
    {
        if (eventManager == null)
            eventManager = FindObjectOfType<EventManager>();
        if (storyProvider == null)
            storyProvider = FindObjectOfType<EventStoryProvider>();
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
        
        // 5. 创建选择按钮
        CreateChoiceButtons(eventData);
        
        // 6. 淡入动画
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
    /// 创建选择按钮
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
            
            // 设置按钮文本
            var buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = storyProvider.GetChoiceText(choice);
            }
            
            // 设置点击事件
            int choiceIndex = i;  // 闭包捕获
            button.onClick.AddListener(() => OnChoiceSelected(choiceIndex));
            
            choiceButtons[i] = button;
            
            // 动画延迟
            StartCoroutine(ShowChoiceButtonWithDelay(button, i * delayBetweenChoices));
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
    /// 快速调试：显示测试事件
    /// </summary>
    [ContextMenu("DEBUG: 显示测试事件")]
    public void DebugShowTestEvent()
    {
        // 创建一个测试事件
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
                }
            }
        };
        
        ShowEvent(testEvent);
    }
}