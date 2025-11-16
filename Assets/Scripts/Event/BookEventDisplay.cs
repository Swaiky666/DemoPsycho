using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 书本事件显示系统
/// 使用Animation直接控制翻页
/// 翻页由事件触发或玩家选择驱动
/// </summary>
public class BookEventDisplay : MonoBehaviour
{
    [Header("书本动画")]
    [SerializeField] private Animation bookAnimation;  // 书本动画组件
    [SerializeField] private AnimationClip pageFlipClip;  // 翻页动画片段
    
    [Header("相机移动")]
    [SerializeField] private Camera mainCamera;  // 主相机
    [SerializeField] private Transform eventCameraPosition;  // 事件时相机位置
    [SerializeField] private float cameraMoveSpeed = 2f;  // 相机移动速度
    
    [Header("文字显示 - Canvas UI")]
    [SerializeField] private TextMeshProUGUI leftPageText;   // 左页文字（Canvas UI）
    [SerializeField] private TextMeshProUGUI rightPageText;  // 右页文字（备用）
    [SerializeField] private float textRevealSpeed = 30f; // 文字显示速度（字符/秒）
    
    [Header("选择按钮生成")]
    [SerializeField] private EventChoice3D choiceButtonPrefab;
    [SerializeField] private Transform choiceSpawnParent;
    [SerializeField] private Vector3 firstChoicePosition = new Vector3(0, 1, 2);
    [SerializeField] private float choiceSpacing = 0.5f;
    
    [Header("动画时序")]
    [SerializeField] private float delayAfterFlip = 0.3f;  // 翻页后延迟
    
    private List<EventChoice3D> currentChoiceButtons = new List<EventChoice3D>();
    private bool isDisplaying = false;
    private EventData currentEvent;
    
    // 相机状态
    private Vector3 defaultCameraPosition;
    private Quaternion defaultCameraRotation;
    private bool isCameraAtEvent = false;
    
    public static BookEventDisplay Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // 自动查找主相机
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        // 保存相机默认位置
        if (mainCamera != null)
        {
            defaultCameraPosition = mainCamera.transform.position;
            defaultCameraRotation = mainCamera.transform.rotation;
        }
        
