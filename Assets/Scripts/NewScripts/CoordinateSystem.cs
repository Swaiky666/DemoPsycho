using UnityEngine;

public class CoordinateSystem : MonoBehaviour
{
    [Header("Radii Definition (Relative to Center)")]
    public Transform innerPoint;
    public Transform middlePoint;
    public Transform outerPoint;

    [Header("Tracking Target")]
    public Transform targetObject;

    [Header("Debug Info")]
    public string currentRegion = "Outside";
    public float radius1, radius2, radius3;

    void Start()
    {
        UpdateRadii();
    }

    void Update()
    {
        if (targetObject == null) return;
        
        // 核心：将目标的世界坐标转换成当前物体的局部坐标
        // 这样即便物体旋转了，localPos.x 和 localPos.z 依然对应物体的局部左右和前后
        Vector3 localPos = transform.InverseTransformPoint(targetObject.position);
        
        // 在局部空间计算平面距离 (忽略局部 Y 轴高度)
        float distance = new Vector2(localPos.x, localPos.z).magnitude;

        currentRegion = GetRegion(localPos, distance);
    }

    // --- 外部调用接口 ---

    /// <summary>
    /// 根据物体的局部方向移动目标坐标。
    /// x: 局部右方, z: 局部前方
    /// </summary>
    public void MoveTargetLocally(float deltaX, float deltaZ)
    {
        if (targetObject == null) return;

        // 将局部移动向量转换为世界移动向量
        Vector3 localMovement = new Vector3(deltaX, 0, deltaZ);
        Vector3 worldMovement = transform.TransformDirection(localMovement);

        targetObject.position += worldMovement;
    }

    // --- 内部计算逻辑 ---

    public void UpdateRadii()
    {
        // 计算半径时也要使用局部距离，防止父物体缩放或旋转干扰
        if (innerPoint) radius1 = GetLocalXZDistance(innerPoint.position);
        if (middlePoint) radius2 = GetLocalXZDistance(middlePoint.position);
        if (outerPoint) radius3 = GetLocalXZDistance(outerPoint.position);
    }

    string GetRegion(Vector3 localPos, float dist)
    {
        if (dist <= radius1) return "Inner Circle (Region 0)";

        if (dist <= radius2)
        {
            // 使用 Mathf.Atan2 计算局部坐标的角度
            float angle = Mathf.Atan2(localPos.z, localPos.x) * Mathf.Rad2Deg;
            if (angle < 0) angle += 360;
            
            int quadrant = Mathf.FloorToInt(angle / 90f) + 1;
            return $"Middle Ring - Sector {quadrant}";
        }

        if (dist <= radius3)
        {
            float angle = Mathf.Atan2(localPos.z, localPos.x) * Mathf.Rad2Deg;
            angle -= 22.5f; // 顺时针旋转22.5度
            if (angle < 0) angle += 360;

            int sector = Mathf.FloorToInt(angle / 45f) + 1;
            return $"Outer Ring - Sector {sector}";
        }

        return "Outside";
    }

    float GetLocalXZDistance(Vector3 worldPos)
    {
        Vector3 localPos = transform.InverseTransformPoint(worldPos);
        return new Vector2(localPos.x, localPos.z).magnitude;
    }

    private void OnDrawGizmos()
    {
        // 绘制 Gizmos 时也需要考虑旋转
        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix; // 将绘图坐标系切换为物体局部坐标系

        Gizmos.color = Color.yellow;
        DrawWireDisk(Vector3.zero, radius1);
        Gizmos.color = Color.cyan;
        DrawWireDisk(Vector3.zero, radius2);
        Gizmos.color = Color.magenta;
        DrawWireDisk(Vector3.zero, radius3);

        Gizmos.matrix = oldMatrix;
    }

    void DrawWireDisk(Vector3 center, float radius)
    {
        if (radius <= 0) return;
        int segments = 50;
        float angle = 0f;
        Vector3 lastPoint = center + new Vector3(radius, 0, 0);
        for (int i = 1; i <= segments; i++)
        {
            angle += 360f / segments;
            Vector3 nextPoint = center + new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad) * radius, 0, Mathf.Sin(angle * Mathf.Deg2Rad) * radius);
            Gizmos.DrawLine(lastPoint, nextPoint);
            lastPoint = nextPoint;
        }
    }
}