using UnityEngine;
using System;

/// <summary>
/// 场景中的可点击按钮（3D 物体）
/// 通过 Raycast 激光检测鼠标点击
/// 支持悬停效果和点击反馈
/// </summary>
public class RaycastButton : MonoBehaviour
{
    /// <summary>
    /// 按钮类型枚举
    /// </summary>
    public enum ButtonType
    {
        Work,           // 工作按钮
        Consume,        // 消费按钮
        Rest,           // 休息按钮
        Other           // 其他
    }

    [Header("按钮设置")]
    [SerializeField] private ButtonType buttonType = ButtonType.Work;
    [SerializeField] private string buttonId = "workButton_1";
    [SerializeField] private string buttonName = "工作站";

    [Header("交互反馈")]
    [SerializeField] private Material normalMaterial;           // 正常材质
    [SerializeField] private Material hoverMaterial;            // 悬停材质
    [SerializeField] private Material activeMaterial;           // 激活材质
    [SerializeField] private float hoverScale = 1.1f;           // 悬停时的缩放
    [SerializeField] private float clickScale = 0.95f;          // 点击时的缩放
    [SerializeField] private bool useParticles = false;         // 是否使用粒子效果
    [SerializeField] private ParticleSystem clickParticles;     // 点击时的粒子

    [Header("动画设置")]
    [SerializeField] private float scaleSmoothTime = 0.1f;      // 缩放平滑时间
    [SerializeField] private bool useFloatingAnimation = true;  // 是否使用漂浮动画
    [SerializeField] private float floatingSpeed = 1f;          // 漂浮速度
    [SerializeField] private float floatingHeight = 0.2f;       // 漂浮高度

    private Renderer buttonRenderer;
    private Collider buttonCollider;
    private Vector3 originalScale;
    private Vector3 originalPosition;
    private Vector3 targetScale;
    private bool isHovering = false;
    private float hoverTimer = 0f;

    // 事件定义
    public delegate void OnButtonClickedDelegate(RaycastButton button);
    public static event OnButtonClickedDelegate OnButtonClicked;

    void Start()
    {
        originalScale = transform.localScale;
        originalPosition = transform.position;
        targetScale = originalScale;

        buttonRenderer = GetComponent<Renderer>();
        buttonCollider = GetComponent<Collider>();

        if (buttonRenderer == null)
            Debug.LogWarning($"[RaycastButton] {buttonName} 缺少 Renderer 组件！");

        if (buttonCollider == null)
            Debug.LogError($"[RaycastButton] {buttonName} 缺少 Collider 组件！必须有 Collider 才能检测点击！");

        Debug.Log($"[RaycastButton] 按钮已初始化: {buttonName} ({buttonType})");
    }

    void Update()
    {
        HandleFloatingAnimation();
        HandleScaleAnimation();
        HandleMouseInteraction();
    }

    /// <summary>
    /// 处理鼠标交互（悬停 + 点击）
    /// </summary>
    private void HandleMouseInteraction()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        bool isHit = Physics.Raycast(ray, out hit);

        // 检查是否被当前物体击中
        bool isThisButton = isHit && hit.transform == transform;

        // 处理悬停
        if (isThisButton && !isHovering)
        {
            OnHoverStart();
        }
        else if (!isThisButton && isHovering)
        {
            OnHoverEnd();
        }

        // 处理点击
        if (isThisButton && Input.GetMouseButtonDown(0))
        {
            OnClicked();
        }
    }

    /// <summary>
    /// 悬停开始
    /// </summary>
    private void OnHoverStart()
    {
        isHovering = true;
        hoverTimer = 0f;

        targetScale = originalScale * hoverScale;

        // 改变材质颜色
        if (buttonRenderer != null && hoverMaterial != null)
        {
            buttonRenderer.material = hoverMaterial;
        }

        Debug.Log($"[RaycastButton] {buttonName} 被悬停");
    }

    /// <summary>
    /// 悬停结束
    /// </summary>
    private void OnHoverEnd()
    {
        isHovering = false;
        targetScale = originalScale;

        // 恢复材质
        if (buttonRenderer != null && normalMaterial != null)
        {
            buttonRenderer.material = normalMaterial;
        }

        Debug.Log($"[RaycastButton] {buttonName} 悬停结束");
    }

    /// <summary>
    /// 被点击
    /// </summary>
    private void OnClicked()
    {
        Debug.Log($"[RaycastButton] {buttonName} 被点击！");

        // 播放点击动画
        StartCoroutine(PlayClickAnimation());

        // 触发点击事件
        OnButtonClicked?.Invoke(this);
    }

    /// <summary>
    /// 播放点击动画
    /// </summary>
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

        // 播放粒子
        if (useParticles && clickParticles != null)
        {
            clickParticles.Play();
        }
    }

    /// <summary>
    /// 处理缩放动画
    /// </summary>
    private void HandleScaleAnimation()
    {
        // 平滑插值到目标缩放
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.deltaTime / scaleSmoothTime
        );
    }

    /// <summary>
    /// 处理漂浮动画
    /// </summary>
    private void HandleFloatingAnimation()
    {
        if (!useFloatingAnimation) return;

        hoverTimer += Time.deltaTime * floatingSpeed;
        Vector3 newPosition = originalPosition;
        newPosition.y += Mathf.Sin(hoverTimer) * floatingHeight;

        transform.position = newPosition;
    }

    /// <summary>
    /// 获取按钮类型
    /// </summary>
    public ButtonType GetButtonType()
    {
        return buttonType;
    }

    /// <summary>
    /// 获取按钮 ID
    /// </summary>
    public string GetButtonId()
    {
        return buttonId;
    }

    /// <summary>
    /// 获取按钮名称
    /// </summary>
    public string GetButtonName()
    {
        return buttonName;
    }

    /// <summary>
    /// 检查是否正在被悬停
    /// </summary>
    public bool IsHovering()
    {
        return isHovering;
    }

    /// <summary>
    /// 快速调试：打印按钮信息
    /// </summary>
    [ContextMenu("DEBUG: 打印按钮信息")]
    public void DebugPrintInfo()
    {
        Debug.Log($"\n========== RaycastButton 信息 ==========");
        Debug.Log($"按钮名称: {buttonName}");
        Debug.Log($"按钮 ID: {buttonId}");
        Debug.Log($"按钮类型: {buttonType}");
        Debug.Log($"是否有 Collider: {buttonCollider != null}");
        Debug.Log($"是否有 Renderer: {buttonRenderer != null}");
        Debug.Log($"是否正在悬停: {isHovering}");
        Debug.Log($"======================================\n");
    }

    /// <summary>
    /// 快速调试：模拟点击
    /// </summary>
    [ContextMenu("DEBUG: 模拟点击")]
    public void DebugSimulateClick()
    {
        OnClicked();
    }
}