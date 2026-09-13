using System.Text.Json;

namespace SaveEarth.Models;

public class DailyTracking
{
    public string UserId { get; private set; } = string.Empty;
    public DateTime Date { get; private set; }
    public float DailyEmission { get; private set; } = 0.0f;
    public string ActivityType { get; private set; } = string.Empty;
    public float ActivityValue { get; private set; } = 0.0f;
    public string Notes { get; private set; } = string.Empty;

    public DailyTracking(string userId, DateTime date, float dailyEmission)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentOutOfRangeException.ThrowIfNegative(dailyEmission);

        UserId = userId;
        Date = date;
        DailyEmission = dailyEmission;
    }

    public void AddLog(string activityType, float activityValue, string notes)
    {
        if (!string.IsNullOrWhiteSpace(ActivityType))
        {
            throw new InvalidOperationException("A daily log already exists. Use EditLog to change it.");
        }

        EditLog(activityType, activityValue, notes);
    }

    public void EditLog(string activityType, float activityValue, string notes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(activityType);
        ArgumentOutOfRangeException.ThrowIfNegative(activityValue);

        ActivityType = activityType;
        ActivityValue = activityValue;
        Notes = notes ?? string.Empty;
    }

    public void SetDailyEmission(float dailyEmission)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(dailyEmission);
        DailyEmission = dailyEmission;
    }

    public string GetDailySummary()
    {
        return JsonSerializer.Serialize(this);
    }
}