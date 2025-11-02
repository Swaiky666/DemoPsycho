using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Mouse Follow Settings")]
    [SerializeField] private float mouseSensitivity = 0.5f;
    [SerializeField] private float maxRotationAngle = 15f; // 最大旋转角度
    [SerializeField] private float dampingFactor = 0.1f; // 阻尼系数（0-1，越小阻尼越强）
    
    private float currentRotationX = 0f;
    private float currentRotationY = 0f;
    private float targetRotationX = 0f;
    private float targetRotationY = 0f;
    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;

    private void Start()
    {
        // 保存初始状态
        initialLocalPosition = transform.localPosition;
        initialLocalRotation = transform.localRotation;
    }

    private void Update()
    {
        // 只在PC平台运行鼠标跟随逻辑
#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX
        HandleMouseInput();
#endif
    }

    private void HandleMouseInput()
    {
        // 获取鼠标在屏幕上的归一化位置 (-1 到 1)
        Vector3 mousePos = Input.mousePosition;
        float screenCenterX = Screen.width / 2f;
        float screenCenterY = Screen.height / 2f;

        float normalizedX = (mousePos.x - screenCenterX) / screenCenterX;
        float normalizedY = (mousePos.y - screenCenterY) / screenCenterY;

        // 限制范围到 -1 到 1
        normalizedX = Mathf.Clamp(normalizedX, -1f, 1f);
        normalizedY = Mathf.Clamp(normalizedY, -1f, 1f);

        // 计算目标旋转角度（不是完全跟随）
        // X轴：鼠标向上移动时视角向上 (取反normalizedY)
        targetRotationX = -normalizedY * maxRotationAngle * mouseSensitivity;
        // Y轴旋转：鼠标向左移动时视角向左
        targetRotationY = normalizedX * maxRotationAngle * mouseSensitivity;

        // 使用阻尼插值使旋转平滑
        currentRotationX = Mathf.Lerp(currentRotationX, targetRotationX, dampingFactor);
        currentRotationY = Mathf.Lerp(currentRotationY, targetRotationY, dampingFactor);

        // 应用旋转
        ApplyRotation();
    }

    private void ApplyRotation()
    {
        // 基于初始旋转应用增量旋转
        Quaternion rotationX = Quaternion.AngleAxis(currentRotationX, Vector3.right);
        Quaternion rotationY = Quaternion.AngleAxis(currentRotationY, Vector3.up);
        
        transform.localRotation = initialLocalRotation * rotationY * rotationX;
    }

    // 可选：重置相机到初始状态
    public void ResetCamera()
    {
        currentRotationX = 0f;
        currentRotationY = 0f;
        targetRotationX = 0f;
        targetRotationY = 0f;
        transform.localPosition = initialLocalPosition;
        transform.localRotation = initialLocalRotation;
    }
}