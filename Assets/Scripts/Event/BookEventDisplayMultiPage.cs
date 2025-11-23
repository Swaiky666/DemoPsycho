using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 多页书本显示系统 - BookEventDisplay.cs的完整替代版本
/// 支持长故事自动分页显示
/// 
/// 功能：
/// 1. 故事过长时自动分页（每页300字）
/// 2. 提供"下一页"按钮供玩家翻页
/// 3. 最后一页后显示选择按钮
/// 4. 每次翻页时播放翻页动画
/// </summary>
public class BookEventDisplayMultiPage : MonoBehaviour
{
    [Header("书本动画")]
    [SerializeField] private Animation bookAnimation;
    [SerializeField] private AnimationClip pageFlipClip;
    
    [Header("相机移动")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform eventCameraPosition;
    [SerializeField] private float cameraMoveSpeed = 2f;
    
    [Header("文字显示 - Canvas UI")]
    [SerializeField] private TextMeshProUGUI leftPageText;
    [SerializeField] private TextMeshProUGUI rightPageText;
    [SerializeField] private float textRevealSpeed = 30f;
    
    [Header("多页设置 ✨")]
    [SerializeField] private int charsPerPage = 300;  // 每页字符数
    [SerializeField] private EventChoice3D nextPageButtonPrefab;  // 下一页按钮
    [SerializeField] private Transform pageButtonSpawnPosition;
    
    [Header("选择按钮生成")]
    [SerializeField] private EventChoice3D choiceButtonPrefab;
    [SerializeField] private Transform choiceSpawnParent;
    [SerializeField] private Vector3 firstChoicePosition = new Vector3(0, 1, 2);
    [SerializeField] private float choiceSpacingX = 2f;
    [SerializeField] private float choiceSpacingY = 0.5f;
    [SerializeField] private bool arrangeHorizontal = true;
    
    [Header("动画时序")]
    [SerializeField] private float delayAfterFlip = 0.3f;
    
    [Header("调试")]
    [SerializeField] private bool debugMode = true;
    
    private List<EventChoice3D> currentChoiceButtons = new List<EventChoice3D>();
    private bool isDisplaying = false;
    private bool isShowingStory = false;  // ✨ 标记是否在显示故事
    private EventData currentEvent;
    
    private Vector3 defaultCameraPosition;
    private Quaternion defaultCameraRotation;
    private bool isCameraAtEvent = false;
    
    // ✨ 多页相关
    private List<string> storyPages = new();
    private int currentPageIndex = 0;
    private EventChoice3D nextPageButton = null;
    
    public static BookEventDisplayMultiPage Instance { get; private set; }

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
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        if (mainCamera != null)
        {
            defaultCameraPosition = mainCamera.transform.position;
            defaultCameraRotation = mainCamera.transform.rotation;
        }
        
        if (bookAnimation != null)
        {
            bookAnimation.playAutomatically = false;
        }
        
        ClearPages();
    }

    public void ShowEvent(EventData eventData)
    {
        if (isDisplaying) return;
        
        currentEvent = eventData;
        currentPageIndex = 0;  // ✨ 重置页码
        StartCoroutine(ShowEventSequence());
    }

    private IEnumerator ShowEventSequence()
    {
        isDisplaying = true;
        isShowingStory = true;  // ✨ 标记正在显示故事
        
        yield return StartCoroutine(MoveCameraToEvent());
        yield return StartCoroutine(PlayPageFlip());
        
        ClearPages();
        ClearChoiceButtons();
        
        yield return new WaitForSeconds(delayAfterFlip);
        
        string storyText = GetStoryText();
        
        // ✨ 分页逻辑
        PaginateStory(storyText);
        
        // ✨ 显示第一页
        yield return StartCoroutine(ShowCurrentPage());
        
        isDisplaying = false;
    }

