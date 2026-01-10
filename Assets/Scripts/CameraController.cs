using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    [Header("相机移动设置")]
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private float mouseSensitivity = 0.5f;
    [SerializeField] private float maxRotationAngle = 10f;
    [SerializeField] private float dampingFactor = 0.1f;

    [Header("UI 与 目标设置")]
    public GameObject targetHoverPanel; // 挂载在 Target 下的子物体 UI
    public string targetTag = "TargetObject";

    private Vector3 defaultPos;
    private Quaternion defaultRot;
    private Quaternion currentBaseRotation;
    
    private bool isMoving = false;
    private bool isFocusing = false;
    private CameraAnchor currentActiveAnchor;
    private Outline lastHighlightedOutline;

    // 旋转偏移变量
    private float currentRotationX, currentRotationY;
    private float targetRotationX, targetRotationY;

    private void Start()
    {
        // 记录游戏开始时的初始姿态
        defaultPos = transform.position;
        defaultRot = transform.rotation;
        currentBaseRotation = defaultRot;
        if (targetHoverPanel) targetHoverPanel.SetActive(false);
    }

    private void Update()
    {
        // 1. 全场景 Outline 悬停检测
        HandleGlobalHighlight();

        // 2. 处理点击交互
        if (Input.GetMouseButtonDown(0))
        {
            HandleUniversalClick();
        }

        // 3. 鼠标跟随摇摆 (仅在非过渡动画时执行)
        if (!isMoving) HandleMouseFollow();
        
        // 4. Billboard：让 Target UI 始终面对相机
        if (targetHoverPanel && targetHoverPanel.activeSelf)
        {
            targetHoverPanel.transform.LookAt(targetHoverPanel.transform.position + transform.forward);
        }
    }

    // --- 逻辑：悬停发光 ---
    private void HandleGlobalHighlight()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Outline outline = hit.collider.GetComponentInParent<Outline>();
            if (outline != null)
            {
                if (lastHighlightedOutline != outline)
                {
                    if (lastHighlightedOutline != null) lastHighlightedOutline.enabled = false;
                    outline.enabled = true;
                    lastHighlightedOutline = outline;
                }
                return;
            }
        }

        if (lastHighlightedOutline != null)
        {
            lastHighlightedOutline.enabled = false;
            lastHighlightedOutline = null;
        }
    }

    // --- 逻辑：点击交互 ---
    private void HandleUniversalClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            GameObject hitObj = hit.collider.gameObject;

            // 情况 A: 点击了带锚点的物体 (例如缩进轮盘)
            CameraAnchor anchor = hitObj.GetComponentInParent<CameraAnchor>();
            if (anchor != null)
            {
                isFocusing = true;
                currentActiveAnchor = anchor;
                StartMove(anchor.GetViewPosition(), anchor.GetViewRotation());
                
                if (anchor.linkedUI != null) anchor.linkedUI.TogglePanel(true);
                if (targetHoverPanel) targetHoverPanel.SetActive(false);
                return;
            }

            // 情况 B: 点击了 Target 目标 (显示其子物体 UI)
            if (hitObj.CompareTag(targetTag))
            {
                if (targetHoverPanel) targetHoverPanel.SetActive(true);
                return;
            }
        }

        // 情况 C: 点击空白处或其他普通物体 -> 触发重置
        ResetEverything();
    }

    // --- 核心修复：重置逻辑 ---
    private void ResetEverything()
    {
        // 检查相机是否已经在初始位置
        bool isAtDefault = Vector3.Distance(transform.position, defaultPos) < 0.01f && 
                           Quaternion.Angle(transform.rotation, defaultRot) < 0.1f;
        
        // 检查 UI 是否已经关闭
        bool isUIClosed = (targetHoverPanel == null || !targetHoverPanel.activeSelf);

        // 如果已经在初始态、没在动、且UI已关，则无需操作（防止点击空白抖动）
        if (!isFocusing && !isMoving && isAtDefault && isUIClosed) return;

        // 执行重置流程
        isFocusing = false;
        
        // 关闭相关的 UI 面板
        if (currentActiveAnchor && currentActiveAnchor.linkedUI) 
            currentActiveAnchor.linkedUI.TogglePanel(false);
        
        if (targetHoverPanel) targetHoverPanel.SetActive(false);

        currentActiveAnchor = null;

        // 无论如何，执行平滑回正动画
        StartMove(defaultPos, defaultRot);
    }

    private void StartMove(Vector3 p, Quaternion r)
    {
        if (isMoving) StopAllCoroutines();
        StartCoroutine(SmoothLerp(p, r));
    }

    IEnumerator SmoothLerp(Vector3 tPos, Quaternion tRot)
    {
        isMoving = true;
        
        // 记录开始时的偏转值，确保归位过程平滑
        while (Vector3.Distance(transform.position, tPos) > 0.005f || 
               Quaternion.Angle(transform.rotation, tRot) > 0.05f)
        {
            transform.position = Vector3.Lerp(transform.position, tPos, Time.deltaTime * smoothSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, tRot, Time.deltaTime * smoothSpeed);
            
            // 归位时，必须将鼠标摇摆的偏移量 currentRotation 同步清零
            currentRotationX = Mathf.Lerp(currentRotationX, 0, Time.deltaTime * smoothSpeed);
            currentRotationY = Mathf.Lerp(currentRotationY, 0, Time.deltaTime * smoothSpeed);
            targetRotationX = targetRotationY = 0;

            yield return null;
        }

        // 强行对齐最终位置
        transform.position = tPos;
        transform.rotation = tRot;
        currentBaseRotation = tRot;
        isMoving = false;
    }

    private void HandleMouseFollow()
    {
        float nx = (Input.mousePosition.x - Screen.width / 2f) / (Screen.width / 2f);
        float ny = (Input.mousePosition.y - Screen.height / 2f) / (Screen.height / 2f);
        
        targetRotationX = -Mathf.Clamp(ny, -1, 1) * maxRotationAngle * mouseSensitivity;
        targetRotationY = Mathf.Clamp(nx, -1, 1) * maxRotationAngle * mouseSensitivity;

        currentRotationX = Mathf.Lerp(currentRotationX, targetRotationX, dampingFactor);
        currentRotationY = Mathf.Lerp(currentRotationY, targetRotationY, dampingFactor);

        // 应用旋转：以当前设定的基准（初始或锚点）进行偏移
        transform.rotation = currentBaseRotation * Quaternion.Euler(currentRotationX, currentRotationY, 0);
    }
}