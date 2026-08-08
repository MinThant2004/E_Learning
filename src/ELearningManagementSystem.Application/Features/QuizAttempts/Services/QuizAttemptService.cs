using ELearningManagementSystem.Application.Common;
using ELearningManagementSystem.Application.Features.QuizAttempts.DTOs;
using ELearningManagementSystem.Application.Interfaces;
using ELearningManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ELearningManagementSystem.Application.Features.QuizAttempts.Services;

public class QuizAttemptService : IQuizAttemptService
{
    private readonly IAppDbContext _context;

    public QuizAttemptService(IAppDbContext context)
    {
        _context = context;
    }

    private async Task<Result<Quiz>> ValidateAvailabilityAsync(int courseId, int userId, CancellationToken cancellationToken)
    {
        var course = await _context.Courses
            .Include(c => c.Lessons.Where(l => !l.DeleteFlag))
            .FirstOrDefaultAsync(c => c.CourseId == courseId && !c.DeleteFlag, cancellationToken);

        if (course == null)
            return Result.Failure<Quiz>("CourseNotFound: The course does not exist or is inactive.");

        var enrollment = await _context.Enrollments
            .Include(e => e.LessonProgresses)
            .FirstOrDefaultAsync(e => e.CourseId == courseId && e.UserId == userId, cancellationToken);

        if (enrollment == null)
            return Result.Failure<Quiz>("EnrollmentNotFound: You are not enrolled in this course.");

        var allLessonsCompleted = course.Lessons.All(lesson => 
            enrollment.LessonProgresses.Any(p => p.LessonId == lesson.LessonId && p.Completed));

        if (!allLessonsCompleted)
            return Result.Failure<Quiz>("CourseNotCompleted: All active lessons must be completed before taking the final quiz.");

        var quiz = await _context.Quizzes
            .Include(q => q.Questions)
            .ThenInclude(q => q.QuestionOptions)
            .FirstOrDefaultAsync(q => q.CourseId == courseId && !q.DeleteFlag, cancellationToken);

        if (quiz == null)
            return Result.Failure<Quiz>("QuizNotFound: No active final quiz found for this course.");

        return Result.Success(quiz);
    }

    public async Task<Result<AvailableQuizResponse>> GetAvailableQuizAsync(int courseId, int userId, CancellationToken cancellationToken = default)
    {
        var validationResult = await ValidateAvailabilityAsync(courseId, userId, cancellationToken);
        if (!validationResult.IsSuccess)
            return Result.Failure<AvailableQuizResponse>(validationResult.Error!);

        var quiz = validationResult.Value!;

        var response = new AvailableQuizResponse
        {
            QuizId = quiz.QuizId,
            CourseId = quiz.CourseId,
            Title = quiz.Title,
            Description = null, // No description field in Quiz entity
            Questions = quiz.Questions.Select(q => new StudentQuizQuestionResponse
            {
                QuestionId = q.QuestionId,
                QuestionText = q.QuestionText,
                DisplayOrder = q.DisplayOrder,
                Options = q.QuestionOptions.Select(o => new StudentQuizOptionResponse
                {
                    OptionId = o.OptionId,
                    OptionText = o.OptionText
                }).ToList()
            }).OrderBy(q => q.DisplayOrder).ToList()
        };

        return Result.Success(response);
    }

    public async Task<Result<QuizAttemptResultResponse>> SubmitQuizAttemptAsync(int courseId, int userId, SubmitQuizAttemptRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await ValidateAvailabilityAsync(courseId, userId, cancellationToken);
        if (!validationResult.IsSuccess)
            return Result.Failure<QuizAttemptResultResponse>(validationResult.Error!);

        var quiz = validationResult.Value!;
        var totalQuestions = quiz.Questions.Count;

        int correctAnswersCount = 0;
        var quizAnswers = new List<QuizAnswer>();

        foreach (var answerRequest in request.Answers)
        {
            var question = quiz.Questions.FirstOrDefault(q => q.QuestionId == answerRequest.QuestionId);
            if (question == null)
                return Result.Failure<QuizAttemptResultResponse>($"InvalidQuestion: Question {answerRequest.QuestionId} does not belong to this quiz.");

            var selectedOption = question.QuestionOptions.FirstOrDefault(o => o.OptionId == answerRequest.SelectedOptionId);
            if (selectedOption == null)
                return Result.Failure<QuizAttemptResultResponse>($"InvalidOption: Option {answerRequest.SelectedOptionId} does not belong to question {answerRequest.QuestionId}.");

            if (selectedOption.IsCorrect)
                correctAnswersCount++;

            quizAnswers.Add(new QuizAnswer
            {
                QuestionId = question.QuestionId,
                SelectedOptionId = selectedOption.OptionId
            });
        }

        decimal score = totalQuestions > 0 ? (decimal)correctAnswersCount / totalQuestions * 100 : 0;
        bool passed = score >= quiz.PassingScore;

        var attempt = new QuizAttempt
        {
            QuizId = quiz.QuizId,
            UserId = userId,
            Score = score,
            CorrectAnswers = correctAnswersCount,
            TotalQuestions = totalQuestions,
            Passed = passed,
            SubmittedAt = DateTime.UtcNow,
            QuizAnswers = quizAnswers
        };

        _context.QuizAttempts.Add(attempt);
        
        // Complete the course enrollment if they passed and haven't already
        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.CourseId == courseId && e.UserId == userId, cancellationToken);
            
        if (enrollment != null && passed && !enrollment.Completed)
        {
            enrollment.Completed = true;
            enrollment.CompletedDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        var response = new QuizAttemptResultResponse
        {
            AttemptId = attempt.AttemptId,
            QuizId = attempt.QuizId,
            Score = attempt.Score,
            CorrectAnswers = attempt.CorrectAnswers,
            TotalQuestions = attempt.TotalQuestions,
            Passed = attempt.Passed,
            SubmittedAt = attempt.SubmittedAt
        };

        return Result.Success(response);
    }

    public async Task<Result<List<QuizAttemptHistoryResponse>>> GetStudentAttemptHistoryAsync(int courseId, int userId, CancellationToken cancellationToken = default)
    {
        var attempts = await _context.QuizAttempts
            .Include(a => a.Quiz)
            .Where(a => a.Quiz.CourseId == courseId && a.UserId == userId)
            .OrderByDescending(a => a.SubmittedAt)
            .Select(a => new QuizAttemptHistoryResponse
            {
                AttemptId = a.AttemptId,
                QuizId = a.QuizId,
                QuizTitle = a.Quiz.Title,
                Score = a.Score,
                CorrectAnswers = a.CorrectAnswers,
                TotalQuestions = a.TotalQuestions,
                Passed = a.Passed,
                SubmittedAt = a.SubmittedAt
            })
            .ToListAsync(cancellationToken);

        return Result.Success(attempts);
    }

    public async Task<Result<QuizAttemptResultResponse>> GetAttemptDetailsAsync(int attemptId, int userId, CancellationToken cancellationToken = default)
    {
        var attempt = await _context.QuizAttempts
            .Where(a => a.AttemptId == attemptId && a.UserId == userId)
            .Select(a => new QuizAttemptResultResponse
            {
                AttemptId = a.AttemptId,
                QuizId = a.QuizId,
                Score = a.Score,
                CorrectAnswers = a.CorrectAnswers,
                TotalQuestions = a.TotalQuestions,
                Passed = a.Passed,
                SubmittedAt = a.SubmittedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (attempt == null)
            return Result.Failure<QuizAttemptResultResponse>("AttemptNotFound: Quiz attempt not found or access denied.");

        return Result.Success(attempt);
    }
}
