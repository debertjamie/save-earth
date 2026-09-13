namespace SaveEarth.Models;

public class SurveyResponse
{
    public string SurveyId { get; private set; } = string.Empty;
    public string UserId { get; private set; } = string.Empty;
    public List<UserAnswer> UserAnswers { get; private set; } = new List<UserAnswer>();
    public SurveyResponseStatus Status { get; private set; } = SurveyResponseStatus.Draft;

    public SurveyResponse(string surveyId, string userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(surveyId);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        SurveyId = surveyId;
        UserId = userId;
    }
    public void AddUserAnswer(UserAnswer userAnswer)
    {
        ArgumentNullException.ThrowIfNull(userAnswer);
        if (Status != SurveyResponseStatus.Draft)
        {
            throw new InvalidOperationException("Answers cannot be changed after submission.");
        }

        if (userAnswer.UserId == UserId && UserAnswers.All(answer => answer.QuestionId != userAnswer.QuestionId))
        {
            UserAnswers.Add(userAnswer);
        }
        else
        {
            throw new ArgumentException("Invalid user answer or mismatched user ID.");
        }
    }

    public bool SaveDraft()
    {
        if (Status == SurveyResponseStatus.Submitted)
        {
            return false;
        }

        Status = SurveyResponseStatus.Draft;
        return true;
    }

    public bool Submit()
    {
        if (UserAnswers.Count == 0)
        {
            return false;
        }

        Status = SurveyResponseStatus.Submitted;
        return true;
    }

    public float CalculateTotalEmissions(List<Question> questions)
    {
        ArgumentNullException.ThrowIfNull(questions);
        var questionsById = questions.ToDictionary(question => question.QuestionId);
        float totalEmissions = 0.0f;

        foreach (var userAnswer in UserAnswers)
        {
            if (questionsById.TryGetValue(userAnswer.QuestionId, out var question))
            {
                var quantity = float.TryParse(userAnswer.AnswerText, out var parsedQuantity)
                    ? parsedQuantity
                    : 1.0f;
                ArgumentOutOfRangeException.ThrowIfNegative(quantity);
                totalEmissions += quantity * question.EmissionFactor;
            }
        }

        return totalEmissions;
    }
}