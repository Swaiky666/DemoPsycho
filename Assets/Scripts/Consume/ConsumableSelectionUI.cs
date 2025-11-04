using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// 消费物品选择 UI 系统 - 测试版
/// 禁用自动关闭逻辑以测试卡牌生成
/// </summary>
public class ConsumableSelectionUI : MonoBehaviour
{
    [Header("UI 引用")]
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private RectTransform cardContainer;
    [SerializeField] private GameObject consumableSelectionPanel;

    [Header("卡牌 Prefab")]
    [SerializeField] private ConsumableCard consumableCardPrefab;

    [Header("布局设置")]
    [SerializeField] private float cardWidth = 350f;
    [SerializeField] private float cardSpacing = 30f;
    [SerializeField] private float cardStartX = 0f;

    [Header("动画设置 - 进入动画")]
    [SerializeField] private float slideInDuration = 0.5f;

    [Header("动画设置 - 容器移动")]
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private float moveToEndPointDuration = 1f;
    [SerializeField] private AnimationCurve moveAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("鼠标滚动设置")]
    [SerializeField] private float edgeThreshold = 200f;
    [SerializeField] private float scrollSpeedMax = 300f;
    [SerializeField] private float scrollSmoothTime = 0.1f;
    [SerializeField] private bool debugMode = false;

    [Header("系统引用")]
    [SerializeField] private RaycastButton consumeButton;
    [SerializeField] private ConsumeSystem consumeSystem;
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private AffectGameState gameState;
    [SerializeField] private PlayerStatsDisplay statsDisplay;

    private List<ConsumableCard> currentCards = new List<ConsumableCard>();
    private bool isUIActive = false;
    private bool isMovingToEndPoint = false;
    private bool canScroll = false;

    private float containerMinX;
    private float containerMaxX;
    private float containerCurrentTargetX;
    private float containerVelocity;

    void Start()
    {
        if (consumeButton == null)
            consumeButton = FindObjectOfType<RaycastButton>();
        if (consumeSystem == null)
            consumeSystem = FindObjectOfType<ConsumeSystem>();
        if (timeManager == null)
            timeManager = FindObjectOfType<TimeManager>();
        if (gameState == null)
            gameState = FindObjectOfType<AffectGameState>();
        if (statsDisplay == null)
            statsDisplay = FindObjectOfType<PlayerStatsDisplay>();
        if (mainCanvas == null)
            mainCanvas = FindObjectOfType<Canvas>();

        if (consumableSelectionPanel != null)
        {
            consumableSelectionPanel.SetActive(false);
        }

        if (consumeButton != null)
        {
            RaycastButton.OnButtonClicked += OnConsumeButtonClicked;
        }

        ConsumableCard.OnCardSelected += OnConsumableCardSelected;

        Debug.Log("[ConsumableSelectionUI] 系统初始化完成");
    }

    void OnDestroy()
    {
        if (consumeButton != null)
        {
            RaycastButton.OnButtonClicked -= OnConsumeButtonClicked;
        }

        ConsumableCard.OnCardSelected -= OnConsumableCardSelected;
    }

    void Update()
    {
        // 按 ESC 关闭
        if (Input.GetKeyDown(KeyCode.Escape) && isUIActive)
        {
            CloseConsumableSelection();
        }

        // ⭐ 禁用自动关闭逻辑 - 注释掉以测试
        /*
        if (isUIActive && Input.GetMouseButtonDown(0))
        {
            if (!IsPointerOverCard())
            {
                CloseConsumableSelection();
            }
        }
        */

        if (canScroll && !isMovingToEndPoint)
        {
            HandleMouseDistanceScroll();
        }

        if (debugMode && canScroll && mainCanvas.worldCamera != null)
        {
            DebugDrawBoundary();
        }
    }

    private void HandleMouseDistanceScroll()
    {
        float screenCenterX = Screen.width * 0.5f;
        float mouseX = Input.mousePosition.x;
        float distanceFromCenter = Mathf.Abs(mouseX - screenCenterX);

        float scrollDelta = 0f;

        if (distanceFromCenter > edgeThreshold)
        {
            float excessDistance = distanceFromCenter - edgeThreshold;
            float screenHalfWidth = Screen.width * 0.5f;
            float distanceRatio = excessDistance / screenHalfWidth;
            float currentSpeed = scrollSpeedMax * distanceRatio;

            if (mouseX < screenCenterX)
            {
                scrollDelta = currentSpeed;
            }
            else
            {
                scrollDelta = -currentSpeed;
            }

            containerVelocity = scrollDelta;
        }
        else
        {
            containerVelocity = 0f;
        }

        containerCurrentTargetX += containerVelocity * Time.deltaTime;
        containerCurrentTargetX = Mathf.Clamp(containerCurrentTargetX, containerMaxX, containerMinX);

        Vector3 containerPos = cardContainer.localPosition;
        containerPos.x = Mathf.Lerp(containerPos.x, containerCurrentTargetX, Time.deltaTime / scrollSmoothTime);
        cardContainer.localPosition = containerPos;
    }

    private void DebugDrawBoundary()
    {
        float screenCenterX = Screen.width * 0.5f;
        float leftBound = screenCenterX - edgeThreshold;
        float rightBound = screenCenterX + edgeThreshold;

        Debug.DrawLine(
            mainCanvas.worldCamera.ScreenToWorldPoint(new Vector3(leftBound, 0, 10)),
            mainCanvas.worldCamera.ScreenToWorldPoint(new Vector3(leftBound, Screen.height, 10)),
            Color.yellow
        );

        Debug.DrawLine(
            mainCanvas.worldCamera.ScreenToWorldPoint(new Vector3(rightBound, 0, 10)),
            mainCanvas.worldCamera.ScreenToWorldPoint(new Vector3(rightBound, Screen.height, 10)),
            Color.yellow
        );

        Debug.DrawLine(
            mainCanvas.worldCamera.ScreenToWorldPoint(new Vector3(screenCenterX, 0, 10)),
            mainCanvas.worldCamera.ScreenToWorldPoint(new Vector3(screenCenterX, Screen.height, 10)),
            Color.cyan
        );

        float mouseX = Input.mousePosition.x;
        Debug.DrawLine(
            mainCanvas.worldCamera.ScreenToWorldPoint(new Vector3(mouseX, 0, 10)),
            mainCanvas.worldCamera.ScreenToWorldPoint(new Vector3(mouseX, Screen.height, 10)),
            Color.red
        );
    }

    private void OnConsumeButtonClicked(RaycastButton button)
    {
        if (button.GetButtonType() != RaycastButton.ButtonType.Consume)
            return;

        Debug.Log("[ConsumableSelectionUI] 消费按钮被点击");
        OpenConsumableSelection();
    }

    private void OpenConsumableSelection()
    {
        if (isUIActive) return;

        isUIActive = true;

        if (consumableSelectionPanel != null)
        {
            consumableSelectionPanel.SetActive(true);
            Debug.Log("[ConsumableSelectionUI] Panel 已激活");
        }

        GenerateConsumableCards();

        Debug.Log("[ConsumableSelectionUI] 消费选择界面已打开");
    }

    public void CloseConsumableSelection()
    {
        if (!isUIActive) return;

        isUIActive = false;
        canScroll = false;

        DestroyAllCards();

        if (consumableSelectionPanel != null)
        {
            consumableSelectionPanel.SetActive(false);
        }

        Debug.Log("[ConsumableSelectionUI] 消费选择界面已关闭");
    }

    private void GenerateConsumableCards()
    {
        Debug.Log("\n" + new string('=', 80));
        Debug.Log("★★★ [GenerateConsumableCards] 开始生成卡牌 ★★★");
        Debug.Log(new string('=', 80));

        List<ConsumableItem> allItems = consumeSystem.GetAllItems();
        Debug.Log($"✓ 获得 {allItems.Count} 个物品");

        Vector3 initialPosition = startPoint != null ? startPoint.localPosition : Vector3.zero;
        cardContainer.localPosition = initialPosition;
        containerCurrentTargetX = initialPosition.x;
        containerVelocity = 0f;
        isMovingToEndPoint = false;
        canScroll = false;
        Debug.Log($"✓ 容器位置: {initialPosition}");

        float totalWidth = 0f;
        for (int i = 0; i < allItems.Count; i++)
        {
            totalWidth += cardWidth + cardSpacing;
        }
        totalWidth -= cardSpacing;

        Debug.Log($"\n--- 开始生成 {allItems.Count} 张卡牌 ---");
        float currentXPosition = cardStartX;

        for (int i = 0; i < allItems.Count; i++)
        {
            ConsumableItem item = allItems[i];

            bool hasGold = gameState != null && gameState.res.gold >= item.cost;
            bool hasTime = timeManager != null && timeManager.HasEnoughTime(item.timeRequired);
            bool isAvailable = hasGold && hasTime;

            ConsumableCard card = Instantiate(consumableCardPrefab, cardContainer);
            Debug.Log($"✓ 卡牌已生成: {item.itemId}");
            
            card.SetItemData(item, isAvailable);
            card.SetConditions(hasGold, hasTime);

            RectTransform cardRect = card.GetComponent<RectTransform>();
            cardRect.anchoredPosition = new Vector2(currentXPosition, 0);
            Debug.Log($"  └─ 位置: ({currentXPosition}, 0)");

            card.PlayEnterAnimation();
            currentCards.Add(card);

            currentXPosition += cardWidth + cardSpacing;
            
            if (!isAvailable)
            {
                string reason = "";
                if (!hasGold) reason += "金币不足 ";
                if (!hasTime) reason += "时间不足 ";
                Debug.Log($"[卡牌] {item.itemId} - 不可选: {reason.Trim()}");
            }
            else
            {
                Debug.Log($"[卡牌] {item.itemId} - ✓ 完全可选");
            }
        }

        Debug.Log($"✓ 所有卡牌生成完成，共 {currentCards.Count} 张");
        Debug.Log($"✓ Hierarchy 中 currentCards 数量: {currentCards.Count}");

        CalculateContainerBounds(totalWidth);

        StartCoroutine(SequenceAfterGeneration());
        
        Debug.Log("\n" + new string('=', 80));
        Debug.Log("★★★ [GenerateConsumableCards] 卡牌生成完成 ★★★");
        Debug.Log(new string('=', 80) + "\n");
    }

    private IEnumerator SequenceAfterGeneration()
    {
        yield return new WaitForSeconds(slideInDuration);
        Debug.Log("[Sequence] ✓ 卡牌进入动画完成");

        if (endPoint != null)
        {
            yield return StartCoroutine(MoveContainerToEndPoint());
            Debug.Log("[Sequence] ✓ 容器已到达endPoint");
        }
        else
        {
            Debug.LogWarning("[Sequence] ⚠️ 没有设置 EndPoint");
        }

        containerCurrentTargetX = cardContainer.localPosition.x;
        Debug.Log($"[Sequence] ✓ 同步 containerCurrentTargetX = {containerCurrentTargetX}");

        canScroll = true;
        Debug.Log("[Sequence] ✅ 启用自动滚动");
    }

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

    private void DestroyAllCards()
    {
        Debug.Log($"[DestroyAllCards] 开始销毁 {currentCards.Count} 张卡牌");
        
        foreach (ConsumableCard card in currentCards)
        {
            if (card != null)
            {
                card.PlayExitAnimation();
            }
        }

        currentCards.Clear();
        Debug.Log("[DestroyAllCards] 销毁完成");
    }

    private void OnConsumableCardSelected(ConsumableItem item)
    {
        Debug.Log($"[ConsumableSelectionUI] 选择了物品: {item.itemId}");

        if (consumeSystem == null) return;

        consumeSystem.UseItem(item.itemId);

        if (statsDisplay != null)
        {
            statsDisplay.UpdateAllDisplays();
        }

        Invoke(nameof(CloseConsumableSelection), 0.5f);
    }

    private bool IsPointerOverCard()
    {
        foreach (ConsumableCard card in currentCards)
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

    public List<ConsumableCard> GetCurrentCards()
    {
        return currentCards;
    }
}