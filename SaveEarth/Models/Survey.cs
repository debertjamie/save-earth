using System.Text.Json;

namespace SaveEarth.Models;

public class Survey
{
    public string SurveyId { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public List<Question> Questions { get; } = new();
    public SurveyStatus Status { get; private set; } = SurveyStatus.Draft;

    public Survey(string surveyId, string title, string description)
    {
        CreateSurvey(surveyId, title, description);
    }

    public void CreateSurvey(string surveyId, string title, string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(surveyId);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        SurveyId = surveyId;
        Title = title;
        Description = description;
    }

    public void AddQuestion(Question question)
    {
        ArgumentNullException.ThrowIfNull(question);

        if (Status != SurveyStatus.Draft)
        {
            throw new InvalidOperationException("Questions can only be added to a draft survey.");
        }

        if (Questions.Any(existing => existing.QuestionId == question.QuestionId))
        {
            throw new InvalidOperationException("A question with the same ID is already in the survey.");
        }

        Questions.Add(question);
    }

    public void Publish()
    {
        if (Questions.Count == 0)
        {
            throw new InvalidOperationException("A survey must contain at least one question before publishing.");
        }

        Status = SurveyStatus.Published;
    }

    public void Close()
    {
        if (Status != SurveyStatus.Published)
        {
            throw new InvalidOperationException("Only a published survey can be closed.");
        }

        Status = SurveyStatus.Closed;
    }
    public string GetSummaryReport()
    {
        return JsonSerializer.Serialize(this);
    }
}