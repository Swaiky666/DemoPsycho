using UnityEngine;
using System;

/// <summary>
/// 工作数据结构
/// 定义一个工作的所有信息
/// </summary>
[System.Serializable]
public class JobData
{
    public string jobId;                // 工作 ID（唯一标识）
    public string jobName;              // 工作名称
    public string timeSlot;             // 时间段（早上、中午、下午、晚上）
    public float timeRequired;          // 工作耗时（小时）
    public float basePay;               // 基础薪资
    public float requiredSkill;         // 技能要求（0-100）
    public float vChange;               // 情绪效价变化
    public float aChange;               // 情绪唤醒变化
    public float healthCost;            // 健康消耗
    public string description;          // 工作描述
    public Sprite jobIcon;              // 工作图标（可选）
    public Color jobColor;              // 工作卡牌颜色（可选）

    /// <summary>
    /// 创建新的工作数据
    /// </summary>
    public JobData(
        string id,
        string name,
        string slot,
        float time,
        float pay,
        float skill,
        float v,
        float a,
        float health,
        string desc = "")
    {
        jobId = id;
        jobName = name;
        timeSlot = slot;
        timeRequired = time;
        basePay = pay;
        requiredSkill = skill;
        vChange = v;
        aChange = a;
        healthCost = health;
        description = desc;
        jobColor = Color.white;
    }

    /// <summary>
    /// 获取工作的格式化描述
    /// </summary>
    public string GetFormattedInfo()
    {
        return $@"
【{jobName}】
时间段：{timeSlot}
耗时：{timeRequired}小时
薪资：¥{basePay:F0}
技能要求：{requiredSkill:F1}

属性变化：
  情绪效价：{vChange:+0.0;-0.0;0}
  情绪唤醒：{aChange:+0.0;-0.0;0}
  健康消耗：-{healthCost:F1}

描述：{description}
";
    }

    /// <summary>
    /// 检查玩家是否满足工作要求
    /// </summary>
    public bool CanPlayerDo(float playerSkill, float playerHealth)
    {
        if (playerSkill < requiredSkill)
            return false;
        if (playerHealth < healthCost)
            return false;
        return true;
    }
}