namespace SaveEarth.Models;

public class UserChallenge
{
    public string UserId { get; private set; } = string.Empty;
    public string ChallengeId { get; private set; } = string.Empty;
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public bool IsCompleted => Status == ChallengeStatus.Completed;
    public ChallengeStatus Status { get; private set; } = ChallengeStatus.InProgress;
    public float Progress { get; private set; }
    public int TotalPoints { get; private set; }

    public UserChallenge(string userId, string challengeId, DateTime startDate, DateTime endDate, int totalPoints = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(challengeId);
        if (endDate <= startDate)
        {
            throw new ArgumentException("The challenge end date must be after the start date.", nameof(endDate));
        }
        ArgumentOutOfRangeException.ThrowIfNegative(totalPoints);

        UserId = userId;
        ChallengeId = challengeId;
        StartDate = startDate;
        EndDate = endDate;
        TotalPoints = totalPoints;
    }

    public void StartChallenge()
    {
        if (Status == ChallengeStatus.Completed)
        {
            throw new InvalidOperationException("A completed challenge cannot be restarted.");
        }

        Status = ChallengeStatus.InProgress;
        Progress = 0.0f;
    }

    public void UpdateProgress(float progress)
    {
        if (progress < 0.0f || progress > 100.0f)
        {
            throw new ArgumentOutOfRangeException(nameof(progress), "Progress must be between 0 and 100.");
        }

        Progress = progress;

        if (Progress >= 100.0f)
        {
           CompleteChallenge();
        }
    }

    public void CompleteChallenge()
    {
        Progress = 100.0f;
        Status = ChallengeStatus.Completed;
    }
}