    /// <summary>
    /// ✨ 将故事文本分页
    /// </summary>
    private void PaginateStory(string fullText)
    {
        storyPages.Clear();
        
        if (fullText.Length <= charsPerPage)
        {
            storyPages.Add(fullText);
            if (debugMode)
                Debug.Log($"[BookEventDisplay] 📄 故事为单页（{fullText.Length}字）");
            return;
        }
        
        int pageCount = (fullText.Length + charsPerPage - 1) / charsPerPage;
        
        for (int i = 0; i < pageCount; i++)
        {
            int startIndex = i * charsPerPage;
            int length = Mathf.Min(charsPerPage, fullText.Length - startIndex);
            
            // 尝试在句号处分页
            if (i < pageCount - 1)
            {
                int endIndex = startIndex + length;
                int lastPeriod = fullText.LastIndexOf('。', endIndex - 1, length);
                
                if (lastPeriod > startIndex)
                {
                    length = lastPeriod - startIndex + 1;
                }
            }
            
            string pageText = fullText.Substring(startIndex, length);
            storyPages.Add(pageText);
        }
        
        if (debugMode)
        {
            Debug.Log($"[BookEventDisplay] 📄 故事分为 {storyPages.Count} 页");
        }
    }

    /// <summary>
    /// ✨ 显示当前页面
    /// </summary>
    private IEnumerator ShowCurrentPage()
    {
        if (currentPageIndex >= storyPages.Count)
        {
            // 所有页面显示完毕，显示选择按钮
            isShowingStory = false;
            CreateChoiceButtons();
            yield break;
        }
        
        string pageText = storyPages[currentPageIndex];
        
        // 显示文字
        yield return StartCoroutine(RevealText(leftPageText, pageText));
        
        // 如果不是最后一页，显示"下一页"按钮
        if (currentPageIndex < storyPages.Count - 1)
        {
            CreateNextPageButton();
        }
        else
        {
            // 最后一页完成，准备显示选择按钮
            isShowingStory = false;
            CreateChoiceButtons();
        }
    }

    /// <summary>
    /// ✨ 创建"下一页"按钮
    /// </summary>
    private void CreateNextPageButton()
    {
        if (nextPageButtonPrefab == null)
        {
            Debug.LogError("[BookEventDisplay] ❌ nextPageButtonPrefab 未分配！");
            return;
        }
        
        if (nextPageButton != null)
        {
            Destroy(nextPageButton.gameObject);
        }
        
        var buttonObj = Instantiate(nextPageButtonPrefab, choiceSpawnParent);
        
        if (pageButtonSpawnPosition != null)
        {
            buttonObj.transform.localPosition = pageButtonSpawnPosition.position;
        }
        else
        {
            buttonObj.transform.localPosition = firstChoicePosition;
        }
        
        nextPageButton = buttonObj;
        
        // 设置按钮文本为"下一页"
        var textComponent = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = "下一页";
        }
        
        // 监听按钮点击
        buttonObj.OnChoiceClicked += OnNextPageClicked;
        
