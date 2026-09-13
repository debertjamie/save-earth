using SaveEarth.Services;

namespace SaveEarth.Models;

public class User
{
    public string UserId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public List<UserChallenge> Challenges { get; } = new();
    public List<SurveyResponse> SurveyResponses { get; } = new();
    public List<string> DailyActivities { get; } = new();
    public int TotalPoints { get; private set; }

    public User(string userId, string name, string email, string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ValidateEmail(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        UserId = userId;
        Name = name;
        Email = email;
        PasswordHash = PasswordHasher.Hash(password);
    }

    private bool Authenticate(string email, string password)
    {
        return string.Equals(Email, email, StringComparison.OrdinalIgnoreCase)
            && PasswordHasher.Verify(password, PasswordHash);
    }

    public bool Login(string email, string password)
    {
        return Authenticate(email, password);
    }

    public bool Register(string name, string email, string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ValidateEmail(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        Name = name;
        Email = email;
        PasswordHash = PasswordHasher.Hash(password);
        return true;
    }

    public bool UpdateProfile(string name, string email, string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ValidateEmail(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        Name = name;
        Email = email;
        PasswordHash = PasswordHasher.Hash(password);
        return true;
    }

    public bool FillSurvey(SurveyResponse surveyResponse)
    {
        if (surveyResponse is null || surveyResponse.UserId != UserId || SurveyResponses.Contains(surveyResponse))
        {
            return false;
        }

        SurveyResponses.Add(surveyResponse);
        return true;
    }

    public bool LogDailyActivity(string activity)
    {
        if (string.IsNullOrWhiteSpace(activity))
        {
            return false;
        }

        DailyActivities.Add(activity);
        return true;
    }

    public bool JoinChallenge(string challengeId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(challengeId);
        if (Challenges.Any(challenge => challenge.ChallengeId == challengeId && !challenge.IsCompleted))
        {
            return false;
        }

        var startDate = DateTime.UtcNow;
        var userChallenge = new UserChallenge(UserId, challengeId, startDate, startDate.AddDays(30));
        userChallenge.StartChallenge();
        Challenges.Add(userChallenge);
        return true;
    }

    public string ViewResults(Survey survey)
    {
        ArgumentNullException.ThrowIfNull(survey);
        return string.Empty;
    }

    private static void ValidateEmail(string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        if (!email.Contains('@', StringComparison.Ordinal) || email.StartsWith('@') || email.EndsWith('@'))
        {
            throw new ArgumentException("A valid email address is required.", nameof(email));
        }
    }
}