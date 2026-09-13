namespace SaveEarth.Models;

public class Challenge
{
    public string ChallengeId { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public int RewardPoints { get; private set; }
    public int TargetDays { get; private set; }
    public float EmissionReductionTarget { get; private set; }

    public Challenge(string challengeId, string title, string description, int rewardPoints, int targetDays, float emissionReductionTarget)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(challengeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentOutOfRangeException.ThrowIfNegative(rewardPoints);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetDays);
        ArgumentOutOfRangeException.ThrowIfNegative(emissionReductionTarget);

        ChallengeId = challengeId;
        Title = title;
        Description = description;
        RewardPoints = rewardPoints;
        TargetDays = targetDays;
        EmissionReductionTarget = emissionReductionTarget;
    }

    public void UpdateTitle(string title)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        Title = title;
    }

    public void UpdateDescription(string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        Description = description;
    }

    public void UpdateRewardPoints(int rewardPoints)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(rewardPoints);
        RewardPoints = rewardPoints;
    }

    public void UpdateTargetDays(int targetDays)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetDays);
        TargetDays = targetDays;
    }

    public void UpdateEmissionReductionTarget(float emissionReductionTarget)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(emissionReductionTarget);
        EmissionReductionTarget = emissionReductionTarget;
    }
}