        if (debugMode)
        {
            Debug.Log($"[BookEventDisplay] 📖 显示'下一页'按钮 (第 {currentPageIndex + 1}/{storyPages.Count} 页)");
        }
    }

    /// <summary>
    /// ✨ 处理"下一页"按钮点击
    /// </summary>
    private void OnNextPageClicked(int choiceIndex)
    {
        if (debugMode)
        {
            Debug.Log("[BookEventDisplay] 📄 下一页被点击");
        }
        
        // 移除当前的下一页按钮
        if (nextPageButton != null)
        {
            nextPageButton.OnChoiceClicked -= OnNextPageClicked;
            Destroy(nextPageButton.gameObject);
            nextPageButton = null;
        }
        
        // 清空当前页面文字
        if (leftPageText != null)
        {
            leftPageText.text = "";
        }
        
        // 翻到下一页
        currentPageIndex++;
        
        StartCoroutine(PlayPageFlip());
        StartCoroutine(ShowPageAfterFlip());
    }

    private IEnumerator ShowPageAfterFlip()
    {
        yield return new WaitForSeconds(delayAfterFlip);
        yield return StartCoroutine(ShowCurrentPage());
    }

    private IEnumerator PlayPageFlip()
    {
        if (bookAnimation == null)
        {
            Debug.LogError("[BookEventDisplay] ❌ bookAnimation 未分配！");
            yield break;
        }
        
        if (pageFlipClip == null)
        {
            Debug.LogError("[BookEventDisplay] ❌ pageFlipClip 未分配！");
            yield break;
        }
        
        bookAnimation.clip = pageFlipClip;
        bookAnimation.Play();
        
        if (debugMode)
        {
            Debug.Log($"[BookEventDisplay] ▶️ 播放翻页动画");
        }
        
        yield return new WaitForSeconds(pageFlipClip.length);
        
        if (debugMode)
        {
            Debug.Log("[BookEventDisplay] ✅ 翻页动画完成");
        }
    }

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
        
        if (debugMode)
        {
            Debug.Log("[BookEventDisplay] 📷 相机已移动到事件位置");
        }
    }

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
        
        if (debugMode)
        {
            Debug.Log("[BookEventDisplay] 📷 相机已返回默认位置");
        }
    }

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
        
        if (debugMode)
        {
            Debug.Log("[BookEventDisplay] 📝 文字显示完成");
        }
    }

    private void CreateChoiceButtons()
    {
        if (currentEvent == null || currentEvent.choices == null) return;
        if (choiceButtonPrefab == null)
        {
            Debug.LogError("[BookEventDisplay] ❌ choiceButtonPrefab 未分配！");
            return;
        }
        
        if (choiceSpawnParent == null)
        {
            Debug.LogError("[BookEventDisplay] ❌ choiceSpawnParent 未分配！");
            return;
        }
        
        Vector3 currentPosition = firstChoicePosition;
        
        for (int i = 0; i < currentEvent.choices.Length; i++)
        {
            var choice = currentEvent.choices[i];
            
            var buttonObj = Instantiate(choiceButtonPrefab, choiceSpawnParent);
            buttonObj.transform.localPosition = currentPosition;
            
            buttonObj.SetChoiceData(choice, i);
            buttonObj.OnChoiceClicked += OnChoiceSelected;
            
            currentChoiceButtons.Add(buttonObj);
            
            if (arrangeHorizontal)
            {
                currentPosition.x += choiceSpacingX;
            }
            else
            {
                currentPosition.y -= choiceSpacingY;
            }
        }
        
        if (debugMode)
        {
            string arrangement = arrangeHorizontal ? "水平" : "竖直";
            Debug.Log($"[BookEventDisplay] ✅ 创建了 {currentChoiceButtons.Count} 个选择按钮 ({arrangement}排列)");
        }
    }

    private void OnChoiceSelected(int choiceIndex)
    {
        if (debugMode)
        {
            Debug.Log($"[BookEventDisplay] 🎯 玩家选择了选项 {choiceIndex}");
        }
        StartCoroutine(HandleChoiceSelected(choiceIndex));
    }

    private IEnumerator HandleChoiceSelected(int choiceIndex)
    {
        ClearPages();
        ClearChoiceButtons();
        
        yield return StartCoroutine(PlayPageFlip());
        
        var eventManager = EventManager.Instance;
        if (eventManager != null)
        {
            eventManager.OnPlayerChoice(choiceIndex);
        }
        
        yield return StartCoroutine(MoveCameraToDefault());
        
        if (debugMode)
        {
            Debug.Log("[BookEventDisplay] 📖 书本已翻页，等待下一个事件");
        }
    }

    private void ClearPages()
    {
        if (leftPageText != null) leftPageText.text = "";
        if (rightPageText != null) rightPageText.text = "";
        
        if (nextPageButton != null)
        {
            nextPageButton.OnChoiceClicked -= OnNextPageClicked;
            Destroy(nextPageButton.gameObject);
            nextPageButton = null;
        }
        
        storyPages.Clear();
        currentPageIndex = 0;
    }

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

    public bool IsDisplaying()
    {
        return isDisplaying;
    }
    
    public bool IsShowingStory()
    {
        return isShowingStory;  // ✨ 可以判断是否还在显示故事
    }
}