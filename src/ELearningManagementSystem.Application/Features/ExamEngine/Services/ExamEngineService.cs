using System.Text;
using System.Text.Json;
using ELearningManagementSystem.Application.Common;
using ELearningManagementSystem.Application.Features.ExamEngine.DTOs;
using ELearningManagementSystem.Application.Interfaces;
using ELearningManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ELearningManagementSystem.Application.Features.ExamEngine.Services;

public class ExamEngineService : IExamEngineService
{
    private readonly IAppDbContext _context;

    public ExamEngineService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ExamAttemptSessionResponse>> StartExamAttemptAsync(int examId, int userId, CancellationToken cancellationToken = default)
    {
        // 1. Check if student has an active InProgress attempt that is not expired
        var existingAttempt = await _context.CourseExamAttempts
            .Include(a => a.CourseExam).ThenInclude(e => e.Course)
            .Include(a => a.AttemptAnswers)
            .FirstOrDefaultAsync(a => a.ExamId == examId && a.UserId == userId && a.Status == "InProgress" && !a.DeleteFlag, cancellationToken);

        if (existingAttempt != null)
        {
            if (DateTime.UtcNow < existingAttempt.ExpiresAt)
            {
                return await GetActiveAttemptSessionAsync(existingAttempt.AttemptId, userId, cancellationToken);
            }
            else
            {
                // Time is up: auto-submit the previous unfinished attempt so the student's
                // selected answers are saved and scored, rather than silently discarding them.
                var autoSubmit = await SubmitExamAttemptAsync(
                    existingAttempt.AttemptId,
                    new SubmitExamAttemptRequest { AttemptId = existingAttempt.AttemptId, Answers = new() },
                    userId,
                    cancellationToken);

                if (autoSubmit.IsSuccess)
                {
                    // The attempt is now Submitted; there is no active session to resume.
                    // Signal the client with the submitted attempt id so it can open the result.
                    return Result.Failure<ExamAttemptSessionResponse>(
                        "AttemptAlreadySubmitted:" + autoSubmit.Value!.AttemptId);
                }
            }
        }

        // 2. Validate Approved Unused Payment (1 Payment = 1 Attempt)
        var payment = await _context.ExamPayments
            .Where(p => p.ExamId == examId && p.UserId == userId && p.Status == "Approved" && !p.IsUsed && !p.DeleteFlag)
            .OrderBy(p => p.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (payment == null)
        {
            return Result.Failure<ExamAttemptSessionResponse>("NoApprovedPayment: An approved payment is required to start this exam attempt.");
        }

        // 3. Load Exam and Question Pool
        var exam = await _context.CourseExams
            .Include(e => e.Course)
            .Include(e => e.ExamQuestions.Where(q => !q.DeleteFlag))
                .ThenInclude(q => q.ExamQuestionOptions.Where(o => !o.DeleteFlag))
            .FirstOrDefaultAsync(e => e.ExamId == examId && e.Status && !e.DeleteFlag, cancellationToken);

        if (exam == null)
        {
            return Result.Failure<ExamAttemptSessionResponse>("ExamNotFound: The requested course exam was not found or is inactive.");
        }

        var availableQuestions = exam.ExamQuestions.ToList();
        if (availableQuestions.Count < exam.QuestionCount)
        {
            return Result.Failure<ExamAttemptSessionResponse>($"InsufficientPool: Question pool contains {availableQuestions.Count} questions, but exam requires {exam.QuestionCount}.");
        }

        // 4. Consume Payment
        payment.IsUsed = true;

        // 5. Randomly Pull Questions from Pool
        var random = new Random();
        var selectedQuestions = availableQuestions
            .OrderBy(_ => random.Next())
            .Take(exam.QuestionCount)
            .ToList();

        // 6. Create Attempt Session
        var startedAt = DateTime.UtcNow;
        var expiresAt = startedAt.AddMinutes(exam.DurationMinutes);

        var attempt = new CourseExamAttempt
        {
            ExamId = examId,
            UserId = userId,
            ExamPaymentId = payment.ExamPaymentId,
            StartedAt = startedAt,
            ExpiresAt = expiresAt,
            Status = "InProgress",
            CreatedAt = DateTime.UtcNow,
            DeleteFlag = false
        };

        _context.CourseExamAttempts.Add(attempt);
        await _context.SaveChangesAsync(cancellationToken);

        // 7. Create Randomized Question & Option Snapshots
        int order = 1;
        foreach (var q in selectedQuestions)
        {
            // Randomize options for each question
            var shuffledOptions = q.ExamQuestionOptions
                .OrderBy(_ => random.Next())
                .Select(o => new ExamAttemptOptionDto
                {
                    OptionId = o.OptionId,
                    OptionText = o.OptionText
                })
                .ToList();

            var answerSnapshot = new CourseExamAttemptAnswer
            {
                AttemptId = attempt.AttemptId,
                ExamQuestionId = q.ExamQuestionId,
                QuestionOrder = order++,
                QuestionTextSnapshot = q.QuestionText,
                ShuffledOptionsJson = JsonSerializer.Serialize(shuffledOptions)
            };

            _context.CourseExamAttemptAnswers.Add(answerSnapshot);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return await GetActiveAttemptSessionAsync(attempt.AttemptId, userId, cancellationToken);
    }

    public async Task<Result<ExamAttemptSessionResponse>> GetActiveAttemptSessionAsync(int attemptId, int userId, CancellationToken cancellationToken = default)
    {
        var attempt = await _context.CourseExamAttempts
            .Include(a => a.CourseExam).ThenInclude(e => e.Course)
            .Include(a => a.AttemptAnswers)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.AttemptId == attemptId && a.UserId == userId && !a.DeleteFlag, cancellationToken);

        if (attempt == null)
        {
            return Result.Failure<ExamAttemptSessionResponse>("AttemptNotFound: Exam attempt session was not found.");
        }

        var remainingSeconds = (int)Math.Max(0, (attempt.ExpiresAt - DateTime.UtcNow).TotalSeconds);

        var questionDtos = new List<ExamAttemptQuestionDto>();
        foreach (var ans in attempt.AttemptAnswers.OrderBy(a => a.QuestionOrder))
        {
            List<ExamAttemptOptionDto> options = new();
            try
            {
                if (!string.IsNullOrEmpty(ans.ShuffledOptionsJson))
                {
                    options = JsonSerializer.Deserialize<List<ExamAttemptOptionDto>>(ans.ShuffledOptionsJson) ?? new();
                }
            }
            catch {}

            questionDtos.Add(new ExamAttemptQuestionDto
            {
                AttemptAnswerId = ans.AttemptAnswerId,
                ExamQuestionId = ans.ExamQuestionId,
                QuestionOrder = ans.QuestionOrder,
                QuestionText = ans.QuestionTextSnapshot,
                SelectedOptionId = ans.SelectedOptionId,
                Options = options
            });
        }

        var response = new ExamAttemptSessionResponse
        {
            AttemptId = attempt.AttemptId,
            ExamId = attempt.ExamId,
            ExamTitle = attempt.CourseExam.Title,
            CourseTitle = attempt.CourseExam.Course.Title,
            DurationMinutes = attempt.CourseExam.DurationMinutes,
            PassingScore = attempt.CourseExam.PassingScore,
            StartedAt = attempt.StartedAt,
            ExpiresAt = attempt.ExpiresAt,
            RemainingSeconds = remainingSeconds,
            Status = attempt.Status,
            Questions = questionDtos
        };

        return Result.Success(response);
    }

    public async Task<Result<bool>> SaveAnswerAsync(int attemptId, SaveAnswerRequest request, int userId, CancellationToken cancellationToken = default)
    {
        var attempt = await _context.CourseExamAttempts
            .FirstOrDefaultAsync(a => a.AttemptId == attemptId && a.UserId == userId && a.Status == "InProgress" && !a.DeleteFlag, cancellationToken);

        if (attempt == null)
        {
            return Result.Failure<bool>("AttemptInactive: Cannot save answer for an inactive or completed attempt.");
        }

        var answer = await _context.CourseExamAttemptAnswers
            .FirstOrDefaultAsync(a => a.AttemptAnswerId == request.AttemptAnswerId && a.AttemptId == attemptId, cancellationToken);

        if (answer == null)
        {
            return Result.Failure<bool>("AnswerNotFound: Question answer record was not found.");
        }

        answer.SelectedOptionId = request.SelectedOptionId;
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }

    public async Task<Result<ExamResultResponse>> SubmitExamAttemptAsync(int attemptId, SubmitExamAttemptRequest request, int userId, CancellationToken cancellationToken = default)
    {
        var attempt = await _context.CourseExamAttempts
            .Include(a => a.CourseExam).ThenInclude(e => e.Course)
            .Include(a => a.AttemptAnswers).ThenInclude(ans => ans.ExamQuestion).ThenInclude(q => q.ExamQuestionOptions)
            .FirstOrDefaultAsync(a => a.AttemptId == attemptId && a.UserId == userId && !a.DeleteFlag, cancellationToken);

        if (attempt == null)
        {
            return Result.Failure<ExamResultResponse>("AttemptNotFound: Exam attempt was not found.");
        }

        if (attempt.Status == "Submitted")
        {
            return await GetAttemptResultAsync(attemptId, userId, cancellationToken);
        }

        // Apply student submitted answers if provided
        if (request.Answers != null && request.Answers.Any())
        {
            foreach (var ans in attempt.AttemptAnswers)
            {
                if (request.Answers.TryGetValue(ans.AttemptAnswerId, out var selectedOptId))
                {
                    ans.SelectedOptionId = selectedOptId;
                }
            }
        }

        // Evaluate Correctness & Calculate Score
        int correctCount = 0;
        int totalQuestions = attempt.AttemptAnswers.Count;

        foreach (var ans in attempt.AttemptAnswers)
        {
            var correctOption = ans.ExamQuestion?.ExamQuestionOptions.FirstOrDefault(o => o.IsCorrect && !o.DeleteFlag);
            if (correctOption != null && ans.SelectedOptionId == correctOption.OptionId)
            {
                ans.IsCorrect = true;
                correctCount++;
            }
            else
            {
                ans.IsCorrect = false;
            }
        }

        decimal score = totalQuestions > 0 ? Math.Round((decimal)correctCount / totalQuestions * 100, 2) : 0;
        bool passed = score >= attempt.CourseExam.PassingScore;

        attempt.SubmittedAt = DateTime.UtcNow;
        attempt.Score = score;
        attempt.Passed = passed;
        attempt.Status = "Submitted";

        await _context.SaveChangesAsync(cancellationToken);

        return await GetAttemptResultAsync(attemptId, userId, cancellationToken);
    }

    public async Task<Result<ExamResultResponse>> GetAttemptResultAsync(int attemptId, int userId, CancellationToken cancellationToken = default)
    {
        var attempt = await _context.CourseExamAttempts
            .Include(a => a.CourseExam).ThenInclude(e => e.Course)
            .Include(a => a.AttemptAnswers).ThenInclude(ans => ans.ExamQuestion).ThenInclude(q => q.ExamQuestionOptions)
            .Include(a => a.AttemptAnswers).ThenInclude(ans => ans.SelectedOption)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.AttemptId == attemptId && a.UserId == userId && !a.DeleteFlag, cancellationToken);

        if (attempt == null)
        {
            return Result.Failure<ExamResultResponse>("AttemptNotFound: Exam attempt result was not found.");
        }

        var breakdown = new List<ExamResultQuestionBreakdownDto>();
        int correctAnswersCount = 0;

        foreach (var ans in attempt.AttemptAnswers.OrderBy(a => a.QuestionOrder))
        {
            var correctOption = ans.ExamQuestion?.ExamQuestionOptions.FirstOrDefault(o => o.IsCorrect && !o.DeleteFlag);
            bool isCorrect = ans.IsCorrect ?? (correctOption != null && ans.SelectedOptionId == correctOption.OptionId);

            if (isCorrect) correctAnswersCount++;

            List<ExamAttemptOptionDto> options = new();
            try
            {
                if (!string.IsNullOrEmpty(ans.ShuffledOptionsJson))
                {
                    options = JsonSerializer.Deserialize<List<ExamAttemptOptionDto>>(ans.ShuffledOptionsJson) ?? new();
                }
            }
            catch {}

            breakdown.Add(new ExamResultQuestionBreakdownDto
            {
                QuestionOrder = ans.QuestionOrder,
                QuestionText = ans.QuestionTextSnapshot,
                SelectedOptionId = ans.SelectedOptionId,
                SelectedOptionText = ans.SelectedOption?.OptionText,
                CorrectOptionId = correctOption?.OptionId ?? 0,
                CorrectOptionText = correctOption?.OptionText ?? "N/A",
                IsCorrect = isCorrect,
                Options = options
            });
        }

        var response = new ExamResultResponse
        {
            AttemptId = attempt.AttemptId,
            ExamId = attempt.ExamId,
            ExamTitle = attempt.CourseExam.Title,
            CourseTitle = attempt.CourseExam.Course.Title,
            StartedAt = attempt.StartedAt,
            SubmittedAt = attempt.SubmittedAt ?? DateTime.UtcNow,
            Score = attempt.Score ?? 0,
            PassingScore = attempt.CourseExam.PassingScore,
            Passed = attempt.Passed ?? false,
            TotalQuestions = attempt.AttemptAnswers.Count,
            CorrectAnswersCount = correctAnswersCount,
            Status = attempt.Status,
            QuestionsBreakdown = breakdown
        };

        return Result.Success(response);
    }

