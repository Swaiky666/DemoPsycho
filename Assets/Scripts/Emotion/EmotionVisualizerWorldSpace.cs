using UnityEngine;
using TMPro;

/// <summary>
/// 完整的极坐标情绪可视化系统 - 支持中英文切换
/// 调试版本：带有详细日志输出以追踪问题
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
    
    [Header("Emotion State - Inspector Debug")]
    [SerializeField] private float valence = 0.5f;
    [SerializeField] private float activation = 0.5f;
    
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

    private void Start()
    {
        if (debugMode) Debug.Log("[EmotionVisualizer] Start() 开始执行");
        
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
    }

    private void OnDestroy()
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged -= OnLanguageChanged;
        }
    }

    private void Update()
    {
        UpdateCurrentPoint();
    }

    /// <summary>
    /// 语言改变回调 - 更新所有标签显示
    /// </summary>
    private void OnLanguageChanged(LocalizationConfig.Language language)
    {
        if (debugMode) Debug.Log($"[EmotionVisualizer] OnLanguageChanged 被调用，新语言: {language}");
        
        // 更新扇区标签
        UpdateSectorLabelsText();
        
        // 更新分类标签
        UpdateCategoryLabelsText();
        
        // 更新当前情绪点的文字
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
                // 使用 GetDisplayName() 获取本地化名称
                tmp.text = sector.GetDisplayName();
            }
            
            childIndex++;
        }
    }

    /// <summary>
    /// 更新所有分类标签的文本
    /// 修复版本：直接更新存储的 TextMeshPro 引用
    /// </summary>
    private void UpdateCategoryLabelsText()
    {
        if (debugMode) Debug.Log($"[EmotionVisualizer] UpdateCategoryLabelsText 开始，当前标签列表数量: {categoryLabelTexts?.Count ?? -1}");
        
        if (categoryLabelTexts == null)
        {
            Debug.LogError("[EmotionVisualizer] categoryLabelTexts 为 null！");
            return;
        }
        
        if (categoryLabelTexts.Count == 0)
        {
            Debug.LogWarning("[EmotionVisualizer] categoryLabelTexts 为空！");
            return;
        }
        
        var categoryData = new[]
        {
            new { name = "ACTIVATION", key = "emotion_category_activation" },
            new { name = "PLEASANT", key = "emotion_category_pleasant" },
            new { name = "DEACTIVATION", key = "emotion_category_deactivation" },
            new { name = "UNPLEASANT", key = "emotion_category_unpleasant" }
        };

        // 更新所有存储的分类标签
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
            
            if (debugMode) Debug.Log($"[EmotionVisualizer] 标签 {i}: '{oldText}' → '{localizedText}' (key: {categoryData[i].key})");
        }
    }

    /// <summary>
    /// 获取本地化的分类名称
    /// 修复版本：正确处理 key 未找到的情况
    /// </summary>
    private string GetLocalizedCategoryName(string key, string defaultName)
    {
        if (LocalizationManager.Instance != null)
        {
            string result = LocalizationManager.Instance.GetString(key);
            
            if (debugMode) Debug.Log($"[EmotionVisualizer] GetString('{key}') 返回: '{result}'");
            
            // 如果返回值等于 key，说明没有找到本地化，使用默认值
            if (result != key && !string.IsNullOrEmpty(result))
            {
                if (debugMode) Debug.Log($"[EmotionVisualizer] 本地化成功: {result}");
                return result;
            }
        }
        
        // 降级：返回默认名称
        if (debugMode) Debug.Log($"[EmotionVisualizer] 本地化失败，使用默认值: {defaultName}");
        return defaultName;
    }

    /// <summary>
    /// 更新当前情绪点的文字
    /// </summary>
    private void UpdateEmotionPointText()
    {
        if (emotionPointText != null && classifier != null)
        {
            EmotionClassifier.Result result = classifier.GetCurrentResult();
            emotionPointText.text = result.displayName;
        }
    }

    /// <summary>
    /// 创建坐标系统（使用本地坐标）
    /// </summary>
    private void CreateCoordinateSystem()
    {
        horizontalLine = CreateLineRenderer("HorizontalLine", xAxisColor);
        horizontalLine.SetPositions(new[] {
            new Vector3(-gridSize * 0.5f, 0, 0),
            new Vector3(gridSize * 0.5f, 0, 0)
        });

        verticalLine = CreateLineRenderer("VerticalLine", zAxisColor);
        verticalLine.SetPositions(new[] {
            new Vector3(0, 0, -gridSize * 0.5f),
            new Vector3(0, 0, gridSize * 0.5f)
        });

        circleLine = CreateLineRenderer("CircleBoundary", new Color(0.7f, 0.7f, 0.7f));
        DrawCircleTopDown(circleLine, gridSize * 0.5f, 64);

        safeZoneCircle = CreateLineRenderer("SafeZoneCircle", safeZoneColor);
        DrawCircleTopDown(safeZoneCircle, safeZoneRadius, 64);

        if (currentPointPrefab != null)
        {
            currentPointInstance = Instantiate(currentPointPrefab, transform);
            currentPointInstance.name = "CurrentPoint";
            currentPointInstance.transform.localPosition = new Vector3(0, 0.1f, 0);
            
            CreateEmotionPointText();
        }
        else
        {
            Debug.LogWarning("CurrentPointPrefab is not assigned!");
        }
        
        radiusLine = CreateLineRenderer("RadiusLine", new Color(1f, 1f, 0f, 0.7f));
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
        
        // 应用自定义字体
        if (customFont != null)
        {
            emotionPointText.font = customFont;
        }

        RectTransform rt = emotionTextObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(2, 1);
    }

    /// <summary>
    /// 创建 LineRenderer（使用本地坐标）
    /// </summary>
    private LineRenderer CreateLineRenderer(string name, Color color)
    {
        GameObject lineObj = new GameObject(name);
        lineObj.transform.SetParent(transform);
        lineObj.transform.localPosition = Vector3.zero;
        lineObj.transform.localRotation = Quaternion.identity;

        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.material = lineMaterial ?? new Material(Shader.Find("Sprites/Default"));
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.sortingOrder = -1;
        
        // 使用本地空间坐标
        lr.useWorldSpace = false;

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
        if (sectorConfig == null || sectorConfig.sectors == null || sectorConfig.sectors.Length == 0)
        {
            Debug.LogWarning("EmotionVisualizerWorldSpace: No sectorConfig or sectors defined!");
            return;
        }

        GameObject containerObj = new GameObject("SectorLabels");
        containerObj.transform.SetParent(transform);
        containerObj.transform.localPosition = Vector3.zero;
        sectorLabelsContainer = containerObj.transform;

        foreach (var sector in sectorConfig.sectors)
        {
            CreateSectorLabel(sector);
        }
    }

    /// <summary>
    /// 创建单个扇区标签
    /// </summary>
    private void CreateSectorLabel(EmotionSectorsConfig.Sector sector)
    {
        GameObject labelObj = new GameObject($"Sector_{sector.id}");
        labelObj.transform.SetParent(sectorLabelsContainer);

        float centerAngleDeg = sector.centerDeg;
        float centerAngleRad = centerAngleDeg * Mathf.Deg2Rad;
        
        float labelRadius = gridSize * 0.35f;
        float x = Mathf.Cos(centerAngleRad) * labelRadius;
        float z = Mathf.Sin(centerAngleRad) * labelRadius;
        
        labelObj.transform.localPosition = new Vector3(x, 0.1f, z);
        labelObj.transform.localRotation = Quaternion.Euler(90, 0, 0);

        TextMeshPro tmp = labelObj.AddComponent<TextMeshPro>();
        // 使用 GetDisplayName() 获取本地化名称
        tmp.text = sector.GetDisplayName();
        tmp.fontSize = 4f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = sector.uiColor;
        
        // 应用自定义字体
        if (customFont != null)
        {
            tmp.font = customFont;
        }

        RectTransform rt = labelObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(2, 1);
    }

    /// <summary>
    /// 创建4个分类标签 - 支持中英文切换
    /// 修复版本：存储 TextMeshPro 引用以便动态更新
    /// </summary>
    private void CreateCategoryLabels()
    {
        if (debugMode) Debug.Log("[EmotionVisualizer] CreateCategoryLabels 开始");
        
        GameObject containerObj = new GameObject("CategoryLabels");
        containerObj.transform.SetParent(transform);
        containerObj.transform.localPosition = Vector3.zero;
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
            
            // 获取本地化的分类名称
            string localizedText = GetLocalizedCategoryName(category.key, category.name);
            tmp.text = localizedText;
            
            if (debugMode) Debug.Log($"[EmotionVisualizer] 创建分类标签 {categoryIndex}: '{localizedText}'");
            
            tmp.fontSize = categoryTextFontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = Color.white;
            tmp.overflowMode = TextOverflowModes.Overflow;
            
            // 应用自定义字体
            if (customFont != null)
            {
                tmp.font = customFont;
            }

            RectTransform rt = labelObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(4, 2);
            
            // 存储引用以便后续动态更新
            categoryLabelTexts.Add(tmp);
            if (debugMode) Debug.Log($"[EmotionVisualizer] 已添加标签到列表，当前列表大小: {categoryLabelTexts.Count}");
            
            categoryIndex++;
        }
        
        if (debugMode) Debug.Log($"[EmotionVisualizer] CreateCategoryLabels 完成，共创建 {categoryIndex} 个标签");
    }

    /// <summary>
    /// 更新当前点和极坐标显示（使用本地坐标）
    /// </summary>
    private void UpdateCurrentPoint()
    {
        if (classifier != null)
        {
            classifier.currentV = valence - 0.5f;
            classifier.currentA = activation - 0.5f;
        }

        float x = (valence - 0.5f) * gridSize;
        float z = (activation - 0.5f) * gridSize;

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
                "{8}: {9}\n{10}: {11}",
                vLabel, classifier.currentV,
                aLabel, classifier.currentA,
                rLabel, classifyResult.radius,
                angleLabel, classifyResult.angleDeg,
                intensityLabel, emotionIntensityLabel,
                sectorLabel, classifyResult.displayName
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
    /// 修复版本：正确处理 key 未找到的情况
    /// </summary>
    private string GetLocalizedDebugLabel(string key, string defaultValue)
    {
        if (LocalizationManager.Instance != null)
        {
            string result = LocalizationManager.Instance.GetString(key);
            
            // 如果返回值等于 key，说明没有找到本地化
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
        Debug.Log($"Current coordinates: V={valence:F2}, A={activation:F2} -> Emotion: {result.displayName} [{result.intensity}]");
    }

    /// <summary>
    /// 外部接口：动态设置坐标
    /// </summary>
    public void SetCoordinates(float newValence, float newActivation)
    {
        valence = Mathf.Clamp01(newValence);
        activation = Mathf.Clamp01(newActivation);
    }
    
    /// <summary>
    /// 调试菜单：手动测试分类标签更新
    /// </summary>
    [ContextMenu("DEBUG: 手动触发分类标签更新")]
    public void DebugUpdateCategoryLabels()
    {
        Debug.Log("[EmotionVisualizer] 开始手动测试分类标签更新");
        UpdateCategoryLabelsText();
        Debug.Log("[EmotionVisualizer] 手动测试完成");
    }
}