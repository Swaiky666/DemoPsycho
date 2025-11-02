using UnityEngine;

public class EmotionClassifier : MonoBehaviour
{
    public EmotionSectorsConfig config;

    [Header("Smoothing (EMA)")]
    [Range(0, 1)] public float alpha = 0.4f;   // 0=钝感,1=敏感
    [Range(0, 1)] public float dailyDecay = 0.1f; // 回合末回弹
    public float currentV, currentA;

    // 迟滞状态：记住当前所属扇区（可选）
    private int _currentSectorIndex = -1;

    public struct Result
    {
        public string id;          // 扇区ID
        public string displayName; // 显示名
        public float radius;
        public float angleDeg;
        public string intensity;   // mild/moderate/intense
        public Color color;
        public int sectorIndex;
    }

    // 外部调用：施加这一回合的增量，然后计算
    public Result StepAndClassify(float dV, float dA)
    {
        // 1) 施加变化并做指数平滑
        float targetV = currentV + dV;
        float targetA = currentA + dA;
        currentV = Mathf.Lerp(currentV, targetV, alpha);
        currentA = Mathf.Lerp(currentA, targetA, alpha);

        // 2) 计算 r/θ
        float r = Mathf.Sqrt(currentV * currentV + currentA * currentA);
        float deg = Mathf.Atan2(currentA, currentV) * Mathf.Rad2Deg;
        if (deg < 0) deg += 360f;

        // 3) 扇区判定（带迟滞）
        int sectorIdx = PickSectorWithHysteresis(deg);

        // 4) 强度分级
        string intensity = (r < config.mildMax) ? "mild" :
                           (r < config.moderateMax) ? "moderate" : "intense";

        var s = sectorIdx >= 0 ? config.sectors[sectorIdx] : null;
        return new Result
        {
            id = s != null ? s.id : "Neutral",
            displayName = s != null ? s.GetDisplayName() : LocalizationManager.Instance?.GetString("neutral") ?? "Neutral",
            radius = r,
            angleDeg = deg,
            intensity = intensity,
            color = s != null ? s.uiColor : Color.white,
            sectorIndex = sectorIdx
        };
    }

    // 回合/日末：回弹到原点 + 可在此做Fail判定
    public void EndOfDayDrift()
    {
        currentV *= (1f - dailyDecay);
        currentA *= (1f - dailyDecay);
    }

    int PickSectorWithHysteresis(float deg)
    {
        if (config == null || config.sectors == null || config.sectors.Length == 0)
            return -1;

        // 若当前没扇区，直接找"进入阈值"最匹配的
        if (_currentSectorIndex < 0)
            return _currentSectorIndex = FindSector(deg, enterMode: true);

        // 已在某扇区：先检查是否仍在"退出阈值"范围内；在就保持
        if (IsInsideSector(deg, config.sectors[_currentSectorIndex], useEnter: false))
            return _currentSectorIndex;

        // 否则寻找"进入阈值"最近的扇区
        return _currentSectorIndex = FindSector(deg, enterMode: true);
    }

    int FindSector(float deg, bool enterMode)
    {
        int best = -1;
        float bestGap = float.MaxValue;

        for (int i = 0; i < config.sectors.Length; i++)
        {
            var s = config.sectors[i];
            if (IsInsideSector(deg, s, useEnter: enterMode))
            {
                // 选中心角差最小的
                float gap = AngleGap(deg, s.centerDeg);
                if (gap < bestGap) { bestGap = gap; best = i; }
            }
        }
        return best;
    }

    bool IsInsideSector(float deg, EmotionSectorsConfig.Sector s, bool useEnter)
    {
        float half = s.halfWidth + (useEnter ? s.enterHysteresis : -s.exitHysteresis);
        half = Mathf.Max(0f, half); // 防止负宽度
        // 处理跨0度情况：使用"最小夹角"判断
        float d = AngleGap(deg, s.centerDeg);
        return d <= half;
    }

    // 最小夹角差（0~180）
    float AngleGap(float a, float b)
    {
        float d = Mathf.Abs(a - b) % 360f;
        return d > 180f ? 360f - d : d;
    }

    public Result GetCurrentResult()
    {
        return StepAndClassify(0, 0);
    }
}