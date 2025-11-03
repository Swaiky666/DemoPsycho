using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// 工作选择 UI 系统
/// 管理卡牌的生成、显示、动画
/// 处理玩家交互
/// </summary>
public class JobSelectionUI : MonoBehaviour
{
    [Header("UI 引用")]
    [SerializeField] private Canvas mainCanvas;                 // 主 Canvas
    [SerializeField] private RectTransform cardContainer;      // 卡牌容器
    [SerializeField] private GameObject jobSelectionPanel;     // 工作选择面板
    [SerializeField] private JobCard jobCardPrefab;            // 卡牌 Prefab

    [Header("卡牌生成设置")]
    [SerializeField] private Transform cardSpawnPoint;         // 卡牌生成位置（右边）
    [SerializeField] private float cardHorizontalSpacing = 400f; // 卡牌水平间距
    [SerializeField] private float cardVerticalSpacing = 100f;   // 卡牌竖直间距
    [SerializeField] private float slideInDistance = 1000f;    // 滑入距离
    [SerializeField] private float slideInDuration = 0.5f;     // 滑入时长

    [Header("系统引用")]
    [SerializeField] private RaycastButton workButton;         // 工作按钮
    [SerializeField] private WorkSystem workSystem;            // 工作系统
    [SerializeField] private TimeManager timeManager;          // 时间管理
    [SerializeField] private AffectGameState gameState;        // 游戏状态
    [SerializeField] private PlayerStatsDisplay statsDisplay;  // 属性显示

    private List<JobCard> currentCards = new List<JobCard>();
    private bool isUIActive = false;

    void Start()
    {
        // 自动查找系统引用
        if (workButton == null)
            workButton = FindObjectOfType<RaycastButton>();
        if (workSystem == null)
            workSystem = FindObjectOfType<WorkSystem>();
        if (timeManager == null)
            timeManager = FindObjectOfType<TimeManager>();
        if (gameState == null)
            gameState = FindObjectOfType<AffectGameState>();
        if (statsDisplay == null)
            statsDisplay = FindObjectOfType<PlayerStatsDisplay>();
        if (mainCanvas == null)
            mainCanvas = FindObjectOfType<Canvas>();

        // 初始化 UI
        if (jobSelectionPanel != null)
        {
            jobSelectionPanel.SetActive(false);
        }

        // 订阅事件
        if (workButton != null)
        {
            RaycastButton.OnButtonClicked += OnWorkButtonClicked;
        }

        JobCard.OnCardSelected += OnJobCardSelected;

        Debug.Log("[JobSelectionUI] 系统初始化完成");
    }

    void OnDestroy()
    {
        if (workButton != null)
        {
            RaycastButton.OnButtonClicked -= OnWorkButtonClicked;
        }

        JobCard.OnCardSelected -= OnJobCardSelected;
    }

    void Update()
    {
        // 按 ESC 关闭工作选择面板
        if (Input.GetKeyDown(KeyCode.Escape) && isUIActive)
        {
            CloseJobSelection();
        }

        // 点击非卡牌区域关闭
        if (isUIActive && Input.GetMouseButtonDown(0))
        {
            if (!IsPointerOverCard())
            {
                CloseJobSelection();
            }
        }
    }

    /// <summary>
    /// 工作按钮被点击
    /// </summary>
    private void OnWorkButtonClicked(RaycastButton button)
    {
        if (button.GetButtonType() != RaycastButton.ButtonType.Work)
            return;

        Debug.Log("[JobSelectionUI] 工作按钮被点击");
        OpenJobSelection();
    }

    /// <summary>
    /// 打开工作选择界面
    /// </summary>
    private void OpenJobSelection()
    {
        if (isUIActive) return;

        isUIActive = true;

        // 显示面板
        if (jobSelectionPanel != null)
        {
            jobSelectionPanel.SetActive(true);
        }

        // 生成卡牌
        GenerateJobCards();

        Debug.Log("[JobSelectionUI] 工作选择界面已打开");
    }

    /// <summary>
    /// 关闭工作选择界面
    /// </summary>
    public void CloseJobSelection()
    {
        if (!isUIActive) return;

        isUIActive = false;

        // 移除卡牌
        DestroyAllCards();

        // 隐藏面板
        if (jobSelectionPanel != null)
        {
            jobSelectionPanel.SetActive(false);
        }

        Debug.Log("[JobSelectionUI] 工作选择界面已关闭");
    }

