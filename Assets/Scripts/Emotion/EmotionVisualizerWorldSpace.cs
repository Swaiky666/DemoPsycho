using UnityEngine;
using TMPro;

/// <summary>
/// 完整的极坐标情绪可视化系统 - 支持中英文切换
/// 修复版本：
/// 1. 所有UI线段跟随 transform
/// 2. 坐标倍率可在 Inspector 中调整
/// </summary>
public class EmotionVisualizerWorldSpace : MonoBehaviour
{
    [Header("Visualization Settings")]
    [SerializeField] private float gridSize = 10f;
    [SerializeField] private Material lineMaterial;
    [SerializeField] private float lineWidth = 0.05f;
    
    [Header("Coordinate Axes Colors")]
    [SerializeField] private Color xAxisColor = Color.white;
    [SerializeField] private Color zAxisColor = Color.white;
    
    [Header("Game State Reference")]
    [SerializeField] private AffectGameState gameState;
    
    [Header("Display")]
    [SerializeField] private TextMeshProUGUI emotionText;
    [SerializeField] private TextMeshProUGUI debugText;
    
    [Header("Category Label Settings")]
    [SerializeField] private float categoryTextFontSize = 3f;
    
    [Header("TMPro Font - 中英文兼容字体")]
    [SerializeField] private TMP_FontAsset customFont;
    
    [Header("Config & Classifier")]
    [SerializeField] private EmotionSectorsConfig sectorConfig;
    private EmotionClassifier classifier;
    
    [Header("Current Point Prefab")]
    [SerializeField] private GameObject currentPointPrefab;
    private GameObject currentPointInstance;
    
    [Header("Emotion Text Above Point")]
    [SerializeField] private float emotionTextHeight = 0.5f;
    [SerializeField] private float emotionTextFontSize = 4f;
    private TextMeshPro emotionPointText;
    
    [Header("Safe Zone Circle")]
    [SerializeField] private float safeZoneRadius = 2f;
    [SerializeField] private Color safeZoneColor = new Color(0, 1, 0, 0.5f);
    private LineRenderer safeZoneCircle;
    
    // ✨ 新增：坐标倍率
    [Header("Coordinate Scale")]
    [SerializeField] private float coordinateScale = 0.1f;  // 倍率：0.1 = 1/10 倍
    [Range(0.01f, 1f)] [SerializeField] private float minCoordinateScale = 0.01f;  // 最小倍率
    [Range(0.01f, 1f)] [SerializeField] private float maxCoordinateScale = 1f;     // 最大倍率
    
    // Line Renderers
    private LineRenderer horizontalLine;
    private LineRenderer verticalLine;
    private LineRenderer circleLine;
    private LineRenderer radiusLine;
    
    // Labels containers
    private Transform sectorLabelsContainer;
    private Transform categoryLabelsContainer;
    
    // 分类标签数据 - 用于动态更新
    private System.Collections.Generic.List<TextMeshPro> categoryLabelTexts;
    
    // 调试标志
    [SerializeField] private bool debugMode = true;
    
    // ✨ 新增：UI 容器用于跟随
    private Transform uiContainer;

    private void Start()
    {
        if (debugMode) Debug.Log("[EmotionVisualizer] Start() 开始执行");
        
        // ✨ 新增：创建 UI 容器
        CreateUIContainer();
        
        // 自动查找 gameState
        if (gameState == null)
        {
            gameState = FindObjectOfType<AffectGameState>();
            if (gameState == null)
            {
                Debug.LogError("[EmotionVisualizer] 找不到 AffectGameState！");
                return;
            }
        }
        
        if (lineMaterial == null)
        {
            lineMaterial = new Material(Shader.Find("Sprites/Default"));
            if (lineMaterial != null)
            {
                lineMaterial.color = Color.white;
            }
        }
        
        if (classifier == null)
        {
            classifier = gameObject.AddComponent<EmotionClassifier>();
            classifier.config = sectorConfig;
        }

        categoryLabelTexts = new System.Collections.Generic.List<TextMeshPro>();
        if (debugMode) Debug.Log("[EmotionVisualizer] categoryLabelTexts 初始化完成");

        CreateCoordinateSystem();
        CreateSectorLabels();
        CreateCategoryLabels();
        UpdateCurrentPoint();
        
        if (debugMode) Debug.Log($"[EmotionVisualizer] CreateCategoryLabels 完成，标签数量: {categoryLabelTexts.Count}");
        
        // 订阅语言改变事件
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged += OnLanguageChanged;
            if (debugMode) Debug.Log("[EmotionVisualizer] 成功订阅 OnLanguageChanged 事件");
        }
        else
        {
            Debug.LogError("[EmotionVisualizer] LocalizationManager.Instance 为 null！");
        }
        
