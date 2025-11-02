using UnityEngine;
using TMPro;

/// <summary>
/// 完整的极坐标情绪可视化系统
/// 使用 EmotionClassifier 进行极坐标转换和扇区分类
/// 所有颜色和标签从 EmotionSectorsConfig 读取
/// </summary>
public class EmotionVisualizerWorldSpace : MonoBehaviour
{
    [Header("Visualization Settings")]
    [SerializeField] private float gridSize = 10f;
    [SerializeField] private Material lineMaterial;
    [SerializeField] private float lineWidth = 0.05f;
    [SerializeField] private float currentPointWidth = 0.2f; // 当前点大小（可在Inspector中调整）
    
    [Header("Coordinate Axes Colors")]
    [SerializeField] private Color xAxisColor = Color.white; // X轴（Valence左右）颜色
    [SerializeField] private Color zAxisColor = Color.white; // Z轴（Activation前后）颜色
    
    [Header("Emotion State - Inspector Debug")]
    [SerializeField] private float valence = 0.5f; // 0-1: 不愉快 -> 愉快
    [SerializeField] private float activation = 0.5f; // 0-1: 低激活 -> 高激活
    
    [Header("Display")]
    [SerializeField] private TextMeshProUGUI emotionText;
    [SerializeField] private TextMeshProUGUI debugText;
    
    [Header("Category Label Settings")]
    [SerializeField] private float categoryTextFontSize = 3f; // 分类文字大小（可在Inspector中调整）
    
    [Header("Config & Classifier")]
    [SerializeField] private EmotionSectorsConfig sectorConfig;
    private EmotionClassifier classifier;
    
    [Header("Current Point Prefab")]
    [SerializeField] private GameObject currentPointPrefab; // 当前点的Prefab（球体）
    private GameObject currentPointInstance; // 当前点的实例
    
    [Header("Emotion Text Above Point")]
    [SerializeField] private float emotionTextHeight = 0.5f; // 文字离球体的高度
    [SerializeField] private float emotionTextFontSize = 4f; // 文字大小
    private TextMeshPro emotionPointText; // 球体上方的情绪文字
    
    [Header("Safe Zone Circle")]
    [SerializeField] private float safeZoneRadius = 2f; // 安全区域半径
    [SerializeField] private Color safeZoneColor = new Color(0, 1, 0, 0.5f); // 安全区域颜色（绿色半透明）
    private LineRenderer safeZoneCircle; // 安全区域圆圈
    
    // Line Renderers
    private LineRenderer horizontalLine;
    private LineRenderer verticalLine;
    private LineRenderer circleLine;
    private LineRenderer radiusLine;
    
    // Labels containers
    private Transform sectorLabelsContainer;
    private Transform categoryLabelsContainer;

    private void Start()
    {
        // 创建自定义材质
        if (lineMaterial == null)
        {
            lineMaterial = new Material(Shader.Find("Sprites/Default"));
            if (lineMaterial != null)
            {
                lineMaterial.color = Color.white;
            }
        }
        
        // 初始化分类器
        if (classifier == null)
        {
            classifier = gameObject.AddComponent<EmotionClassifier>();
            classifier.config = sectorConfig;
        }

        CreateCoordinateSystem();
        CreateSectorLabels();
        CreateCategoryLabels();
        UpdateCurrentPoint();
    }

    private void Update()
    {
        UpdateCurrentPoint();
    }

    /// <summary>
    /// 创建坐标系统（红绿线 + 圆形边界 + 安全区域圆）
    /// </summary>
    private void CreateCoordinateSystem()
    {
        // X轴 - Valence轴 (使用可配置颜色)
        horizontalLine = CreateLineRenderer("HorizontalLine", xAxisColor);
        horizontalLine.SetPositions(new[] {
            new Vector3(-gridSize * 0.5f, 0, 0),
            new Vector3(gridSize * 0.5f, 0, 0)
        });

        // Z轴 - Activation轴 (使用可配置颜色)
        verticalLine = CreateLineRenderer("VerticalLine", zAxisColor);
        verticalLine.SetPositions(new[] {
            new Vector3(0, 0, -gridSize * 0.5f),
            new Vector3(0, 0, gridSize * 0.5f)
        });

        // 圆形边界 (灰色)
        circleLine = CreateLineRenderer("CircleBoundary", new Color(0.7f, 0.7f, 0.7f));
        DrawCircleTopDown(circleLine, gridSize * 0.5f, 64);

        // 安全区域圆 (绿色) - 相对于GameObject位置
        safeZoneCircle = CreateLineRenderer("SafeZoneCircle", safeZoneColor);
        DrawCircleTopDown(safeZoneCircle, safeZoneRadius, 64);

        // 创建当前点Prefab的实例
        if (currentPointPrefab != null)
        {
            currentPointInstance = Instantiate(currentPointPrefab, transform);
            currentPointInstance.name = "CurrentPoint";
            currentPointInstance.transform.localPosition = new Vector3(0, 0.1f, 0); // 初始位置在中心
            
            // 创建文字对象显示情绪名称
            CreateEmotionPointText();
        }
        else
        {
            Debug.LogWarning("CurrentPointPrefab is not assigned! Please assign a prefab (e.g., a Sphere) to the 'Current Point Prefab' field.");
        }
        
        // 半径线 (初始为黄色)
        radiusLine = CreateLineRenderer("RadiusLine", new Color(1f, 1f, 0f, 0.7f));
        radiusLine.startWidth = lineWidth * 0.5f;
        radiusLine.endWidth = lineWidth * 0.5f;
    }