    public async Task<Result<PublicCertificateResponse>> GetPublicCertificateAsync(int attemptId, CancellationToken cancellationToken = default)
    {
        var attempt = await _context.CourseExamAttempts
            .Include(a => a.CourseExam).ThenInclude(e => e.Course)
            .Include(a => a.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.AttemptId == attemptId && a.Status == "Submitted" && (a.Passed == true) && !a.DeleteFlag, cancellationToken);

        if (attempt == null)
        {
            return Result.Failure<PublicCertificateResponse>("CertificateNotFound: Valid certification attempt was not found.");
        }

        var response = new PublicCertificateResponse
        {
            AttemptId = attempt.AttemptId,
            VerificationCode = $"CERT-EDU-{attempt.AttemptId:D6}",
            StudentName = attempt.User?.FullName ?? "Student",
            ExamTitle = attempt.CourseExam.Title,
            CourseTitle = attempt.CourseExam.Course.Title,
            Score = attempt.Score ?? 0,
            PassingScore = attempt.CourseExam.PassingScore,
            Passed = attempt.Passed ?? false,
            IssuedAt = attempt.SubmittedAt ?? attempt.CreatedAt,
            IsValid = true
        };

        return Result.Success(response);
    }

    public async Task<Result<CertificateFileDownloadResponse>> DownloadCertificateFileAsync(int attemptId, CancellationToken cancellationToken = default)
    {
        var attempt = await _context.CourseExamAttempts
            .Include(a => a.CourseExam).ThenInclude(e => e.Course)
            .Include(a => a.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.AttemptId == attemptId && a.Status == "Submitted" && (a.Passed == true) && !a.DeleteFlag, cancellationToken);

        if (attempt == null)
        {
            return Result.Failure<CertificateFileDownloadResponse>("CertificateNotFound: Valid certification attempt was not found.");
        }

        var studentName = attempt.User?.FullName ?? "Student";
        var courseTitle = attempt.CourseExam.Course.Title;
        var examTitle = attempt.CourseExam.Title;
        var score = attempt.Score ?? 0;
        var passingScore = attempt.CourseExam.PassingScore;
        var issuedAt = attempt.SubmittedAt ?? attempt.CreatedAt;
        var verificationCode = $"CERT-EDU-{attempt.AttemptId:D6}";

        var html = BuildCertificateHtml(studentName, courseTitle, examTitle, score, passingScore, issuedAt, verificationCode);
        var safeCourse = string.Join("_", courseTitle.Split(System.IO.Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));

        var response = new CertificateFileDownloadResponse
        {
            Content = Encoding.UTF8.GetBytes(html),
            FileName = string.IsNullOrWhiteSpace(safeCourse)
                ? "EduSphere_Certificate.html"
                : $"Certificate_{safeCourse}.html"
        };

        return Result.Success(response);
    }

