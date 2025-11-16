using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 书本事件显示系统 - 完全修复版
/// ✅ 修复：动画播放、按钮水平间距
/// </summary>
public class BookEventDisplay : MonoBehaviour
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
    
    [Header("选择按钮生成")]
    [SerializeField] private EventChoice3D choiceButtonPrefab;
    [SerializeField] private Transform choiceSpawnParent;
    [SerializeField] private Vector3 firstChoicePosition = new Vector3(0, 1, 2);
    [SerializeField] private float choiceSpacingX = 2f;  // ✨ 水平间距（俯视角）
    [SerializeField] private float choiceSpacingY = 0.5f;
    [SerializeField] private bool arrangeHorizontal = true;  // true=水平排列，false=竖直排列
    
    [Header("动画时序")]
    [SerializeField] private float delayAfterFlip = 0.3f;
    
    [Header("调试")]
    [SerializeField] private bool debugMode = true;
    
    private List<EventChoice3D> currentChoiceButtons = new List<EventChoice3D>();
    private bool isDisplaying = false;
    private EventData currentEvent;
    
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
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        if (mainCamera != null)
        {
            defaultCameraPosition = mainCamera.transform.position;
            defaultCameraRotation = mainCamera.transform.rotation;
        }
        
        // 禁用自动播放（防止冲突）
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
        StartCoroutine(ShowEventSequence());
    }

    private IEnumerator ShowEventSequence()
    {
        isDisplaying = true;
        
        yield return StartCoroutine(MoveCameraToEvent());
        yield return StartCoroutine(PlayPageFlip());
        
        ClearPages();
        ClearChoiceButtons();
        
        yield return new WaitForSeconds(delayAfterFlip);
        
        string storyText = GetStoryText();
        yield return StartCoroutine(RevealText(leftPageText, storyText));
        
        CreateChoiceButtons();
        
        isDisplaying = false;
    }

    /// <summary>
    /// ✅ 正确的动画播放方法
    /// </summary>
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
        
        // ✅ 方式1：使用clip对象（最安全）
        bookAnimation.clip = pageFlipClip;
        bookAnimation.Play();
        
        if (debugMode)
        {
            Debug.Log($"[BookEventDisplay] ▶️  播放翻页动画: {pageFlipClip.name} (长度: {pageFlipClip.length:F2}秒)");
        }
        
        // 等待动画完成
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

    /// <summary>
    /// ✅ 修复：正确的按钮生成逻辑，支持水平/竖直排列
    /// </summary>
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
            
            // ✅ 根据排列方式调整下一个按钮的位置
            if (arrangeHorizontal)
            {
                currentPosition.x += choiceSpacingX;  // 水平排列，X轴增加
            }
            else
            {
                currentPosition.y -= choiceSpacingY;  // 竖直排列，Y轴减少
            }
        }
        
        if (debugMode)
        {
            string arrangement = arrangeHorizontal ? "水平" : "竖直";
            Debug.Log($"[BookEventDisplay] ✅ 创建了 {currentChoiceButtons.Count} 个选择按钮 ({arrangement}排列, 间距: {(arrangeHorizontal ? choiceSpacingX : choiceSpacingY)})");
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
        // 1. 立即清空文字
        ClearPages();
        
        // 2. 清除选择按钮
        ClearChoiceButtons();
        
        // 3. 播放翻页动画
        yield return StartCoroutine(PlayPageFlip());
        
        // 4. 通知EventManager处理选择结果
        var eventManager = EventManager.Instance;
        if (eventManager != null)
        {
            eventManager.OnPlayerChoice(choiceIndex);
        }
        
        // 5. 相机返回默认位置
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
}