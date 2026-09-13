namespace SaveEarth.Services;

public class RecommendationEngine
{
    public string RecommendationId { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;

    public List<string> GenerateRecommendations(float totalEmissions)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(totalEmissions);

        var recommendations = new List<string>();
        if (totalEmissions >= 20.0f)
        {
            recommendations.Add("Use public transportation for more trips");
        }
        if (totalEmissions >= 10.0f)
        {
            recommendations.Add("Reduce meat consumption");
        }
        if (totalEmissions >= 5.0f)
        {
            recommendations.Add("Reduce household electricity use");
        }

        recommendations.Add("Recycle more and avoid single-use products");
        return recommendations;
    }

    public string[] GetSuggestions(float totalEmissions)
    {
        var recommendations = GenerateRecommendations(totalEmissions);
        return recommendations.ToArray();
    }
}