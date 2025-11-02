using UnityEngine;

[CreateAssetMenu(menuName = "Affect/Emotion Sectors Config")]
public class EmotionSectorsConfig : ScriptableObject
{
    [System.Serializable]
    public class Sector
    {
        public string id = "Pleasant";         // 英文键（用于本地化查询）
        [Range(0, 360)] public float centerDeg = 0f;   // 扇区中心角（度）
        [Range(0, 180)] public float halfWidth = 22.5f; // 半宽（度）
        public Color uiColor = Color.white;     // UI/音画可用
        [Tooltip("进入该扇区所需额外角度缓冲（度）")]
        [Range(0, 30)] public float enterHysteresis = 5f;
        [Tooltip("离开该扇区所需额外角度缓冲（度）")]
        [Range(0, 30)] public float exitHysteresis = 2f;
        
        /// <summary>
        /// 获取本地化后的扇区名称
        /// </summary>
        public string GetDisplayName()
        {
            if (LocalizationManager.Instance != null)
            {
                return LocalizationManager.Instance.GetString($"emotion_{id}");
            }
            return id;
        }
    }

    [Header("Sectors (按顺序渲染/检索)")]
    public Sector[] sectors;

    [Header("Intensity by radius")]
    public float mildMax = 1.5f;      // < mildMax => mild
    public float moderateMax = 4f;    // < moderateMax => moderate，否则 intense

    [Header("Clamp & Wrap")]
    public bool wrap360 = true; // 角度环绕处理
}