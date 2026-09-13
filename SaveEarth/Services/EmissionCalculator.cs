namespace SaveEarth.Services;

public class EmissionCalculator
{
    public float CalculateDailyEmission(Models.DailyTracking dailyTracking)
    {
        ArgumentNullException.ThrowIfNull(dailyTracking);
        var emission = dailyTracking.ActivityValue * GetEmissionFactor(dailyTracking.ActivityType);
        dailyTracking.SetDailyEmission(emission);
        return emission;
    }

    private float GetEmissionFactor(string activityType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(activityType);

        return activityType.ToLowerInvariant() switch
        {
            "driving" => 0.411f,
            "flying" => 0.254f,
            "electricity" => 0.0005f,
            _ => throw new ArgumentException($"Unsupported activity type: {activityType}.", nameof(activityType))
        };
    }
}