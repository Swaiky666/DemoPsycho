using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private float mouseSensitivity = 0.5f;
    [SerializeField] private float maxRotationAngle = 10f;
    [SerializeField] private float dampingFactor = 0.1f;

    [Header("UI & Targets")]
    public GameObject targetHoverPanel;
    public string targetTag = "TargetObject";

    private Vector3 defaultPos;
    private Quaternion defaultRot;
    private Quaternion currentBaseRotation;
    
    private bool isMoving = false;
    private bool isFocusing = false;
    private CameraAnchor currentActiveAnchor;

    // 摇摆变量
    private float currentRotationX, currentRotationY;
    private float targetRotationX, targetRotationY;

    // 勾边逻辑变量
    private Outline lastHighlightedOutline;

    private void Start()
    {
        defaultPos = transform.position;
        defaultRot = transform.rotation;
        currentBaseRotation = defaultRot;
        if (targetHoverPanel) targetHoverPanel.SetActive(false);
    }

    private void Update()
    {
        // 1. 处理点击检测
        if (Input.GetMouseButtonDown(0)) HandleUniversalClick();

        // 2. 处理悬停勾边 (指着就发光)
        HandleHighlight();

        // 3. 鼠标摇摆
        if (!isMoving) HandleMouseFollow();
        
        // 4. Billboard (UI 面对相机)
        if (targetHoverPanel && targetHoverPanel.activeSelf)
        {
            targetHoverPanel.transform.LookAt(targetHoverPanel.transform.position + transform.forward);
        }
    }

    // --- 新增：勾边处理逻辑 ---
    private void HandleHighlight()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.CompareTag(targetTag))
            {
                Outline outline = hit.collider.GetComponent<Outline>();
                if (outline != null)
                {
                    if (lastHighlightedOutline != outline)
                    {
                        if (lastHighlightedOutline != null) lastHighlightedOutline.enabled = false;
                        outline.enabled = true; // 开启发光
                        lastHighlightedOutline = outline;
                    }
                    return;
                }
            }
        }

        // 如果没指着物体，关闭之前的发光
        if (lastHighlightedOutline != null)
        {
            lastHighlightedOutline.enabled = false;
            lastHighlightedOutline = null;
        }
    }

    private void HandleUniversalClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            GameObject hitObj = hit.collider.gameObject;
            CameraAnchor anchor = hitObj.GetComponent<CameraAnchor>();

            if (anchor != null)
            {
                isFocusing = true;
                currentActiveAnchor = anchor;
                StartMove(anchor.GetViewPosition(), anchor.GetViewRotation());
                if (anchor.linkedUI != null) anchor.linkedUI.TogglePanel(true);
                if (targetHoverPanel) targetHoverPanel.SetActive(false);
                return;
            }

            if (hitObj.CompareTag(targetTag))
            {
                if (targetHoverPanel) targetHoverPanel.SetActive(true);
                return;
            }
        }
        ResetEverything();
    }

    private void ResetEverything()
    {
        if (!isFocusing && !isMoving && (targetHoverPanel && !targetHoverPanel.activeSelf)) return;
        isFocusing = false;
        if (currentActiveAnchor && currentActiveAnchor.linkedUI) currentActiveAnchor.linkedUI.TogglePanel(false);
        if (targetHoverPanel) targetHoverPanel.SetActive(false);
        currentActiveAnchor = null;
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
        while (Vector3.Distance(transform.position, tPos) > 0.01f || Quaternion.Angle(transform.rotation, tRot) > 0.1f)
        {
            transform.position = Vector3.Lerp(transform.position, tPos, Time.deltaTime * smoothSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, tRot, Time.deltaTime * smoothSpeed);
            currentRotationX = Mathf.Lerp(currentRotationX, 0, Time.deltaTime * smoothSpeed);
            currentRotationY = Mathf.Lerp(currentRotationY, 0, Time.deltaTime * smoothSpeed);
            yield return null;
        }
        transform.position = tPos; transform.rotation = tRot;
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
        transform.rotation = currentBaseRotation * Quaternion.Euler(currentRotationX, currentRotationY, 0);
    }
}