        // 订阅 gameState 的效果应用事件
        if (gameState != null)
        {
            gameState.OnEffectApplied += OnGameStateEffectApplied;
            if (debugMode) Debug.Log("[EmotionVisualizer] 成功订阅 gameState.OnEffectApplied 事件");
        }
    }

    private void OnDestroy()
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged -= OnLanguageChanged;
        }
        
        if (gameState != null)
        {
            gameState.OnEffectApplied -= OnGameStateEffectApplied;
        }
    }

    private void Update()
    {
        UpdateCurrentPoint();
    }

    /// <summary>
    /// ✨ 新增：创建 UI 容器用于集中管理
    /// </summary>
    private void CreateUIContainer()
    {
        GameObject containerObj = new GameObject("UIContainer");
        containerObj.transform.SetParent(transform);
        containerObj.transform.localPosition = Vector3.zero;
        containerObj.transform.localRotation = Quaternion.identity;
        uiContainer = containerObj.transform;
        
        if (debugMode) Debug.Log("[EmotionVisualizer] UI 容器已创建");
    }

    /// <summary>
    /// gameState 效果应用时的回调
    /// </summary>
    private void OnGameStateEffectApplied(System.Collections.Generic.List<string> effects)
    {
        if (debugMode)
        {
            Debug.Log($"[EmotionVisualizer] OnGameStateEffectApplied 被调用");
            foreach (var effect in effects)
            {
                Debug.Log($"  • 效果: {effect}");
            }
            Debug.Log($"  • 当前情绪: V={gameState.valence:F2}, A={gameState.arousal:F2}");
        }
    }

    /// <summary>
    /// 语言改变回调 - 更新所有标签显示
    /// </summary>
    private void OnLanguageChanged(LocalizationConfig.Language language)
    {
        if (debugMode) Debug.Log($"[EmotionVisualizer] OnLanguageChanged 被调用，新语言: {language}");
        
        UpdateSectorLabelsText();
        UpdateCategoryLabelsText();
        UpdateEmotionPointText();
        
        if (debugMode) Debug.Log("[EmotionVisualizer] 所有标签更新完成");
    }

    /// <summary>
    /// 更新所有扇区标签的文本
    /// </summary>
    private void UpdateSectorLabelsText()
    {
        if (sectorLabelsContainer == null || sectorConfig == null) return;

        int childIndex = 0;
        foreach (var sector in sectorConfig.sectors)
        {
            if (childIndex >= sectorLabelsContainer.childCount) break;
            
            Transform child = sectorLabelsContainer.GetChild(childIndex);
            TextMeshPro tmp = child.GetComponent<TextMeshPro>();
            
            if (tmp != null)
            {
                tmp.text = sector.GetDisplayName();
            }
            
            childIndex++;
        }
    }

    /// <summary>
    /// 更新所有分类标签的文本
    /// </summary>
    private void UpdateCategoryLabelsText()
    {
        if (debugMode) Debug.Log($"[EmotionVisualizer] UpdateCategoryLabelsText 开始，当前标签列表数量: {categoryLabelTexts?.Count ?? -1}");
        
        if (categoryLabelTexts == null || categoryLabelTexts.Count == 0)
        {
            return;
        }
        
        var categoryData = new[]
        {
            new { name = "ACTIVATION", key = "emotion_category_activation" },
            new { name = "PLEASANT", key = "emotion_category_pleasant" },
            new { name = "DEACTIVATION", key = "emotion_category_deactivation" },
            new { name = "UNPLEASANT", key = "emotion_category_unpleasant" }
        };

        for (int i = 0; i < categoryLabelTexts.Count && i < categoryData.Length; i++)
        {
            TextMeshPro tmp = categoryLabelTexts[i];
            if (tmp == null)
            {
                Debug.LogWarning($"[EmotionVisualizer] categoryLabelTexts[{i}] 为 null");
                continue;
            }
            
            string oldText = tmp.text;
            string localizedText = GetLocalizedCategoryName(categoryData[i].key, categoryData[i].name);
            tmp.text = localizedText;
        }
    }

    /// <summary>
    /// 获取本地化的分类名称
    /// </summary>
    private string GetLocalizedCategoryName(string key, string defaultName)
    {
        if (LocalizationManager.Instance != null)
        {
            string result = LocalizationManager.Instance.GetString(key);
            
            if (result != key && !string.IsNullOrEmpty(result))
            {
                return result;
            }
        }
        
        return defaultName;
    }

    /// <summary>
    /// 更新情绪点上方的文字
    /// </summary>
    private void UpdateEmotionPointText()
    {
        if (emotionPointText == null || classifier == null) return;
        
        EmotionClassifier.Result result = classifier.GetCurrentResult();
        emotionPointText.text = result.displayName;
    }

    /// <summary>
    /// 创建坐标系统
    /// ✨ 改进：所有线段都是 uiContainer 的子对象
    /// </summary>
    private void CreateCoordinateSystem()
    {
        if (debugMode) Debug.Log("[EmotionVisualizer] CreateCoordinateSystem 开始");
        
        // X 轴（Valence 轴）
        horizontalLine = CreateLineRenderer("XAxis", xAxisColor);
        horizontalLine.SetPositions(new[] { 
            new Vector3(-gridSize * 0.5f, 0.05f, 0),
            new Vector3(gridSize * 0.5f, 0.05f, 0)
        });

        // Z 轴（Activation 轴）
        verticalLine = CreateLineRenderer("ZAxis", zAxisColor);
        verticalLine.SetPositions(new[] { 
            new Vector3(0, 0.05f, -gridSize * 0.5f),
            new Vector3(0, 0.05f, gridSize * 0.5f)
        });

        // 圆形轨迹
        circleLine = CreateLineRenderer("CircleLine", new Color(0.5f, 0.5f, 0.5f, 0.5f));
        DrawCircleTopDown(circleLine, gridSize * 0.35f, 32);

        // 安全区圆形
        safeZoneCircle = CreateLineRenderer("SafeZoneCircle", safeZoneColor);
        DrawCircleTopDown(safeZoneCircle, safeZoneRadius, 32);

        // 半径线（会动态更新）
        radiusLine = CreateLineRenderer("RadiusLine", Color.white);

        // 当前点
        if (currentPointPrefab != null)
        {
            currentPointInstance = Instantiate(currentPointPrefab, uiContainer);
            currentPointInstance.name = "CurrentPoint";
            currentPointInstance.transform.localPosition = Vector3.zero;

            CreateEmotionPointText();
        }
        else
        {
            Debug.LogWarning("[EmotionVisualizer] CurrentPointPrefab is not assigned!");
        }

        if (debugMode) Debug.Log("[EmotionVisualizer] CreateCoordinateSystem 完成");
    }

    /// <summary>
    /// 创建情绪点上方的文字
    /// </summary>
    private void CreateEmotionPointText()
    {
        GameObject emotionTextObj = new GameObject("EmotionPointText");
        emotionTextObj.transform.SetParent(currentPointInstance.transform);
        emotionTextObj.transform.localPosition = new Vector3(0, emotionTextHeight, 0);
        emotionTextObj.transform.localRotation = Quaternion.Euler(90, 0, 0);

        emotionPointText = emotionTextObj.AddComponent<TextMeshPro>();
        emotionPointText.text = "Neutral";
        emotionPointText.fontSize = emotionTextFontSize;
        emotionPointText.alignment = TextAlignmentOptions.Center;
        emotionPointText.color = Color.white;
        
        if (customFont != null)
        {
            emotionPointText.font = customFont;
        }

        RectTransform rt = emotionTextObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(2, 1);
    }

    /// <summary>
    /// 创建 LineRenderer
    /// ✨ 改进：所有线段都在 uiContainer 下
    /// </summary>
    private LineRenderer CreateLineRenderer(string name, Color color)
    {
        GameObject lineObj = new GameObject(name);
        lineObj.transform.SetParent(uiContainer);  // ✨ 改为 uiContainer
        lineObj.transform.localPosition = Vector3.zero;
        lineObj.transform.localRotation = Quaternion.identity;

        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.material = lineMaterial ?? new Material(Shader.Find("Sprites/Default"));
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.sortingOrder = -1;
        lr.useWorldSpace = false;  // 使用本地空间坐标

        return lr;
    }

    /// <summary>
    /// 绘制圆形（使用本地坐标）
    /// </summary>
    private void DrawCircleTopDown(LineRenderer lr, float radius, int segments)
    {
        Vector3[] points = new Vector3[segments + 1];
        for (int i = 0; i <= segments; i++)
        {
            float angle = (i / (float)segments) * 2f * Mathf.PI;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            points[i] = new Vector3(x, 0.1f, z);
        }
        lr.positionCount = points.Length;
        lr.SetPositions(points);
    }

    /// <summary>
    /// 创建所有扇区标签
    /// </summary>
    private void CreateSectorLabels()
    {
        if (debugMode) Debug.Log("[EmotionVisualizer] CreateSectorLabels 开始");
        
        GameObject containerObj = new GameObject("SectorLabels");
        containerObj.transform.SetParent(uiContainer);  // ✨ 改为 uiContainer
        containerObj.transform.localPosition = Vector3.zero;
        containerObj.transform.localRotation = Quaternion.identity;
        sectorLabelsContainer = containerObj.transform;

        if (sectorConfig == null || sectorConfig.sectors == null || sectorConfig.sectors.Length == 0)
        {
            Debug.LogWarning("[EmotionVisualizer] 没有设置 SectorConfig 或 Sectors 为空！");
            return;
        }

        int sectorIndex = 0;
        foreach (var sector in sectorConfig.sectors)
        {
            GameObject labelObj = new GameObject($"Sector_{sector.id}");
            labelObj.transform.SetParent(sectorLabelsContainer);

            float labelRadius = gridSize * 0.25f;
            float angleRad = sector.centerDeg * Mathf.Deg2Rad;
            float x = Mathf.Cos(angleRad) * labelRadius;
            float z = Mathf.Sin(angleRad) * labelRadius;
            
            labelObj.transform.localPosition = new Vector3(x, 0.1f, z);
            labelObj.transform.localRotation = Quaternion.Euler(90, 0, 0);

            TextMeshPro tmp = labelObj.AddComponent<TextMeshPro>();
            tmp.text = sector.GetDisplayName();
            tmp.fontSize = 3f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = sector.uiColor;
            tmp.overflowMode = TextOverflowModes.Overflow;
            
            if (customFont != null)
                tmp.font = customFont;

            RectTransform rt = labelObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(3, 1);

            if (debugMode) Debug.Log($"[EmotionVisualizer] 创建扇区标签 {sectorIndex}: {sector.id}");
            sectorIndex++;
        }

        if (debugMode) Debug.Log($"[EmotionVisualizer] CreateSectorLabels 完成，共创建 {sectorIndex} 个标签");
    }

    /// <summary>
    /// 创建分类标签（四个主要方向）
    /// </summary>
    private void CreateCategoryLabels()
    {
        if (debugMode) Debug.Log("[EmotionVisualizer] CreateCategoryLabels 开始");
        
        GameObject containerObj = new GameObject("CategoryLabels");
        containerObj.transform.SetParent(uiContainer);  // ✨ 改为 uiContainer
        containerObj.transform.localPosition = Vector3.zero;
        containerObj.transform.localRotation = Quaternion.identity;
        categoryLabelsContainer = containerObj.transform;

        var categoryData = new[]
        {
            new { name = "ACTIVATION", angle = 90f, key = "emotion_category_activation" },
            new { name = "PLEASANT", angle = 0f, key = "emotion_category_pleasant" },
            new { name = "DEACTIVATION", angle = 270f, key = "emotion_category_deactivation" },
            new { name = "UNPLEASANT", angle = 180f, key = "emotion_category_unpleasant" }
        };

        int categoryIndex = 0;
        foreach (var category in categoryData)
        {
            GameObject labelObj = new GameObject($"Category_{category.name}");
            labelObj.transform.SetParent(categoryLabelsContainer);

            float labelRadius = gridSize * 0.55f;
            float angleRad = category.angle * Mathf.Deg2Rad;
            float x = Mathf.Cos(angleRad) * labelRadius;
            float z = Mathf.Sin(angleRad) * labelRadius;
            
            labelObj.transform.localPosition = new Vector3(x, 0.15f, z);
            labelObj.transform.localRotation = Quaternion.Euler(90, 0, 0);

            TextMeshPro tmp = labelObj.AddComponent<TextMeshPro>();
            
            string localizedText = GetLocalizedCategoryName(category.key, category.name);
            tmp.text = localizedText;
            
            tmp.fontSize = categoryTextFontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = Color.white;
            tmp.overflowMode = TextOverflowModes.Overflow;
            
            if (customFont != null)
            {
                tmp.font = customFont;
            }

            RectTransform rt = labelObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(4, 2);
            
            categoryLabelTexts.Add(tmp);
            if (debugMode) Debug.Log($"[EmotionVisualizer] 创建分类标签 {categoryIndex}: '{localizedText}'");
            
            categoryIndex++;
        }
        
        if (debugMode) Debug.Log($"[EmotionVisualizer] CreateCategoryLabels 完成，共创建 {categoryIndex} 个标签");
    }

    /// <summary>
    /// 更新当前点和极坐标显示
    /// ✨ 改进：添加坐标倍率缩放
    /// </summary>
    private void UpdateCurrentPoint()
    {
        if (classifier == null) return;
        
        // 从 gameState 获取实时坐标
        float valence = 0f;
        float arousal = 0f;
        
        if (gameState != null)
        {
            valence = gameState.valence;
            arousal = gameState.arousal;
        }
        
        // ✨ 新增：应用坐标倍率
        valence *= coordinateScale;
        arousal *= coordinateScale;
        
        // 更新 classifier 的当前坐标
        classifier.currentV = valence;
        classifier.currentA = arousal;

        // 计算像素位置
        float x = valence * gridSize;
        float z = arousal * gridSize;

        EmotionClassifier.Result classifyResult = classifier.GetCurrentResult();

        if (currentPointInstance != null)
        {
            currentPointInstance.transform.localPosition = new Vector3(x, 0.1f, z);
            
            Renderer renderer = currentPointInstance.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = classifyResult.color;
            }

            if (emotionPointText != null)
            {
                emotionPointText.text = classifyResult.displayName;
                emotionPointText.color = classifyResult.color;
            }
        }
        
        radiusLine.SetPositions(new[] {
            new Vector3(0, 0.1f, 0),
            new Vector3(x, 0.1f, z)
        });
        
        // 更新情绪显示 UI
        if (emotionText != null)
        {
            string intensityLabel = GetLocalizedIntensityLabel(classifyResult.intensity);
            emotionText.text = $"{classifyResult.displayName}\n[{intensityLabel}]";
        }
        
        // 更新调试信息显示
        if (debugText != null)
        {
            string vLabel = GetLocalizedDebugLabel("debug_valence", "V");
            string aLabel = GetLocalizedDebugLabel("debug_arousal", "A");
            string rLabel = GetLocalizedDebugLabel("debug_radius", "r");
            string angleLabel = GetLocalizedDebugLabel("debug_angle", "θ");
            string intensityLabel = GetLocalizedDebugLabel("debug_intensity", "Intensity");
            string sectorLabel = GetLocalizedDebugLabel("debug_sector", "Sector");
            
            string emotionIntensityLabel = GetLocalizedIntensityLabel(classifyResult.intensity);
            
            debugText.text = string.Format(
                "{0}: {1:F2} | {2}: {3:F2}\n" +
                "{4}: {5:F2} | {6}: {7:F1}°\n" +
                "{8}: {9}\n{10}: {11}\n" +
                "Scale: {12:F2}",  // ✨ 新增：显示当前倍率
                vLabel, classifier.currentV,
                aLabel, classifier.currentA,
                rLabel, classifyResult.radius,
                angleLabel, classifyResult.angleDeg,
                intensityLabel, emotionIntensityLabel,
                sectorLabel, classifyResult.displayName,
                coordinateScale
            );
        }

        UpdateAllLabelsColor(classifyResult.displayName);
        
        radiusLine.startColor = classifyResult.color;
        radiusLine.endColor = classifyResult.color;
    }

    /// <summary>
    /// 获取本地化的强度等级标签
    /// </summary>
    private string GetLocalizedIntensityLabel(string intensity)
    {
        string key = $"emotion_intensity_{intensity.ToLower()}";
        return GetLocalizedDebugLabel(key, intensity);
    }

    /// <summary>
    /// 获取本地化的调试标签
    /// </summary>
    private string GetLocalizedDebugLabel(string key, string defaultValue)
    {
        if (LocalizationManager.Instance != null)
        {
            string result = LocalizationManager.Instance.GetString(key);
            
            if (result != key && !string.IsNullOrEmpty(result))
            {
                return result;
            }
        }
        
        return defaultValue;
    }

    /// <summary>
    /// 更新所有标签颜色
    /// </summary>
    private void UpdateAllLabelsColor(string currentSectorId)
    {
        if (sectorLabelsContainer == null) return;

        foreach (Transform child in sectorLabelsContainer)
        {
            TextMeshPro tmp = child.GetComponent<TextMeshPro>();
            if (tmp != null)
            {
                bool isCurrentSector = false;
                foreach (var sector in sectorConfig.sectors)
                {
                    if (sector.GetDisplayName() == currentSectorId)
                    {
                        tmp.color = sector.uiColor;
                        isCurrentSector = true;
                        break;
                    }
                }
                
                if (!isCurrentSector)
                {
                    tmp.color = new Color(0.5f, 0.5f, 0.5f);
                }
            }
        }
    }

    /// <summary>
    /// 调试用的按钮
    /// </summary>
    [ContextMenu("Detect Emotion")]
    public void DetectEmotion()
    {
        EmotionClassifier.Result result = classifier.GetCurrentResult();
        Debug.Log($"Current coordinates: V={classifier.currentV:F2}, A={classifier.currentA:F2} -> Emotion: {result.displayName} [{result.intensity}]");
    }

    /// <summary>
    /// 调试菜单：打印当前情绪坐标
    /// </summary>
    [ContextMenu("DEBUG: 打印当前情绪坐标")]
    public void DebugPrintCoordinates()
    {
        if (gameState != null)
        {
            Debug.Log($"[DEBUG] GameState 情绪坐标: V={gameState.valence:F2}, A={gameState.arousal:F2}");
            Debug.Log($"[DEBUG] 应用倍率 {coordinateScale} 后: V={gameState.valence * coordinateScale:F2}, A={gameState.arousal * coordinateScale:F2}");
        }
        if (classifier != null)
        {
            Debug.Log($"[DEBUG] Classifier 坐标: V={classifier.currentV:F2}, A={classifier.currentA:F2}");
        }
    }

    /// <summary>
    /// 调试菜单：快速设置倍率
    /// </summary>
    [ContextMenu("DEBUG: 设置倍率为 0.1")]
    public void DebugSetScale01()
    {
        coordinateScale = 0.1f;
        Debug.Log($"[DEBUG] 坐标倍率已设置为 {coordinateScale}");
    }

    [ContextMenu("DEBUG: 设置倍率为 0.5")]
    public void DebugSetScale05()
    {
        coordinateScale = 0.5f;
        Debug.Log($"[DEBUG] 坐标倍率已设置为 {coordinateScale}");
    }

    [ContextMenu("DEBUG: 设置倍率为 1.0")]
    public void DebugSetScale10()
    {
        coordinateScale = 1.0f;
        Debug.Log($"[DEBUG] 坐标倍率已设置为 {coordinateScale}");
    }
}