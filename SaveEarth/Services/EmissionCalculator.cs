using System.Collections.Generic;
using SaveEarth.Models;

namespace SaveEarth.Services;

public class EmissionCalculator
{
    // private: hanya dapat diakses di dalam class EmissionCalculator
    private static float RoundEmission(float value) => MathF.Round(value, 2);

    // public: dapat diakses dari luar class maupun assembly
    public float CalculateTotalEmission(IEnumerable<EmissionActivity> activities)
    {
        ArgumentNullException.ThrowIfNull(activities);

        float total = 0f;
        foreach (EmissionActivity activity in activities)
        {
            // Polymorphism: method yang dipanggil menyesuaikan tipe objek aslinya
            total += activity.CalculateEmission();
        }
        return RoundEmission(total);
    }

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