    /// <summary>
    /// 生成工作卡牌
    /// </summary>
    private void GenerateJobCards()
    {
        // 清空旧卡牌
        DestroyAllCards();

        if (workSystem == null || jobCardPrefab == null)
        {
            Debug.LogError("[JobSelectionUI] WorkSystem 或 JobCard Prefab 未设置！");
            return;
        }

        // 获取所有工作
        List<JobData> allJobs = workSystem.GetAllJobs();
        if (allJobs == null || allJobs.Count == 0)
        {
            Debug.LogWarning("[JobSelectionUI] 没有可用的工作数据！");
            return;
        }

        // 获取当前时间段
        string currentTimeSlot = timeManager?.GetCurrentTimeSlotName() ?? "早上";

        // 为每个工作创建卡牌
        for (int i = 0; i < allJobs.Count; i++)
        {
            JobData job = allJobs[i];

            // 检查是否可用
            bool isAvailable = CheckJobAvailability(job, currentTimeSlot);

            // 创建卡牌
            JobCard card = Instantiate(jobCardPrefab, cardContainer);
            card.SetJobData(job, isAvailable);

            // 设置位置
            RectTransform cardRect = card.GetComponent<RectTransform>();
            Vector2 position = new Vector2(
                i % 2 == 0 ? -cardHorizontalSpacing / 2 : cardHorizontalSpacing / 2,
                -i / 2 * cardVerticalSpacing
            );
            cardRect.anchoredPosition = position + new Vector2(slideInDistance, 0);

            // 播放进入动画
            card.PlayEnterAnimation();

            currentCards.Add(card);

            Debug.Log($"[JobSelectionUI] 卡牌生成: {job.jobName}, 可用: {isAvailable}");
        }
    }

    /// <summary>
    /// 检查工作是否可用
    /// </summary>
    private bool CheckJobAvailability(JobData job, string currentTimeSlot)
    {
        // 检查时间段
        if (job.timeSlot != currentTimeSlot)
            return false;

        // 检查技能要求
        if (gameState != null && gameState.workSkill < job.requiredSkill)
            return false;

        // 检查健康值是否足够
        if (gameState != null && gameState.health < job.healthCost)
            return false;

        // 检查时间是否足够
        if (timeManager != null && !timeManager.HasEnoughTime(job.timeRequired))
            return false;

        return true;
    }

    /// <summary>
    /// 工作卡牌被选择
    /// </summary>
    private void OnJobCardSelected(JobData job)
    {
        Debug.Log($"[JobSelectionUI] 选择了工作: {job.jobName}");

        if (workSystem == null) return;

        // 执行工作
        workSystem.ExecuteJob(job);

        // 更新属性显示
        if (statsDisplay != null)
        {
            statsDisplay.UpdateAllDisplays();
        }

        // 关闭 UI
        Invoke(nameof(CloseJobSelection), 0.5f);
    }

    /// <summary>
    /// 销毁所有卡牌
    /// </summary>
    private void DestroyAllCards()
    {
        foreach (JobCard card in currentCards)
        {
            if (card != null)
            {
                card.PlayExitAnimation();
            }
        }

        currentCards.Clear();
    }

    /// <summary>
    /// 检查鼠标是否在卡牌上
    /// </summary>
    private bool IsPointerOverCard()
    {
        foreach (JobCard card in currentCards)
        {
            if (card != null && RectTransformUtility.RectangleContainsScreenPoint(
                card.GetComponent<RectTransform>(),
                Input.mousePosition,
                mainCanvas.worldCamera))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 获取当前卡牌数量
    /// </summary>
    public int GetCurrentCardCount()
    {
        return currentCards.Count;
    }

    /// <summary>
    /// 快速调试：强制打开工作选择
    /// </summary>
    [ContextMenu("DEBUG: 打开工作选择")]
    public void DebugOpenJobSelection()
    {
        OpenJobSelection();
    }

    /// <summary>
    /// 快速调试：强制关闭工作选择
    /// </summary>
    [ContextMenu("DEBUG: 关闭工作选择")]
    public void DebugCloseJobSelection()
    {
        CloseJobSelection();
    }
}