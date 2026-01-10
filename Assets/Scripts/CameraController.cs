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
    public GameObject targetHoverPanel; // 挂载在 Target 下的 UI 面板
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

    private void Start()
    {
        defaultPos = transform.position;
        defaultRot = transform.rotation;
        currentBaseRotation = defaultRot;
        if (targetHoverPanel) targetHoverPanel.SetActive(false);
    }

    private void Update()
    {
        // 1. 点击检测 (PC/手机通用)
        if (Input.GetMouseButtonDown(0))
        {
            HandleUniversalClick();
        }

        // 2. 鼠标摇摆 (非过渡状态)
        if (!isMoving) HandleMouseFollow();
        
        // 3. Billboard 效果：让 UI 始终面对相机 (可选)
        if (targetHoverPanel && targetHoverPanel.activeSelf)
        {
            targetHoverPanel.transform.LookAt(targetHoverPanel.transform.position + transform.forward);
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

            // 点击了坐标系锚点
            if (anchor != null)
            {
                isFocusing = true;
                currentActiveAnchor = anchor;
                StartMove(anchor.GetViewPosition(), anchor.GetViewRotation());
                
                if (anchor.linkedUI != null) anchor.linkedUI.TogglePanel(true);
                if (targetHoverPanel) targetHoverPanel.SetActive(false);
                return;
            }

            // 点击了 TargetObject
            if (hitObj.CompareTag(targetTag))
            {
                if (targetHoverPanel) targetHoverPanel.SetActive(true);
                return;
            }
        }

        // 点击空白处：重置一切
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