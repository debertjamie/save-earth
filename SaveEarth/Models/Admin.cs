using System.Text.Json;

namespace SaveEarth.Models;

public class Admin
{
    public string AdminId { get; private set; } = string.Empty;
    public string Role { get; private set; } = "Admin";
    public List<User> Users { get; } = new();
    public List<Survey> Surveys { get; } = new();
    public List<Challenge> Challenges { get; } = new();
    public HashSet<string> RecommendationIds { get; } = new(StringComparer.OrdinalIgnoreCase);

    public Admin(string adminId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(adminId);
        AdminId = adminId;
    }

    public void AddUser(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        if (Users.Any(existing => existing.UserId == user.UserId))
        {
            throw new InvalidOperationException("A user with the same ID already exists.");
        }

        Users.Add(user);
    }

    public bool RemoveUser(string userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        var user = Users.FirstOrDefault(existing => existing.UserId == userId);
        return user is not null && Users.Remove(user);
    }

    public void UpdateUser(User user, string name, string email, string password)
    {
        ArgumentNullException.ThrowIfNull(user);
        if (!Users.Contains(user))
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
        Challenges.Add(challenge);
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
            UserCount = Users.Count,
            SurveyCount = Surveys.Count,
            ChallengeCount = Challenges.Count,
            RecommendationCount = RecommendationIds.Count
        });
    }
}