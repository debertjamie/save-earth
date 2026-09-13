namespace SaveEarth.Models;

public class Question
{
    public string QuestionId { get; private set; } = string.Empty;
    public string QuestionText { get; private set; } = string.Empty;
    public string QuestionType { get; private set; } = string.Empty;
    public float EmissionFactor { get; private set; } = 0.0f;

    public Question(string questionId, string questionText, string questionType, float emissionFactor)
    {
        CreateQuestion(questionId, questionText, questionType, emissionFactor);
    }

    public void CreateQuestion(string questionId, string questionText, string questionType, float emissionFactor)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(questionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(questionText);
        ArgumentException.ThrowIfNullOrWhiteSpace(questionType);
        ArgumentOutOfRangeException.ThrowIfNegative(emissionFactor);

        QuestionId = questionId;
        QuestionText = questionText;
        QuestionType = questionType;
        EmissionFactor = emissionFactor;
    }

    public void AddQuestionToSurvey(Survey survey)
    {
        ArgumentNullException.ThrowIfNull(survey);
        survey.AddQuestion(this);
    }

    public void UpdateQuestionText(string questionText)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(questionText);
        QuestionText = questionText;
    }

    public void UpdateQuestionType(string questionType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(questionType);
        QuestionType = questionType;
    }

    public void UpdateEmissionFactor(float emissionFactor)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(emissionFactor);
        EmissionFactor = emissionFactor;
    }
}