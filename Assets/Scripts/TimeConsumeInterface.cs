using UnityEngine;

/// <summary>
/// 时间消耗请求的返回结果
/// </summary>
public struct TimeConsumeResult
{
    public bool success;           // 是否成功消耗
    public float requestedHours;   // 请求的小时数
    public float remainingHours;   // 剩余小时数
    public string failReason;      // 失败原因（若失败）
    public string action;          // 行为名称

    public override string ToString()
    {
        if (success)
            return $"✓ {action} 成功 | 消耗 {requestedHours}h | 剩余 {remainingHours:F1}h";
        else
            return $"✗ {action} 失败 | 原因: {failReason}";
    }
}

/// <summary>
/// 时间消耗请求类
/// 工作系统、消费系统等通过这个类向 TimeManager 请求消耗时间
/// </summary>
public class TimeConsumeRequest
{
    public float hours;            // 请求消耗的小时数
    public string action;          // 行为名称
    public int priority = 0;       // 优先级（可选，未来扩展）

    public TimeConsumeRequest(float hours, string action = "未命名")
    {
        this.hours = hours;
        this.action = action;
    }
}

/// <summary>
/// TimeManager 的时间消耗接口（供外界调用）
/// 使用这个接口而不直接调用 TimeManager.TryConsumeTime()
/// </summary>
public interface ITimeConsumer
{
    /// <summary>
    /// 请求消耗时间，返回详细结果
    /// </summary>
    TimeConsumeResult RequestTimeConsume(TimeConsumeRequest request);

    /// <summary>
    /// 快速检查是否有足够时间（不消耗）
    /// </summary>
    bool HasEnoughTime(float hours);

    /// <summary>
    /// 获取剩余时间
    /// </summary>
    float GetRemainTime();
}

/// <summary>
/// TimeManager 扩展：实现 ITimeConsumer 接口
/// </summary>
public partial class TimeManager : ITimeConsumer
{
    public TimeConsumeResult RequestTimeConsume(TimeConsumeRequest request)
    {
        float remain = GetRemainTime();
        bool success = TryConsumeTime(request.hours, request.action);

        return new TimeConsumeResult
        {
            success = success,
            requestedHours = request.hours,
            remainingHours = GetRemainTime(),
            failReason = success ? "" : $"时间不足（请求 {request.hours}h，剩余 {remain:F1}h）",
            action = request.action
        };
    }

    public bool HasEnoughTime(float hours)
    {
        return GetRemainTime() >= hours;
    }

    float ITimeConsumer.GetRemainTime() => GetRemainTime();
}