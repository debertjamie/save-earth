namespace SaveEarth.Models;

public class UserAnswer
{
    public string UserId { get; private set; } = string.Empty;
    public string QuestionId { get; private set; } = string.Empty;
    public string AnswerId { get; private set; } = string.Empty;
    public string AnswerText { get; private set; } = string.Empty;

    public UserAnswer(string userId, string questionId, string answerId, string answerText)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(questionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(answerId);
        ArgumentException.ThrowIfNullOrWhiteSpace(answerText);

        UserId = userId;
        QuestionId = questionId;
        AnswerId = answerId;
        AnswerText = answerText;
    }
}