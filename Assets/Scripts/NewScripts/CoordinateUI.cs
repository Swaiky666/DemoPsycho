using UnityEngine;
using UnityEngine.UI;
using TMPro; // 使用 TextMeshPro 必不可少

public class CoordinateUI : MonoBehaviour
{
    [Header("UI Panels (World Space)")]
    public GameObject mainInfoPanel;  // 点击显示的面板
    public GameObject hoverPanel;     // 悬停显示的面板

    [Header("UI Text References (Assign TMP or Legacy Text)")]
    public TextMeshProUGUI nameTMP;
    public TextMeshProUGUI effectTMP;
    public TextMeshProUGUI descTMP;

    private CoordinateSystem coordSys;
    private bool isPanelOpen = false;

    void Start()
    {
        coordSys = GetComponent<CoordinateSystem>();
        
        // 初始关闭 UI
        if (mainInfoPanel) mainInfoPanel.SetActive(false);
        if (hoverPanel) hoverPanel.SetActive(false);
    }

    // --- 1. 鼠标悬停逻辑 (Hover) ---
    private void OnMouseEnter()
    {
        if (hoverPanel != null) hoverPanel.SetActive(true);
    }

    private void OnMouseExit()
    {
        if (hoverPanel != null) hoverPanel.SetActive(false);
    }

    // --- 2. 点击触发逻辑 (Click) ---
    private void OnMouseDown()
    {
        isPanelOpen = !isPanelOpen; // 切换显示/隐藏
        
        if (isPanelOpen)
        {
            UpdateUIContent();
            if (mainInfoPanel) mainInfoPanel.SetActive(true);
        }
        else
        {
            if (mainInfoPanel) mainInfoPanel.SetActive(false);
        }
    }

    // --- 3. 更新 UI 内容 ---
    private void UpdateUIContent()
    {
        if (coordSys == null) return;

        // 获取当前坐标系的状态名称 (从你的 CoordinateSystem 脚本获取)
        string status = coordSys.currentRegion;

        // 根据状态名称匹配不同的内容
        switch (status)
        {
            case "Inner Circle (Region 0)":
                SetTexts("CORE AREA", "Buff: High Energy", "This is the central origin of the coordinate system.");
                break;
            case string s when s.Contains("Middle Ring"):
                SetTexts("STABILIZER ZONE", "Buff: Normal Flow", "A balanced region partitioned into 4 quadrants.");
                break;
            case string s when s.Contains("Outer Ring"):
                SetTexts("FRONTIER SECTOR", "Buff: Low Gravity", "The outermost boundary with 22.5-degree offset sectors.");
                break;
            default:
                SetTexts("UNKNOWN", "None", "Outside of the active coordinate system.");
                break;
        }
    }

    // 核心修复部分：显式检查组件是否存在
    private void SetTexts(string n, string e, string d)
    {
        if (nameTMP != null) nameTMP.text = "Status: " + n;
        if (effectTMP != null) effectTMP.text = "Effect: " + e;
        if (descTMP != null) descTMP.text = "Info: " + d;
    }
}