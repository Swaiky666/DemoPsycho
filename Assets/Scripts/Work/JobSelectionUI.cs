using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// 工作选择 UI 系统 - 改进版
/// 
/// 主要功能：
/// 1. 从startPoint位置开始生成卡牌
/// 2. 卡牌进入动画
/// 3. 容器移动到endPoint
/// 4. 基于鼠标离屏幕中心距离的自动滚动
/// 5. ✓ NEW: 条件细分检查（技能、健康、时间段）
/// 6. ✓ NEW: 向卡牌传递条件状态，使其显示警告颜色
/// </summary>
public class JobSelectionUI : MonoBehaviour
{
    [Header("UI 引用")]
    [SerializeField] private Canvas mainCanvas;                 // 主 Canvas
    [SerializeField] private RectTransform cardContainer;      // 卡牌容器
    [SerializeField] private GameObject jobSelectionPanel;     // 工作选择面板

    [Header("卡牌 Prefab")]
    [SerializeField] private JobCard jobCardPrefab;            // 卡牌 Prefab

    [Header("布局设置")]
    [SerializeField] private float cardWidth = 350f;           // 卡牌宽度
    [SerializeField] private float cardSpacing = 30f;          // 卡牌之间的间距
    [SerializeField] private float cardStartX = 0f;            // 第一张卡牌的起始 X 位置

    [Header("动画设置 - 进入动画")]
    [SerializeField] private float slideInDuration = 0.5f;     // 卡牌进入动画时长

