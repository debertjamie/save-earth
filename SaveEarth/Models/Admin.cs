using System.Text.Json;

namespace SaveEarth.Models;

public class Admin
{
    public string AdminId { get; private set; } = string.Empty;
    public string Role { get; private set; } = "Admin";

    // private: field internal yang hanya dapat diakses di dalam class Admin
    private readonly List<User> _users = new();
    private readonly List<Challenge> _challenges = new();

    // public: properti yang dapat diakses dari mana saja
    public List<User> Users => _users;
    public List<Survey> Surveys { get; } = new();
    public List<Challenge> Challenges => _challenges;
    public HashSet<string> RecommendationIds { get; } = new(StringComparer.OrdinalIgnoreCase);

    public Admin(string adminId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(adminId);
        AdminId = adminId;
    }

    public void AddUser(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        if (_users.Any(existing => existing.UserId == user.UserId))
        {
            throw new InvalidOperationException("A user with the same ID already exists.");
        }

        _users.Add(user);
    }

    public bool RemoveUser(string userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        var user = _users.FirstOrDefault(existing => existing.UserId == userId);
        return user is not null && _users.Remove(user);
    }

    public void UpdateUser(User user, string name, string email, string password)
    {
        ArgumentNullException.ThrowIfNull(user);
        if (!_users.Contains(user))
        {
            throw new InvalidOperationException("The user is not managed by this administrator.");
        }

        user.UpdateProfile(name, email, password);
    }

    public Survey CreateSurvey(string surveyName, List<Question> questions)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(surveyName);
        ArgumentNullException.ThrowIfNull(questions);
        if (questions.Count == 0)
        {
            throw new ArgumentException("A survey must contain at least one question.", nameof(questions));
        }

        var survey = new Survey(Guid.NewGuid().ToString("N"), surveyName, surveyName);
        foreach (var question in questions)
        {
            survey.AddQuestion(question);
        }

        Surveys.Add(survey);
        return survey;
    }

    public Challenge CreateChallenge(string challengeName, string description, DateTime startDate, DateTime endDate)
    {
        if (endDate <= startDate)
        {
            throw new ArgumentException("The challenge end date must be after the start date.", nameof(endDate));
        }

        var durationDays = (int)Math.Ceiling((endDate - startDate).TotalDays);
        var challenge = new Challenge(
            Guid.NewGuid().ToString("N"), challengeName, description, 0, durationDays, 0.0f);
        _challenges.Add(challenge);
        return challenge;
    }

    public bool AddRecommendation(string recommendationId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(recommendationId);
        return RecommendationIds.Add(recommendationId);
    }

    public bool RemoveRecommendation(string recommendationId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(recommendationId);
        return RecommendationIds.Remove(recommendationId);
    }

    public string ViewReports()
    {
        return JsonSerializer.Serialize(new
        {
            UserCount = _users.Count,
            SurveyCount = Surveys.Count,
            ChallengeCount = _challenges.Count,
            RecommendationCount = RecommendationIds.Count
        });
    }
}