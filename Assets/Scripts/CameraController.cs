using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    [Header("Mouse Follow Settings")]
    [SerializeField] private float mouseSensitivity = 0.5f;
    [SerializeField] private float maxRotationAngle = 10f;
    [SerializeField] private float dampingFactor = 0.1f;

    [Header("Smooth Movement Settings")]
    [SerializeField] private float smoothSpeed = 5f;

    [Header("Status Debug")]
    [SerializeField] private bool isMoving = false; // 是否正在过渡动画中
    [SerializeField] private bool isFocusing = false; // 是否处于聚焦物体状态

    private float currentRotationX = 0f;
    private float currentRotationY = 0f;
    private float targetRotationX = 0f;
    private float targetRotationY = 0f;

    private Vector3 defaultWorldPos;
    private Quaternion defaultWorldRot;

    private Quaternion currentBaseRotation;
    private Coroutine currentMoveCoroutine;

    private void Start()
    {
        defaultWorldPos = transform.position;
        defaultWorldRot = transform.rotation;
        currentBaseRotation = defaultWorldRot;
        isFocusing = false;
    }

    private void Update()
    {
        HandleInputDetection();

        if (!isMoving)
        {
#if UNITY_STANDALONE || UNITY_EDITOR || UNITY_WEBGL
            HandleMouseFollow();
#endif
        }
    }

    private void HandleInputDetection()
    {
        bool hasClicked = false;
        Vector3 inputPos = Vector3.zero;

        if (Input.GetMouseButtonDown(0))
        {
            hasClicked = true;
            inputPos = Input.mousePosition;
        }
        else if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            hasClicked = true;
            inputPos = Input.GetTouch(0).position;
        }

        if (hasClicked)
        {
            Ray ray = Camera.main.ScreenPointToRay(inputPos);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                CameraAnchor anchor = hit.collider.GetComponent<CameraAnchor>();
                if (anchor != null)
                {
                    Debug.Log($"<color=green>[Success]</color> Focusing to: {hit.collider.name}");
                    isFocusing = true;
                    MoveToView(anchor.GetViewPosition(), anchor.GetViewRotation());
                }
                else
                {
                    ResetToDefault();
                }
            }
            else
            {
                ResetToDefault();
            }
        }
    }

    public void ResetToDefault()
    {
        // --- 核心修复逻辑 ---
        // 如果当前已经不在聚焦状态，且没有在移动中，点击空白处不进行任何操作，防止抖动
        if (!isFocusing && !isMoving) return;

        Debug.Log("<color=white>[Reset]</color> Returning to Default View");
        isFocusing = false;
        MoveToView(defaultWorldPos, defaultWorldRot);
    }

    private void MoveToView(Vector3 targetPos, Quaternion targetRot)
    {
        if (currentMoveCoroutine != null) StopCoroutine(currentMoveCoroutine);
        currentMoveCoroutine = StartCoroutine(SmoothLerp(targetPos, targetRot));
    }

    IEnumerator SmoothLerp(Vector3 targetPos, Quaternion targetRot)
    {
        isMoving = true;
        
        // 记录当前的旋转偏移，在移动过程中渐变到0，防止瞬间跳变
        float startX = currentRotationX;
        float startY = currentRotationY;

        float elapsed = 0;
        float duration = 0.5f; // 过渡的基础参考时间

        // 为了绝对平滑，我们使用一个简单的阈值判断
        while (Vector3.Distance(transform.position, targetPos) > 0.001f || 
               Quaternion.Angle(transform.rotation, targetRot) > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * smoothSpeed);
            transform.rotation = Vector3.Distance(transform.position, targetPos) < 0.1f 
                ? Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * smoothSpeed)
                : Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * (smoothSpeed * 0.5f));

            // 在移动时平滑减少鼠标摇晃的残余值
            currentRotationX = Mathf.Lerp(currentRotationX, 0, Time.deltaTime * smoothSpeed);
            currentRotationY = Mathf.Lerp(currentRotationY, 0, Time.deltaTime * smoothSpeed);
            targetRotationX = currentRotationX;
            targetRotationY = currentRotationY;

            yield return null;
        }

        transform.position = targetPos;
        transform.rotation = targetRot;
        
        currentBaseRotation = targetRot;
        isMoving = false;
    }

    private void HandleMouseFollow()
    {
        Vector3 mousePos = Input.mousePosition;
        float screenCenterX = Screen.width / 2f;
        float screenCenterY = Screen.height / 2f;

        float normalizedX = (mousePos.x - screenCenterX) / screenCenterX;
        float normalizedY = (mousePos.y - screenCenterY) / screenCenterY;

        normalizedX = Mathf.Clamp(normalizedX, -1f, 1f);
        normalizedY = Mathf.Clamp(normalizedY, -1f, 1f);

        targetRotationX = -normalizedY * maxRotationAngle * mouseSensitivity;
        targetRotationY = normalizedX * maxRotationAngle * mouseSensitivity;

        currentRotationX = Mathf.Lerp(currentRotationX, targetRotationX, dampingFactor);
        currentRotationY = Mathf.Lerp(currentRotationY, targetRotationY, dampingFactor);

        ApplyFollowRotation();
    }

    private void ApplyFollowRotation()
    {
        Quaternion rotX = Quaternion.AngleAxis(currentRotationX, Vector3.right);
        Quaternion rotY = Quaternion.AngleAxis(currentRotationY, Vector3.up);
        
        // 保持相对于当前基准（默认位置或锚点位置）的旋转
        transform.rotation = currentBaseRotation * rotY * rotX;
    }
}