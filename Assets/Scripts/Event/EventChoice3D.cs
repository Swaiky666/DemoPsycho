using UnityEngine;
using TMPro;
using System;

/// <summary>
/// 3D事件选择按钮
/// 类似RaycastButton，可被激光检测并点击
/// </summary>
public class EventChoice3D : MonoBehaviour
{
    [Header("显示 - Canvas UI")]
    [SerializeField] private TextMeshProUGUI buttonText;  // Canvas UI文本
    [SerializeField] private Renderer buttonRenderer;
    
    [Header("交互反馈")]
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material hoverMaterial;
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float clickScale = 0.95f;
    
    [Header("动画")]
    [SerializeField] private float scaleSmoothTime = 0.1f;
    [SerializeField] private bool useFloating = true;
    [SerializeField] private float floatingSpeed = 1f;
    [SerializeField] private float floatingHeight = 0.05f;
    
    private EventChoice choiceData;
    private int choiceIndex;
    private Collider buttonCollider;
    private Vector3 originalScale;
    private Vector3 originalPosition;
    private Vector3 targetScale;
    private bool isHovering = false;
    private float hoverTimer = 0f;
    
    // 事件
    public delegate void OnChoiceClickedDelegate(int choiceIndex);
    public event OnChoiceClickedDelegate OnChoiceClicked;

    void Start()
    {
        originalScale = transform.localScale;
        originalPosition = transform.localPosition;
        targetScale = originalScale;
        
        buttonRenderer = GetComponent<Renderer>();
        buttonCollider = GetComponent<Collider>();
        
        if (buttonCollider == null)
        {
            Debug.LogError("[EventChoice3D] 缺少 Collider！");
        }
    }

    void Update()
    {
        HandleFloatingAnimation();
        HandleScaleAnimation();
        HandleMouseInteraction();
    }

    /// <summary>
    /// 设置选择数据
    /// </summary>
    public void SetChoiceData(EventChoice choice, int index)
    {
        choiceData = choice;
        choiceIndex = index;
        
        // 更新文字
        if (buttonText != null)
        {
            string text = GetChoiceText();
            buttonText.text = text;
        }
    }

    /// <summary>
    /// 获取本地化的选择文本
    /// </summary>
    private string GetChoiceText()
    {
        var storyProvider = EventStoryProvider.Instance;
        if (storyProvider != null)
        {
            return storyProvider.GetChoiceText(choiceData);
        }
        return choiceData.choiceTextKey;
    }

    /// <summary>
    /// 处理鼠标交互
    /// </summary>
    private void HandleMouseInteraction()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        bool isHit = Physics.Raycast(ray, out hit);
        
        bool isThisButton = isHit && hit.transform == transform;
        
        // 悬停
        if (isThisButton && !isHovering)
        {
            OnHoverStart();
        }
        else if (!isThisButton && isHovering)
        {
            OnHoverEnd();
        }
        
        // 点击
        if (isThisButton && Input.GetMouseButtonDown(0))
        {
            OnClicked();
        }
    }

    private void OnHoverStart()
    {
        isHovering = true;
        targetScale = originalScale * hoverScale;
        
        if (buttonRenderer != null && hoverMaterial != null)
        {
            buttonRenderer.material = hoverMaterial;
        }
    }

    private void OnHoverEnd()
    {
        isHovering = false;
        targetScale = originalScale;
        
        if (buttonRenderer != null && normalMaterial != null)
        {
            buttonRenderer.material = normalMaterial;
        }
    }

    private void OnClicked()
    {
        Debug.Log($"[EventChoice3D] 选项 {choiceIndex} 被点击");
        
        StartCoroutine(PlayClickAnimation());
        
        // 触发事件
        OnChoiceClicked?.Invoke(choiceIndex);
    }

    private System.Collections.IEnumerator PlayClickAnimation()
    {
        Vector3 clickTarget = originalScale * clickScale;
        float timer = 0f;
        float duration = 0.1f;
        
        // 缩小
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;
            transform.localScale = Vector3.Lerp(targetScale, clickTarget, progress);
            yield return null;
        }
        
        // 恢复
        timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;
            transform.localScale = Vector3.Lerp(clickTarget, targetScale, progress);
            yield return null;
        }
        
        transform.localScale = targetScale;
    }

    private void HandleScaleAnimation()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.deltaTime / scaleSmoothTime
        );
    }

    private void HandleFloatingAnimation()
    {
        if (!useFloating) return;
        
        hoverTimer += Time.deltaTime * floatingSpeed;
        Vector3 newPosition = originalPosition;
        newPosition.y += Mathf.Sin(hoverTimer) * floatingHeight;
        transform.localPosition = newPosition;
    }
}