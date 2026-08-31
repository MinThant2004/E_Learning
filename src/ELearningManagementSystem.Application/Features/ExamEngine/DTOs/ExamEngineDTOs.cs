namespace ELearningManagementSystem.Application.Features.ExamEngine.DTOs;

public class StartExamAttemptRequest
{
    public int ExamId { get; set; }
}

public class ExamAttemptSessionResponse
{
    public int AttemptId { get; set; }
    public int ExamId { get; set; }
    public string ExamTitle { get; set; } = string.Empty;
    public string CourseTitle { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public int PassingScore { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public int RemainingSeconds { get; set; }
    public string Status { get; set; } = "InProgress";
    public List<ExamAttemptQuestionDto> Questions { get; set; } = new();
}

public class ExamAttemptQuestionDto
{
    public int AttemptAnswerId { get; set; }
    public int ExamQuestionId { get; set; }
    public int QuestionOrder { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public int? SelectedOptionId { get; set; }
    public List<ExamAttemptOptionDto> Options { get; set; } = new();
}

public class ExamAttemptOptionDto
{
    public int OptionId { get; set; }
    public string OptionText { get; set; } = string.Empty;
}

public class SaveAnswerRequest
{
    public int AttemptAnswerId { get; set; }
    public int? SelectedOptionId { get; set; }
}

public class SubmitExamAttemptRequest
{
    public int AttemptId { get; set; }
    public Dictionary<int, int?> Answers { get; set; } = new(); // AttemptAnswerId -> SelectedOptionId
}

public class ExamResultResponse
{
    public int AttemptId { get; set; }
    public int ExamId { get; set; }
    public string ExamTitle { get; set; } = string.Empty;
    public string CourseTitle { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime SubmittedAt { get; set; }
    public decimal Score { get; set; }
    public int PassingScore { get; set; }
    public bool Passed { get; set; }
    public int TotalQuestions { get; set; }
    public int CorrectAnswersCount { get; set; }
    public string Status { get; set; } = "Submitted";
    public List<ExamResultQuestionBreakdownDto> QuestionsBreakdown { get; set; } = new();
}

public class ExamResultQuestionBreakdownDto
{
    public int QuestionOrder { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public int? SelectedOptionId { get; set; }
    public string? SelectedOptionText { get; set; }
    public int CorrectOptionId { get; set; }
    public string CorrectOptionText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public List<ExamAttemptOptionDto> Options { get; set; } = new();
}

public class PublicCertificateResponse
{
    public int AttemptId { get; set; }
    public string VerificationCode { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string ExamTitle { get; set; } = string.Empty;
    public string CourseTitle { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public int PassingScore { get; set; }
    public bool Passed { get; set; }
    public DateTime IssuedAt { get; set; }
    public bool IsValid { get; set; } = true;
}

public class CertificateFileDownloadResponse
{
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = "EduSphere_Certificate.html";
    public string ContentType { get; set; } = "text/html";
}