    /// <summary>
    /// 创建LineRenderer
    /// </summary>
    private LineRenderer CreateLineRenderer(string name, Color color)
    {
        GameObject lineObj = new GameObject(name);
        lineObj.transform.SetParent(transform);
        lineObj.transform.localPosition = Vector3.zero;

        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        
        // 设置材质
        if (lineMaterial != null)
        {
            lr.material = lineMaterial;
        }
        else
        {
            lr.material = new Material(Shader.Find("Sprites/Default"));
        }
        
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.sortingOrder = 32767;
        lr.useWorldSpace = false; // 改为false：所有圈都相对于GameObject位置显示
        lr.receiveShadows = false;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        
        // 确保可以看到
        lr.enabled = true;
        
        return lr;
    }

    /// <summary>
    /// 创建球体上方的情绪文字
    /// </summary>
    private void CreateEmotionPointText()
    {
        // 创建文字对象
        GameObject textObj = new GameObject("EmotionPointText");
        textObj.transform.SetParent(currentPointInstance.transform);
        textObj.transform.localPosition = new Vector3(0, emotionTextHeight, 0);
        textObj.transform.localRotation = Quaternion.Euler(90, 0, 0); // 面朝上

        // 添加 TextMeshPro 组件
        emotionPointText = textObj.AddComponent<TextMeshPro>();
        emotionPointText.text = "Neutral";
        emotionPointText.fontSize = emotionTextFontSize;
        emotionPointText.alignment = TextAlignmentOptions.Center;
        emotionPointText.color = Color.white;

        // 设置 RectTransform
        RectTransform rt = textObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(2, 1);
    }
    private void DrawCircleTopDown(LineRenderer lr, float radius, int segments)
    {
        Vector3[] positions = new Vector3[segments + 1];
        float angleStep = 360f / segments;

        for (int i = 0; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            positions[i] = new Vector3(
                Mathf.Cos(angle) * radius,
                0,
                Mathf.Sin(angle) * radius
            );
        }

        lr.positionCount = segments + 1;
        lr.SetPositions(positions);
    }

    /// <summary>
    /// 创建所有扇区标签（从Config中的每个Sector）
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

        // 为每个扇区创建一个标签
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

        // 根据中心角度和半径计算标签位置
        float centerAngleDeg = sector.centerDeg;
        float centerAngleRad = centerAngleDeg * Mathf.Deg2Rad;
        
        // 标签放在距离原点 gridSize * 0.35 处（内圈）
        float labelRadius = gridSize * 0.35f;
        float x = Mathf.Cos(centerAngleRad) * labelRadius;
        float z = Mathf.Sin(centerAngleRad) * labelRadius;
        
        labelObj.transform.localPosition = new Vector3(x, 0.1f, z);
        labelObj.transform.localRotation = Quaternion.Euler(90, 0, 0);

        // 创建TextMeshPro显示文本
        TextMeshPro tmp = labelObj.AddComponent<TextMeshPro>();
        tmp.text = sector.GetDisplayName();
        tmp.fontSize = 4f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = sector.uiColor;

