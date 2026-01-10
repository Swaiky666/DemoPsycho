using UnityEngine;
using UnityEngine.UI;

public class CoordinateUI : MonoBehaviour
{
    [Header("UI 面板引用")]
    public GameObject mainInfoPanel;
    public Text statusText; // 假设你有显示状态的文字

    private bool isPanelOpen = false;

    void Start()
    {
        if (mainInfoPanel != null) mainInfoPanel.SetActive(false);
    }

    // 核心控制方法：由 CameraController 调用
    public void TogglePanel(bool isOpen)
    {
        isPanelOpen = isOpen;
        if (mainInfoPanel != null)
        {
            mainInfoPanel.SetActive(isPanelOpen);
            if (isPanelOpen) UpdateUIContent();
        }
    }

    public void UpdateUIContent()
    {
        if (statusText != null)
            statusText.text = "相机已锁定 - 坐标系统已就绪";
    }
}