    private static string BuildCertificateHtml(
        string studentName,
        string courseTitle,
        string examTitle,
        decimal score,
        int passingScore,
        DateTime issuedAt,
        string verificationCode)
    {
        return $@"<!DOCTYPE html>
<html lang=""en"">
<head>
<meta charset=""utf-8""/>
<title>Certificate of Achievement - {System.Net.WebUtility.HtmlEncode(courseTitle)}</title>
<style>
  * {{ margin:0; padding:0; box-sizing:border-box; }}
  body {{ font-family:'Georgia','Times New Roman',serif; background:#f1f5f9; color:#0f172a; display:flex; align-items:center; justify-content:center; min-height:100vh; padding:40px; }}
  .cert {{ width:860px; max-width:100%; background:#ffffff; border:2px solid #e2e8f0; border-radius:24px; padding:48px; text-align:center; box-shadow:0 20px 40px -12px rgba(2,6,23,0.2); position:relative; }}
  .logo {{ width:64px; height:64px; border-radius:16px; background:#f0f2ff; border:1px solid #dfe3ff; color:#4f46e5; display:flex; align-items:center; justify-content:center; font-size:32px; margin:0 auto 12px; }}
  .brand {{ font-size:13px; letter-spacing:3px; text-transform:uppercase; color:#4f46e5; font-weight:700; }}
  h1 {{ font-size:40px; letter-spacing:2px; text-transform:uppercase; color:#0f172a; margin:20px 0 4px; font-weight:800; }}
  .subtitle {{ font-size:13px; letter-spacing:3px; text-transform:uppercase; color:#94a3b8; font-weight:700; }}
  .rule {{ width:72px; height:3px; background:#4f46e5; border-radius:99px; margin:22px auto; }}
  .presented {{ font-size:14px; color:#64748b; font-style:italic; margin-bottom:10px; }}
  .name {{ font-size:42px; color:#0f172a; font-weight:800; margin-bottom:12px; }}
  .body-copy {{ font-size:14px; color:#64748b; max-width:560px; margin:0 auto; line-height:1.7; }}
  .course {{ display:inline-block; margin-top:16px; padding:12px 24px; border-radius:14px; background:#eef0ff; border:1px solid #dfe3ff; color:#312e81; font-size:20px; font-weight:800; }}
  .badges {{ display:flex; justify-content:center; gap:16px; margin:22px 0; flex-wrap:wrap; }}
  .badge {{ background:#f8fafc; border:1px solid #e2e8f0; border-radius:14px; padding:10px 18px; min-width:140px; }}
  .badge .label {{ font-size:11px; text-transform:uppercase; letter-spacing:1px; color:#94a3b8; font-weight:700; display:block; }}
  .badge .value {{ font-size:16px; color:#0f172a; font-weight:800; }}
  .badge .value.green {{ color:#059669; }}
  .footer {{ display:flex; justify-content:space-between; align-items:flex-end; margin-top:34px; padding-top:22px; border-top:1px solid #e2e8f0; text-align:left; }}
  .footer .col {{ }}
  .footer .mini {{ font-size:11px; text-transform:uppercase; letter-spacing:1px; color:#94a3b8; font-weight:700; }}
  .footer .date {{ font-size:14px; font-weight:800; color:#0f172a; }}
  .signature {{ font-size:18px; font-style:italic; font-weight:800; color:#1e1b4b; border-bottom:1px solid #94a3b8; padding:0 8px 3px; display:inline-block; }}
  .signature-title {{ font-size:11px; text-transform:uppercase; letter-spacing:1px; color:#64748b; font-weight:700; margin-top:6px; }}
  .verify {{ margin-top:26px; font-size:12px; color:#94a3b8; font-family:monospace; }}
</style>
</head>
<body>
  <div class=""cert"">
    <div class=""logo"">&#127891;</div>
    <div class=""brand"">EduSphere E-Learning Portal</div>
    <h1>Certificate of Achievement</h1>
    <div class=""subtitle"">Official Certification of Completion</div>
    <div class=""rule""></div>

    <div class=""presented"">This is proudly presented to</div>
    <div class=""name"">{System.Net.WebUtility.HtmlEncode(studentName)}</div>
    <div class=""body-copy"">
      for successfully completing all course requirements and passing the certification examination for
    </div>
    <div class=""course"">{System.Net.WebUtility.HtmlEncode(courseTitle)}</div>

    <div class=""badges"">
      <div class=""badge""><span class=""label"">Score Achieved</span><span class=""value green"">{score:N0}%</span></div>
      <div class=""badge""><span class=""label"">Pass Mark</span><span class=""value"">{passingScore}%</span></div>
      <div class=""badge""><span class=""label"">Issue Date</span><span class=""value"">{issuedAt:dd MMM yyyy}</span></div>
    </div>

    <div class=""footer"">
      <div class=""col"">
        <span class=""mini"">Date Issued</span>
        <div class=""date"">{issuedAt:dd MMMM yyyy}</div>
      </div>
      <div class=""col"" style=""text-align:right;"">
        <span class=""signature"">Dr. Alexander Wright</span>
        <div class=""signature-title"">Academic Program Director</div>
      </div>
    </div>

    <div class=""verify"">Verification ID: {verificationCode} &middot; {System.Net.WebUtility.HtmlEncode(examTitle)}</div>
  </div>
</body>
</html>";
    }

    public async Task<Result<List<PublicCertificateResponse>>> GetMyCertificatesAsync(int userId, CancellationToken cancellationToken = default)
    {
        var attempts = await _context.CourseExamAttempts
            .Include(a => a.CourseExam).ThenInclude(e => e.Course)
            .Include(a => a.User)
            .AsNoTracking()
            .Where(a => a.UserId == userId && a.Status == "Submitted" && a.Passed == true && !a.DeleteFlag)
            .OrderByDescending(a => a.SubmittedAt)
            .ToListAsync(cancellationToken);

        var list = attempts.Select(a => new PublicCertificateResponse
        {
            AttemptId = a.AttemptId,
            VerificationCode = $"CERT-EDU-{a.AttemptId:D6}",
            StudentName = a.User?.FullName ?? "Student",
            ExamTitle = a.CourseExam.Title,
            CourseTitle = a.CourseExam.Course.Title,
            Score = a.Score ?? 0,
            PassingScore = a.CourseExam.PassingScore,
            Passed = true,
            IssuedAt = a.SubmittedAt ?? a.CreatedAt,
            IsValid = true
        }).ToList();

        return Result.Success(list);
    }
}