        RectTransform rt = labelObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(2, 1);
    }

    /// <summary>
    /// 创建4个分类标签 (ACTIVATION, PLEASANT, DEACTIVATION, UNPLEASANT)
    /// </summary>
    private void CreateCategoryLabels()
    {
        GameObject containerObj = new GameObject("CategoryLabels");
        containerObj.transform.SetParent(transform);
        containerObj.transform.localPosition = Vector3.zero;
        categoryLabelsContainer = containerObj.transform;

        // 4个分类标签的位置（圆周上的4个方向）
        var categoryData = new[]
        {
            new { name = "ACTIVATION", angle = 90f },      // 上
            new { name = "PLEASANT", angle = 0f },         // 右
            new { name = "DEACTIVATION", angle = 270f },   // 下
            new { name = "UNPLEASANT", angle = 180f }      // 左
        };

        foreach (var category in categoryData)
        {
            GameObject labelObj = new GameObject($"Category_{category.name}");
            labelObj.transform.SetParent(categoryLabelsContainer);

            // 放在外圈（gridSize * 0.55）
            float labelRadius = gridSize * 0.55f;
            float angleRad = category.angle * Mathf.Deg2Rad;
            float x = Mathf.Cos(angleRad) * labelRadius;
            float z = Mathf.Sin(angleRad) * labelRadius;
            
            labelObj.transform.localPosition = new Vector3(x, 0.15f, z);
            labelObj.transform.localRotation = Quaternion.Euler(90, 0, 0);

            TextMeshPro tmp = labelObj.AddComponent<TextMeshPro>();
            tmp.text = category.name;
            tmp.fontSize = categoryTextFontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = Color.white; // 分类标签默认白色
            tmp.overflowMode = TextOverflowModes.Overflow; // 防止换行，直接显示超出部分

            RectTransform rt = labelObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(4, 2); // 增加宽度确保单行显示
        }
    }

    /// <summary>
    /// 更新当前点和极坐标显示
    /// 俯视角：X轴为Valence（左右），Z轴为Activation（前后）
    /// </summary>
    private void UpdateCurrentPoint()
    {
        // 更新分类器的坐标（转换为相对中心的 [-0.5, 0.5] 范围）
        if (classifier != null)
        {
            classifier.currentV = valence - 0.5f;
            classifier.currentA = activation - 0.5f;
        }

        // 俯视角坐标计算（XZ平面）
        // Valence (0-1) 映射到 X 轴（-gridSize*0.5 到 gridSize*0.5）
        // Activation (0-1) 映射到 Z 轴（-gridSize*0.5 到 gridSize*0.5）
        float x = (valence - 0.5f) * gridSize;
        float z = (activation - 0.5f) * gridSize;

        // 获取分类结果
        EmotionClassifier.Result classifyResult = classifier.GetCurrentResult();

        // 更新当前点Prefab的位置
        if (currentPointInstance != null)
        {
            // 俯视角：点的Y坐标设为0.1（略高于地面，便于可视）
            currentPointInstance.transform.localPosition = new Vector3(x, 0.1f, z);
            
            // 更新点的颜色
            Renderer renderer = currentPointInstance.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = classifyResult.color;
            }

            // 更新情绪文字
            if (emotionPointText != null)
            {
                emotionPointText.text = classifyResult.displayName;
                emotionPointText.color = classifyResult.color; // 文字颜色与点相同
            }
        }
        
        // 绘制半径线（从原点到当前点）
        radiusLine.SetPositions(new[] {
            new Vector3(0, 0.1f, 0),
            new Vector3(x, 0.1f, z)
        });
        
        // 更新情绪显示 UI
        if (emotionText != null)
        {
            emotionText.text = $"{classifyResult.displayName}\n[{classifyResult.intensity}]";
        }
        
        // 更新调试信息显示
        if (debugText != null)
        {
            debugText.text = string.Format(
                "V: {0:F2} | A: {1:F2}\n" +
                "r: {2:F2} | θ: {3:F1}°\n" +
                "Intensity: {4}\nSector: {5}",
                classifier.currentV,
                classifier.currentA,
                classifyResult.radius,
                classifyResult.angleDeg,
                classifyResult.intensity,
                classifyResult.displayName
            );
        }

        // 更新所有标签颜色：当前扇区高亮，其他暗色
        UpdateAllLabelsColor(classifyResult.displayName);
        
        // 更新半径线的颜色
        radiusLine.startColor = classifyResult.color;
        radiusLine.endColor = classifyResult.color;
    }

    /// <summary>
    /// 更新所有标签颜色
    /// </summary>
    private void UpdateAllLabelsColor(string currentSectorId)
    {
        if (sectorLabelsContainer == null) return;

        // 更新所有扇区标签
        foreach (Transform child in sectorLabelsContainer)
        {
            TextMeshPro tmp = child.GetComponent<TextMeshPro>();
            if (tmp != null)
            {
                // 从Config中查找对应的扇区颜色
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
                    tmp.color = new Color(0.5f, 0.5f, 0.5f); // 暗色
                }
            }
        }
    }

    /// <summary>
    /// 在Inspector中调试用的按钮
    /// </summary>
    [ContextMenu("Detect Emotion")]
    public void DetectEmotion()
    {
        EmotionClassifier.Result result = classifier.GetCurrentResult();
        Debug.Log($"当前坐标: V={valence:F2}, A={activation:F2} -> 情绪: {result.displayName} [{result.intensity}]");
    }

    /// <summary>
    /// 外部接口：动态设置坐标
    /// </summary>
    public void SetCoordinates(float newValence, float newActivation)
    {
        valence = Mathf.Clamp01(newValence);
        activation = Mathf.Clamp01(newActivation);
    }
}