    [Header("动画设置 - 容器移动")]
    [SerializeField] private Transform startPoint;             // ⭐ Container初始位置
    [SerializeField] private Transform endPoint;               // ⭐ Container的目标位置
    [SerializeField] private float moveToEndPointDuration = 1f; // 移动到 endPoint 的时长
    [SerializeField] private AnimationCurve moveAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("鼠标滚动设置 - 基于鼠标离屏幕中心距离")]
    [SerializeField] private float edgeThreshold = 200f;       // 距屏幕中心多少像素后触发滚动
    [SerializeField] private float scrollSpeedMax = 300f;      // 最大滚动速度（像素/秒）
    [SerializeField] private float scrollSmoothTime = 0.1f;    // 容器跟随的平滑系数
    [SerializeField] private bool debugMode = false;           // 调试模式（显示边界线）

    [Header("系统引用")]
    [SerializeField] private RaycastButton workButton;         // 工作按钮
    [SerializeField] private WorkSystem workSystem;            // 工作系统
    [SerializeField] private TimeManager timeManager;          // 时间管理
    [SerializeField] private AffectGameState gameState;        // 游戏状态
    [SerializeField] private PlayerStatsDisplay statsDisplay;  // 属性显示

    private List<JobCard> currentCards = new List<JobCard>();
    private bool isUIActive = false;
    private bool isMovingToEndPoint = false;
    private bool canScroll = false;

    // 容器边界计算
    private float containerMinX;
    private float containerMaxX;
    private float containerCurrentTargetX;
    private float containerVelocity;

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

        // ⭐ 仅有的滚动方式：基于鼠标距离
        if (canScroll && !isMovingToEndPoint)
        {
            HandleMouseDistanceScroll();
        }

        // 调试：绘制边界线
        if (debugMode && canScroll && mainCanvas.worldCamera != null)
        {
            DebugDrawBoundary();
        }
    }

    /// <summary>
    /// 基于鼠标离屏幕中心距离的自动滚动
    /// 这是唯一的滚动方式，删除了所有其他方式
    /// </summary>
    private void HandleMouseDistanceScroll()
    {
        float screenCenterX = Screen.width * 0.5f;
        float mouseX = Input.mousePosition.x;
        float distanceFromCenter = Mathf.Abs(mouseX - screenCenterX);

        float scrollDelta = 0f;

        // 检查是否在触发距离外
        if (distanceFromCenter > edgeThreshold)
        {
            // 计算超出阈值的距离
            float excessDistance = distanceFromCenter - edgeThreshold;
            float screenHalfWidth = Screen.width * 0.5f;
            
            // 距离比例 (0 ~ 1)
            float distanceRatio = excessDistance / screenHalfWidth;
            
            // 根据距离计算速度
            float currentSpeed = scrollSpeedMax * distanceRatio;

            // 根据鼠标在左侧还是右侧决定方向
            if (mouseX < screenCenterX)
            {
                // 鼠标在左侧，向右滚动
                scrollDelta = currentSpeed;
            }
            else
            {
                // 鼠标在右侧，向左滚动
                scrollDelta = -currentSpeed;
            }

            containerVelocity = scrollDelta;
        }
        else
        {
            // 在安全区域内，速度衰减为0
            containerVelocity = 0f;
        }

        // 应用速度
        containerCurrentTargetX += containerVelocity * Time.deltaTime;
        containerCurrentTargetX = Mathf.Clamp(containerCurrentTargetX, containerMaxX, containerMinX);

        // 平滑移动容器
        Vector3 containerPos = cardContainer.localPosition;
        containerPos.x = Mathf.Lerp(containerPos.x, containerCurrentTargetX, Time.deltaTime / scrollSmoothTime);
        cardContainer.localPosition = containerPos;
    }

    /// <summary>
    /// 调试：在Scene视图中绘制边界线
    /// </summary>
    private void DebugDrawBoundary()
    {
        float screenCenterX = Screen.width * 0.5f;
        float leftBound = screenCenterX - edgeThreshold;
        float rightBound = screenCenterX + edgeThreshold;

        // 左边界线（黄色）
        Debug.DrawLine(
            mainCanvas.worldCamera.ScreenToWorldPoint(new Vector3(leftBound, 0, 10)),
            mainCanvas.worldCamera.ScreenToWorldPoint(new Vector3(leftBound, Screen.height, 10)),
            Color.yellow
        );

        // 右边界线（黄色）
        Debug.DrawLine(
            mainCanvas.worldCamera.ScreenToWorldPoint(new Vector3(rightBound, 0, 10)),
            mainCanvas.worldCamera.ScreenToWorldPoint(new Vector3(rightBound, Screen.height, 10)),
            Color.yellow
        );

        // 屏幕中心线（青色）
        Debug.DrawLine(
            mainCanvas.worldCamera.ScreenToWorldPoint(new Vector3(screenCenterX, 0, 10)),
            mainCanvas.worldCamera.ScreenToWorldPoint(new Vector3(screenCenterX, Screen.height, 10)),
            Color.cyan
        );

        // 鼠标位置线（红色）
        float mouseX = Input.mousePosition.x;
        Debug.DrawLine(
            mainCanvas.worldCamera.ScreenToWorldPoint(new Vector3(mouseX, 0, 10)),
            mainCanvas.worldCamera.ScreenToWorldPoint(new Vector3(mouseX, Screen.height, 10)),
            Color.red
        );
    }

    private void OnWorkButtonClicked(RaycastButton button)
    {
        if (button.GetButtonType() != RaycastButton.ButtonType.Work)
            return;

        Debug.Log("[JobSelectionUI] 工作按钮被点击");
        OpenJobSelection();
    }

    private void OpenJobSelection()
    {
        if (isUIActive) return;

        isUIActive = true;

        if (jobSelectionPanel != null)
        {
            jobSelectionPanel.SetActive(true);
        }

        GenerateJobCards();

        Debug.Log("[JobSelectionUI] 工作选择界面已打开");
    }

    public void CloseJobSelection()
    {
        if (!isUIActive) return;

        isUIActive = false;
        canScroll = false;

        DestroyAllCards();

        if (jobSelectionPanel != null)
        {
            jobSelectionPanel.SetActive(false);
        }

        Debug.Log("[JobSelectionUI] 工作选择界面已关闭");
    }

    /// <summary>
    /// 生成工作卡牌
    /// ✓ 改进版本：支持条件细分检查和警告颜色显示
    /// </summary>
    private void GenerateJobCards()
    {
        // 检查必要的系统引用
        if (workSystem == null)
        {
            Debug.LogError("[JobSelectionUI] WorkSystem 未分配!");
            return;
        }

        if (timeManager == null)
        {
            Debug.LogError("[JobSelectionUI] TimeManager 未分配!");
            return;
        }

        Debug.Log("\n" + new string('=', 80));
        Debug.Log("★★★ [GenerateJobCards] 开始生成卡牌 ★★★");
        Debug.Log(new string('=', 80));

        // 获取所有工作
        List<JobData> allJobs = workSystem.GetAllJobs();
        string currentTimeSlot = timeManager.GetCurrentTimeSlotName();
        Debug.Log($"✓ 当前时间段: {currentTimeSlot}");

        // ⭐ 重置容器状态 - 设置到startPoint
        Vector3 initialPosition = startPoint != null ? startPoint.localPosition : Vector3.zero;
        cardContainer.localPosition = initialPosition;
        containerCurrentTargetX = initialPosition.x;
        containerVelocity = 0f;
        isMovingToEndPoint = false;
        canScroll = false;
        Debug.Log($"✓ 容器已重置到初始位置 {initialPosition}");

        // 计算总宽度
        float totalWidth = 0f;
        for (int i = 0; i < allJobs.Count; i++)
        {
            totalWidth += cardWidth + cardSpacing;
        }
        totalWidth -= cardSpacing;

        // 生成卡牌
        Debug.Log($"\n--- 开始生成 {allJobs.Count} 张卡牌 ---");
        float currentXPosition = cardStartX;

        for (int i = 0; i < allJobs.Count; i++)
        {
            JobData job = allJobs[i];

            // ⭐ 改进点 1：分别检查工作的各项条件
            bool timeSlotValid = job.timeSlot == currentTimeSlot;
            bool skillValid = gameState != null && gameState.workSkill >= job.requiredSkill;
            bool healthValid = gameState != null && gameState.health >= job.healthCost;
            bool timeValid = timeManager != null && timeManager.HasEnoughTime(job.timeRequired);
            
            // 工作是否完全可用（所有条件都符合）
            bool isAvailable = timeSlotValid && skillValid && healthValid && timeValid;

            // 实例化卡牌
            JobCard card = Instantiate(jobCardPrefab, cardContainer);
            card.SetJobData(job, isAvailable);
            
            // ⭐ 改进点 2：关键步骤 - 设置卡牌的具体条件状态（用于显示警告颜色）
            // 这一步会使卡牌根据条件状态显示不同的颜色
            card.SetConditions(timeSlotValid, skillValid, healthValid);

            // 设置位置
            RectTransform cardRect = card.GetComponent<RectTransform>();
            cardRect.anchoredPosition = new Vector2(currentXPosition, 0);

            // 播放进入动画
            card.PlayEnterAnimation();
            currentCards.Add(card);

            // 计算下一个位置
            currentXPosition += cardWidth + cardSpacing;
            
            // ⭐ 调试日志：显示不可用的原因
            if (!isAvailable)
            {
                string reason = "";
                if (!timeSlotValid) reason += "时间段不符 ";
                if (!skillValid) reason += "技能不足 ";
                if (!healthValid) reason += "健康不足 ";
                if (!timeValid) reason += "时间不足 ";
                Debug.Log($"[卡牌] {job.jobName} - 不可选: {reason.Trim()}");
            }
            else
            {
                Debug.Log($"[卡牌] {job.jobName} - ✓ 完全可选");
            }
        }

        Debug.Log($"✓ 所有卡牌生成完成，共 {currentCards.Count} 张");

        // 计算容器边界
        CalculateContainerBounds(totalWidth);

        // 启动时序流程
        StartCoroutine(SequenceAfterGeneration());
        
        Debug.Log("\n" + new string('=', 80));
        Debug.Log("★★★ [GenerateJobCards] 卡牌生成完成 ★★★");
        Debug.Log(new string('=', 80) + "\n");
    }

    /// <summary>
    /// 生成后的时序流程
    /// 1. 等待卡牌进入动画
    /// 2. 移动容器到endPoint
    /// 3. 同步containerCurrentTargetX
    /// 4. 启用滚动
    /// </summary>
    private IEnumerator SequenceAfterGeneration()
    {
        // 等待卡牌进入动画完成
        yield return new WaitForSeconds(slideInDuration);
        Debug.Log("[Sequence] ✓ 卡牌进入动画完成");

        // 移动容器到endPoint
        if (endPoint != null)
        {
            yield return StartCoroutine(MoveContainerToEndPoint());
            Debug.Log("[Sequence] ✓ 容器已到达endPoint");
        }
        else
        {
            Debug.LogWarning("[Sequence] ⚠️ 没有设置 EndPoint");
        }

        // 同步位置（关键！）
        containerCurrentTargetX = cardContainer.localPosition.x;
        Debug.Log($"[Sequence] ✓ 同步 containerCurrentTargetX = {containerCurrentTargetX}");

        // 启用滚动
        canScroll = true;
        Debug.Log("[Sequence] ✅ 启用自动滚动");
    }

    /// <summary>
    /// 计算容器的边界
    /// </summary>
    private void CalculateContainerBounds(float totalCardsWidth)
    {
        RectTransform parentRect = cardContainer.parent.GetComponent<RectTransform>();
        if (parentRect == null)
        {
            Debug.LogError("[ERROR] 无法获取 Container 的父元素!");
            return;
        }

        float visibleWidth = parentRect.rect.width;
        containerMaxX = -(totalCardsWidth - visibleWidth);
        containerMinX = 0f;

        if (containerMaxX > 0)
        {
            containerMaxX = 0;
        }

        Debug.Log($"[Bounds] 可滑动范围: [{containerMaxX:F1}, {containerMinX:F1}]");
    }

    /// <summary>
    /// 移动容器到 endPoint
    /// </summary>
    private IEnumerator MoveContainerToEndPoint()
    {
        isMovingToEndPoint = true;
        float timer = 0f;
        Vector3 startPosition = cardContainer.localPosition;
        Vector3 targetPosition = endPoint.localPosition;

        Debug.Log($"[Move] 从 {startPosition} 移动到 {targetPosition}");

        while (timer < moveToEndPointDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / moveToEndPointDuration;
            progress = moveAnimationCurve.Evaluate(progress);

            cardContainer.localPosition = Vector3.Lerp(startPosition, targetPosition, progress);
            yield return null;
        }

        cardContainer.localPosition = targetPosition;
        isMovingToEndPoint = false;
    }

    /// <summary>
    /// 检查工作是否可用（保留此方法以兼容性）
    /// 注：新版本已在 GenerateJobCards() 中内联这个逻辑
    /// </summary>
    private bool CheckJobAvailability(JobData job, string currentTimeSlot)
    {
        if (job.timeSlot != currentTimeSlot)
            return false;

        if (gameState != null && gameState.workSkill < job.requiredSkill)
            return false;

        if (gameState != null && gameState.health < job.healthCost)
            return false;

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

        workSystem.ExecuteJob(job);

        if (statsDisplay != null)
        {
            statsDisplay.UpdateAllDisplays();
        }

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

    public int GetCurrentCardCount()
    {
        return currentCards.Count;
    }

    /// <summary>
    /// 获取当前卡牌列表
    /// </summary>
    public List<JobCard> GetCurrentCards()
    {
        return currentCards;
    }

    [ContextMenu("DEBUG: 打开工作选择")]
    public void DebugOpenJobSelection()
    {
        OpenJobSelection();
    }

    [ContextMenu("DEBUG: 关闭工作选择")]
    public void DebugCloseJobSelection()
    {
        CloseJobSelection();
    }

    [ContextMenu("DEBUG: 打印容器信息")]
    public void DebugPrintContainerInfo()
    {
        if (cardContainer == null) return;

        Debug.Log($"\n{new string('=', 80)}");
        Debug.Log("★★★ 容器信息 ★★★");
        Debug.Log($"{new string('=', 80)}");
        Debug.Log($"容器位置: {cardContainer.localPosition}");
        Debug.Log($"容器大小: {cardContainer.rect.size}");
        Debug.Log($"卡牌数量: {currentCards.Count}");
        Debug.Log($"容器滑动范围: [{containerMaxX:F1}, {containerMinX:F1}]");
        Debug.Log($"当前目标X: {containerCurrentTargetX:F1}");
        Debug.Log($"允许滚动: {canScroll}");
        Debug.Log($"正在移动: {isMovingToEndPoint}");

        Debug.Log("\n--- 各卡牌位置 ---");
        for (int i = 0; i < currentCards.Count; i++)
        {
            RectTransform cardRect = currentCards[i].GetComponent<RectTransform>();
            Debug.Log($"卡牌 {i}: X = {cardRect.anchoredPosition.x:F1}");
        }
        Debug.Log($"{new string('=', 80)}\n");
    }

    [ContextMenu("DEBUG: 打印卡牌条件信息")]
    public void DebugPrintCardConditions()
    {
        Debug.Log($"\n{new string('=', 80)}");
        Debug.Log("★★★ 卡牌条件信息 ★★★");
        Debug.Log($"{new string('=', 80)}");
        
        if (gameState != null)
        {
            Debug.Log($"玩家技能: {gameState.workSkill}");
            Debug.Log($"玩家健康: {gameState.health}");
        }

        if (timeManager != null)
        {
            Debug.Log($"当前时间段: {timeManager.GetCurrentTimeSlotName()}");
            Debug.Log($"剩余时间: {timeManager.GetCurrentHours()} 小时");
        }

        Debug.Log($"\n--- 各卡牌条件 ---");
        for (int i = 0; i < currentCards.Count; i++)
        {
            JobCard card = currentCards[i];
            JobData job = card.GetJobData();
            
            Debug.Log($"\n卡牌 {i}: {job.jobName}");
            Debug.Log($"  可选: {card.IsSelectable()}");
            Debug.Log($"  技能要求: {job.requiredSkill}");
            Debug.Log($"  健康消耗: {job.healthCost}");
            Debug.Log($"  时间段: {job.timeSlot}");
        }
        Debug.Log($"{new string('=', 80)}\n");
    }

    [ContextMenu("DEBUG: 切换调试模式")]
    public void DebugToggleDebugMode()
    {
        debugMode = !debugMode;
        Debug.Log($"[DEBUG] 模式: {(debugMode ? "ON" : "OFF")}");
    }
}