        // 初始化：显示空白书本
        ClearPages();
    }

    /// <summary>
    /// 显示新事件
    /// </summary>
    public void ShowEvent(EventData eventData)
    {
        if (isDisplaying) return;
        
        currentEvent = eventData;
        StartCoroutine(ShowEventSequence());
    }

    /// <summary>
    /// 显示事件序列
    /// </summary>
    private IEnumerator ShowEventSequence()
    {
        isDisplaying = true;
        
        // 1. 移动相机到事件位置
        yield return StartCoroutine(MoveCameraToEvent());
        
        // 2. 播放翻页动画
        yield return StartCoroutine(PlayPageFlip());
        
        // 3. 清空旧内容
        ClearPages();
        ClearChoiceButtons();
        
        // 4. 短暂延迟
        yield return new WaitForSeconds(delayAfterFlip);
        
        // 5. 获取并显示故事文本
        string storyText = GetStoryText();
        yield return StartCoroutine(RevealText(leftPageText, storyText));
        
        // 6. 生成选择按钮
        CreateChoiceButtons();
        
        isDisplaying = false;
    }

    /// <summary>
    /// 播放翻页动画
    /// </summary>
    private IEnumerator PlayPageFlip()
    {
        if (bookAnimation != null && pageFlipClip != null)
        {
            bookAnimation.Play(pageFlipClip.name);
            yield return new WaitForSeconds(pageFlipClip.length);
        }
        else
        {
            Debug.LogWarning("[BookEventDisplay] 翻页动画未设置");
            yield return null;
        }
    }

    /// <summary>
    /// 移动相机到事件位置
    /// </summary>
    private IEnumerator MoveCameraToEvent()
    {
        if (mainCamera == null || eventCameraPosition == null)
        {
            Debug.LogWarning("[BookEventDisplay] 相机或事件位置未设置");
            yield break;
        }
        
        isCameraAtEvent = true;
        
        Vector3 targetPosition = eventCameraPosition.position;
        Quaternion targetRotation = eventCameraPosition.rotation;
        
        float elapsedTime = 0f;
        Vector3 startPosition = mainCamera.transform.position;
        Quaternion startRotation = mainCamera.transform.rotation;
        
        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * cameraMoveSpeed;
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime);
            
            mainCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            mainCamera.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            
            yield return null;
        }
        
        mainCamera.transform.position = targetPosition;
        mainCamera.transform.rotation = targetRotation;
        
        Debug.Log("[BookEventDisplay] 相机已移动到事件位置");
    }

    /// <summary>
    /// 移动相机回默认位置
    /// </summary>
    private IEnumerator MoveCameraToDefault()
    {
        if (mainCamera == null)
        {
            yield break;
        }
        
        isCameraAtEvent = false;
        
        float elapsedTime = 0f;
        Vector3 startPosition = mainCamera.transform.position;
        Quaternion startRotation = mainCamera.transform.rotation;
        
        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * cameraMoveSpeed;
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime);
            
            mainCamera.transform.position = Vector3.Lerp(startPosition, defaultCameraPosition, t);
            mainCamera.transform.rotation = Quaternion.Slerp(startRotation, defaultCameraRotation, t);
            
            yield return null;
        }
        
        mainCamera.transform.position = defaultCameraPosition;
        mainCamera.transform.rotation = defaultCameraRotation;
        
        Debug.Log("[BookEventDisplay] 相机已返回默认位置");
    }

    /// <summary>
    /// 获取故事文本
    /// </summary>
    private string GetStoryText()
    {
        if (currentEvent == null) return "";
        
        var storyProvider = EventStoryProvider.Instance;
        if (storyProvider != null)
        {
            return storyProvider.GetStory(currentEvent);
        }
        
        return currentEvent.storyKey;
    }

    /// <summary>
    /// 逐字显示文本
    /// </summary>
    private IEnumerator RevealText(TextMeshProUGUI textMesh, string fullText)
    {
        if (textMesh == null) yield break;
        
        textMesh.text = "";
        int totalChars = fullText.Length;
        float charDelay = 1f / textRevealSpeed;
        
        for (int i = 0; i <= totalChars; i++)
        {
            textMesh.text = fullText.Substring(0, i);
            yield return new WaitForSeconds(charDelay);
        }
    }

    /// <summary>
    /// 创建选择按钮
    /// </summary>
    private void CreateChoiceButtons()
    {
        if (currentEvent == null || currentEvent.choices == null) return;
        if (choiceButtonPrefab == null) return;
        
        Vector3 currentPosition = firstChoicePosition;
        
        for (int i = 0; i < currentEvent.choices.Length; i++)
        {
            var choice = currentEvent.choices[i];
            
            // 实例化按钮
            var buttonObj = Instantiate(choiceButtonPrefab, choiceSpawnParent);
            buttonObj.transform.localPosition = currentPosition;
            
            // 设置按钮数据
            buttonObj.SetChoiceData(choice, i);
            
            // 订阅点击事件
            buttonObj.OnChoiceClicked += OnChoiceSelected;
            
            currentChoiceButtons.Add(buttonObj);
            
            // 下一个位置
            currentPosition.y -= choiceSpacing;
        }
        
        Debug.Log($"[BookEventDisplay] 创建了 {currentChoiceButtons.Count} 个选择按钮");
    }

    /// <summary>
    /// 选择被点击
    /// </summary>
    private void OnChoiceSelected(int choiceIndex)
    {
        Debug.Log($"[BookEventDisplay] 选择了选项 {choiceIndex}");
        
        StartCoroutine(HandleChoiceSelected(choiceIndex));
    }

    /// <summary>
    /// 处理选择点击的完整流程
    /// </summary>
    private IEnumerator HandleChoiceSelected(int choiceIndex)
    {
        // 1. 通知 EventManager 处理选择
        var eventManager = EventManager.Instance;
        if (eventManager != null)
        {
            eventManager.OnPlayerChoice(choiceIndex);
        }
        
        // 2. 清除选择按钮
        ClearChoiceButtons();
        
        // 3. 播放翻页动画
        yield return StartCoroutine(PlayPageFlip());
        
        // 4. 清空书页，显示空白（等待下一个事件）
        ClearPages();
        
        // 5. 相机返回默认位置
        yield return StartCoroutine(MoveCameraToDefault());
        
        Debug.Log("[BookEventDisplay] 书本已翻页，等待下一个事件");
    }

    /// <summary>
    /// 清空书页
    /// </summary>
    private void ClearPages()
    {
        if (leftPageText != null) leftPageText.text = "";
        if (rightPageText != null) rightPageText.text = "";
    }

    /// <summary>
    /// 清空选择按钮
    /// </summary>
    private void ClearChoiceButtons()
    {
        foreach (var button in currentChoiceButtons)
        {
            if (button != null)
            {
                button.OnChoiceClicked -= OnChoiceSelected;
                Destroy(button.gameObject);
            }
        }
        currentChoiceButtons.Clear();
    }

    /// <summary>
    /// 检查是否正在显示
    /// </summary>
    public bool IsDisplaying()
    {
        return isDisplaying;
    }
}