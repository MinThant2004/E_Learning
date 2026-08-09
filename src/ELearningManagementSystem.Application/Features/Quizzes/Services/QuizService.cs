using ELearningManagementSystem.Application.Common;
using ELearningManagementSystem.Application.Features.Quizzes.DTOs;
using ELearningManagementSystem.Application.Features.AuditLogs.DTOs;
using ELearningManagementSystem.Application.Features.AuditLogs.Services;
using ELearningManagementSystem.Application.Interfaces;
using ELearningManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ELearningManagementSystem.Application.Features.Quizzes.Services;

public class QuizService : IQuizService
{
    private readonly IAppDbContext _context;
    private readonly IAuditLogService _auditLogService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<QuizService> _logger;

    public QuizService(IAppDbContext context, IAuditLogService auditLogService, ICurrentUserService currentUserService, ILogger<QuizService> logger)
    {
        _context = context;
        _auditLogService = auditLogService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    // ─── Quiz CRUD ───────────────────────────────────────────────

    public async Task<Result<List<QuizResponse>>> GetQuizzesByCourseAsync(int courseId, bool? isArchived = null, CancellationToken cancellationToken = default)
    {
        var courseExists = await _context.Courses.AnyAsync(c => c.CourseId == courseId, cancellationToken);
        if (!courseExists)
            return Result.Failure<List<QuizResponse>>("CourseNotFound");

        IQueryable<Quiz> query = _context.Quizzes
            .AsNoTracking()
            .Where(q => q.CourseId == courseId);

        if (isArchived.HasValue)
        {
            query = query.Where(q => q.DeleteFlag == isArchived.Value);
        }

        var quizzes = await query
            .OrderByDescending(q => q.CreatedAt)
            .Select(q => new QuizResponse
            {
                QuizId = q.QuizId,
                CourseId = q.CourseId,
                Title = q.Title,
                PassingScore = q.PassingScore,
                CreatedAt = q.CreatedAt,
                UpdatedAt = q.UpdatedAt,
                DeleteFlag = q.DeleteFlag,
                QuestionCount = q.Questions.Count
            })
            .ToListAsync(cancellationToken);

        return Result.Success(quizzes);
    }

    public async Task<Result<QuizDetailResponse>> GetQuizByIdAsync(int quizId, CancellationToken cancellationToken = default)
    {
        var quiz = await _context.Quizzes
            .AsNoTracking()
            .Include(q => q.Questions)
                .ThenInclude(q => q.QuestionOptions)
            .FirstOrDefaultAsync(q => q.QuizId == quizId, cancellationToken);

        if (quiz == null)
            return Result.Failure<QuizDetailResponse>("QuizNotFound");

        return Result.Success(MapToDetail(quiz));
    }

    public async Task<Result<QuizDetailResponse>> CreateQuizAsync(CreateQuizRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return Result.Failure<QuizDetailResponse>("InvalidQuizTitle");

        if (request.PassingScore < 0 || request.PassingScore > 100)
            return Result.Failure<QuizDetailResponse>("InvalidPassingScore");

        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.CourseId == request.CourseId, cancellationToken);

        if (course == null)
            return Result.Failure<QuizDetailResponse>("CourseNotFound");

        if (course.DeleteFlag)
            return Result.Failure<QuizDetailResponse>("CourseArchived");

        // Business rule: prefer one active quiz per course
        var existingActive = await _context.Quizzes
            .AnyAsync(q => q.CourseId == request.CourseId && !q.DeleteFlag, cancellationToken);

        if (existingActive)
            return Result.Failure<QuizDetailResponse>("QuizAlreadyExistsForCourse");

        var quiz = new Quiz
        {
            CourseId = request.CourseId,
            Title = request.Title.Trim(),
            PassingScore = request.PassingScore,
            CreatedAt = DateTime.UtcNow,
            DeleteFlag = false
        };

        _context.Quizzes.Add(quiz);
        await _context.SaveChangesAsync(cancellationToken);

        if (_currentUserService.UserId.HasValue)
        {
            await _auditLogService.CreateAuditLogAsync(new CreateAuditLogRequest
            {
                UserId = _currentUserService.UserId.Value,
                Action = "Create",
                TableName = "Quizzes",
                RecordId = quiz.QuizId
            }, cancellationToken);
            
            _logger.LogInformation("User {UserId} created Quiz {QuizId} in Course {CourseId} (Title={Title})", _currentUserService.UserId.Value, quiz.QuizId, quiz.CourseId, quiz.Title);
        }

        return await GetQuizByIdAsync(quiz.QuizId, cancellationToken);
    }

    public async Task<Result<QuizDetailResponse>> UpdateQuizAsync(int quizId, UpdateQuizRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return Result.Failure<QuizDetailResponse>("InvalidQuizTitle");

        if (request.PassingScore < 0 || request.PassingScore > 100)
            return Result.Failure<QuizDetailResponse>("InvalidPassingScore");

        var quiz = await _context.Quizzes
            .FirstOrDefaultAsync(q => q.QuizId == quizId && !q.DeleteFlag, cancellationToken);

        if (quiz == null)
            return Result.Failure<QuizDetailResponse>("QuizNotFound");

        quiz.Title = request.Title.Trim();
        quiz.PassingScore = request.PassingScore;
        quiz.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        if (_currentUserService.UserId.HasValue)
        {
            await _auditLogService.CreateAuditLogAsync(new CreateAuditLogRequest
            {
                UserId = _currentUserService.UserId.Value,
                Action = "Update",
                TableName = "Quizzes",
                RecordId = quiz.QuizId
            }, cancellationToken);
            
            _logger.LogInformation("User {UserId} updated Quiz {QuizId} in Course {CourseId} (Title={Title})", _currentUserService.UserId.Value, quiz.QuizId, quiz.CourseId, quiz.Title);
        }

        return await GetQuizByIdAsync(quiz.QuizId, cancellationToken);
    }

    public async Task<Result<bool>> ArchiveQuizAsync(int quizId, CancellationToken cancellationToken = default)
    {
        var quiz = await _context.Quizzes
            .FirstOrDefaultAsync(q => q.QuizId == quizId && !q.DeleteFlag, cancellationToken);

        if (quiz == null)
            return Result.Failure<bool>("QuizNotFound");

        quiz.DeleteFlag = true;
        quiz.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        if (_currentUserService.UserId.HasValue)
        {
            await _auditLogService.CreateAuditLogAsync(new CreateAuditLogRequest
            {
                UserId = _currentUserService.UserId.Value,
                Action = "Archive",
                TableName = "Quizzes",
                RecordId = quiz.QuizId
            }, cancellationToken);
            
            _logger.LogInformation("User {UserId} archived Quiz {QuizId}", _currentUserService.UserId.Value, quiz.QuizId);
        }

        return Result.Success(true);
    }

    public async Task<Result<bool>> RestoreQuizAsync(int quizId, CancellationToken cancellationToken = default)
    {
        var quiz = await _context.Quizzes
            .Include(q => q.Course)
            .FirstOrDefaultAsync(q => q.QuizId == quizId && q.DeleteFlag, cancellationToken);

        if (quiz == null)
            return Result.Failure<bool>("QuizNotFound");

        if (quiz.Course.DeleteFlag)
            return Result.Failure<bool>("CannotRestoreArchivedCourse");

        // Business rule: only one active quiz per course
        var existingActive = await _context.Quizzes
            .AnyAsync(q => q.CourseId == quiz.CourseId && !q.DeleteFlag && q.QuizId != quizId, cancellationToken);

        if (existingActive)
            return Result.Failure<bool>("QuizAlreadyExistsForCourse");

        quiz.DeleteFlag = false;
        quiz.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        if (_currentUserService.UserId.HasValue)
        {
            await _auditLogService.CreateAuditLogAsync(new CreateAuditLogRequest
            {
                UserId = _currentUserService.UserId.Value,
                Action = "Restore",
                TableName = "Quizzes",
                RecordId = quiz.QuizId
            }, cancellationToken);
            
            _logger.LogInformation("User {UserId} restored Quiz {QuizId}", _currentUserService.UserId.Value, quiz.QuizId);
        }

        return Result.Success(true);
    }

    // ─── Question CRUD ───────────────────────────────────────────

    public async Task<Result<QuestionResponse>> CreateQuestionAsync(CreateQuestionRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.QuestionText))
            return Result.Failure<QuestionResponse>("InvalidQuestionText");

        var quiz = await _context.Quizzes
            .FirstOrDefaultAsync(q => q.QuizId == request.QuizId && !q.DeleteFlag, cancellationToken);

        if (quiz == null)
            return Result.Failure<QuestionResponse>("QuizNotFound");

        // Validate options
        if (request.Options.Count < 2)
            return Result.Failure<QuestionResponse>("InvalidQuestionConfiguration");

        if (!request.Options.Any(o => o.IsCorrect))
            return Result.Failure<QuestionResponse>("InvalidCorrectAnswer");

        if (request.Options.Any(o => string.IsNullOrWhiteSpace(o.OptionText)))
            return Result.Failure<QuestionResponse>("InvalidOptionText");

        // Calculate display order
        int finalOrder = request.DisplayOrder;
        if (finalOrder < 1)
        {
            var maxOrder = await _context.Questions
                .Where(q => q.QuizId == request.QuizId)
                .Select(q => (int?)q.DisplayOrder)
                .MaxAsync(cancellationToken) ?? 0;
            finalOrder = maxOrder + 1;
        }

        var question = new Question
        {
            QuizId = request.QuizId,
            QuestionText = request.QuestionText.Trim(),
            DisplayOrder = finalOrder
        };

        _context.Questions.Add(question);
        await _context.SaveChangesAsync(cancellationToken);

        // Add options
        foreach (var optReq in request.Options)
        {
            var option = new QuestionOption
            {
                QuestionId = question.QuestionId,
                OptionText = optReq.OptionText.Trim(),
                IsCorrect = optReq.IsCorrect
            };
            _context.QuestionOptions.Add(option);
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Resequence all questions in this quiz
        await ResequenceQuestionsAsync(request.QuizId, cancellationToken);

        if (_currentUserService.UserId.HasValue)
        {
            await _auditLogService.CreateAuditLogAsync(new CreateAuditLogRequest
            {
                UserId = _currentUserService.UserId.Value,
                Action = "Create",
                TableName = "Questions",
                RecordId = question.QuestionId
            }, cancellationToken);
            
            _logger.LogInformation("User {UserId} created Question {QuestionId} in Quiz {QuizId}", _currentUserService.UserId.Value, question.QuestionId, question.QuizId);
        }

        return await GetQuestionResponseAsync(question.QuestionId, cancellationToken);
    }

    public async Task<Result<QuestionResponse>> UpdateQuestionAsync(int questionId, UpdateQuestionRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.QuestionText))
            return Result.Failure<QuestionResponse>("InvalidQuestionText");

        var question = await _context.Questions
            .Include(q => q.Quiz)
            .FirstOrDefaultAsync(q => q.QuestionId == questionId, cancellationToken);

        if (question == null)
            return Result.Failure<QuestionResponse>("QuestionNotFound");

        if (question.Quiz.DeleteFlag)
            return Result.Failure<QuestionResponse>("QuizNotFound");

        question.QuestionText = request.QuestionText.Trim();

        if (request.DisplayOrder >= 1 && request.DisplayOrder != question.DisplayOrder)
        {
            question.DisplayOrder = request.DisplayOrder;
        }

        await _context.SaveChangesAsync(cancellationToken);

        await ResequenceQuestionsAsync(question.QuizId, cancellationToken);

        if (_currentUserService.UserId.HasValue)
        {
            await _auditLogService.CreateAuditLogAsync(new CreateAuditLogRequest
            {
                UserId = _currentUserService.UserId.Value,
                Action = "Update",
                TableName = "Questions",
                RecordId = question.QuestionId
            }, cancellationToken);
            
            _logger.LogInformation("User {UserId} updated Question {QuestionId} in Quiz {QuizId}", _currentUserService.UserId.Value, question.QuestionId, question.QuizId);
        }

        return await GetQuestionResponseAsync(question.QuestionId, cancellationToken);
    }

    public async Task<Result<bool>> DeleteQuestionAsync(int questionId, CancellationToken cancellationToken = default)
    {
        var question = await _context.Questions
            .Include(q => q.QuestionOptions)
            .FirstOrDefaultAsync(q => q.QuestionId == questionId, cancellationToken);

        if (question == null)
            return Result.Failure<bool>("QuestionNotFound");

        int quizId = question.QuizId;

        // Remove options first
        _context.QuestionOptions.RemoveRange(question.QuestionOptions);
        _context.Questions.Remove(question);
        await _context.SaveChangesAsync(cancellationToken);

        await ResequenceQuestionsAsync(quizId, cancellationToken);

        if (_currentUserService.UserId.HasValue)
        {
            await _auditLogService.CreateAuditLogAsync(new CreateAuditLogRequest
            {
                UserId = _currentUserService.UserId.Value,
                Action = "Delete",
                TableName = "Questions",
                RecordId = questionId
            }, cancellationToken);
            
            _logger.LogInformation("User {UserId} deleted Question {QuestionId}", _currentUserService.UserId.Value, questionId);
        }

        return Result.Success(true);
    }

    // ─── Option CRUD ─────────────────────────────────────────────

    public async Task<Result<List<QuestionOptionResponse>>> GetOptionsByQuestionAsync(int questionId, CancellationToken cancellationToken = default)
    {
        var questionExists = await _context.Questions.AnyAsync(q => q.QuestionId == questionId, cancellationToken);
        if (!questionExists)
            return Result.Failure<List<QuestionOptionResponse>>("QuestionNotFound");

        var options = await _context.QuestionOptions
            .AsNoTracking()
            .Where(o => o.QuestionId == questionId)
            .Select(o => new QuestionOptionResponse
            {
                OptionId = o.OptionId,
                QuestionId = o.QuestionId,
                OptionText = o.OptionText,
                IsCorrect = o.IsCorrect
            })
            .ToListAsync(cancellationToken);

        return Result.Success(options);
    }

    public async Task<Result<QuestionOptionResponse>> CreateOptionAsync(int questionId, CreateQuestionOptionRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.OptionText))
            return Result.Failure<QuestionOptionResponse>("InvalidOptionText");

        if (request.OptionText.Length > 500)
            return Result.Failure<QuestionOptionResponse>("InvalidOptionText");

        var question = await _context.Questions
            .Include(q => q.Quiz)
            .FirstOrDefaultAsync(q => q.QuestionId == questionId, cancellationToken);

        if (question == null)
            return Result.Failure<QuestionOptionResponse>("QuestionNotFound");

        if (question.Quiz.DeleteFlag)
            return Result.Failure<QuestionOptionResponse>("QuizNotFound");

        var option = new QuestionOption
        {
            QuestionId = questionId,
            OptionText = request.OptionText.Trim(),
            IsCorrect = request.IsCorrect
        };

        _context.QuestionOptions.Add(option);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(new QuestionOptionResponse
        {
            OptionId = option.OptionId,
            QuestionId = option.QuestionId,
            OptionText = option.OptionText,
            IsCorrect = option.IsCorrect
        });
    }

    public async Task<Result<QuestionOptionResponse>> UpdateOptionAsync(int optionId, UpdateQuestionOptionRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.OptionText))
            return Result.Failure<QuestionOptionResponse>("InvalidOptionText");

        if (request.OptionText.Length > 500)
            return Result.Failure<QuestionOptionResponse>("InvalidOptionText");

        var option = await _context.QuestionOptions
            .Include(o => o.Question)
                .ThenInclude(q => q.Quiz)
            .FirstOrDefaultAsync(o => o.OptionId == optionId, cancellationToken);

        if (option == null)
            return Result.Failure<QuestionOptionResponse>("OptionNotFound");

        if (option.QuestionId != request.QuestionId)
            return Result.Failure<QuestionOptionResponse>("OptionNotInQuestion");

        if (option.Question.Quiz.DeleteFlag)
            return Result.Failure<QuestionOptionResponse>("QuizNotFound");

        // Validate correct-answer rules: if changing from correct to incorrect, ensure another correct option exists
        if (option.IsCorrect && !request.IsCorrect)
        {
            var otherCorrectExists = await _context.QuestionOptions
                .AnyAsync(o => o.QuestionId == option.QuestionId && o.OptionId != optionId && o.IsCorrect, cancellationToken);

            if (!otherCorrectExists)
                return Result.Failure<QuestionOptionResponse>("InvalidCorrectAnswer");
        }

        option.OptionText = request.OptionText.Trim();
        option.IsCorrect = request.IsCorrect;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(new QuestionOptionResponse
        {
            OptionId = option.OptionId,
            QuestionId = option.QuestionId,
            OptionText = option.OptionText,
            IsCorrect = option.IsCorrect
        });
    }

    public async Task<Result<bool>> DeleteOptionAsync(int optionId, CancellationToken cancellationToken = default)
    {
        var option = await _context.QuestionOptions
            .FirstOrDefaultAsync(o => o.OptionId == optionId, cancellationToken);

        if (option == null)
            return Result.Failure<bool>("OptionNotFound");

        // Ensure at least 2 options remain after deletion
        var remainingCount = await _context.QuestionOptions
            .CountAsync(o => o.QuestionId == option.QuestionId && o.OptionId != optionId, cancellationToken);

        if (remainingCount < 2)
            return Result.Failure<bool>("InvalidQuestionConfiguration");

        // Ensure at least one correct option remains
        if (option.IsCorrect)
        {
            var otherCorrect = await _context.QuestionOptions
                .AnyAsync(o => o.QuestionId == option.QuestionId && o.OptionId != optionId && o.IsCorrect, cancellationToken);

            if (!otherCorrect)
                return Result.Failure<bool>("InvalidCorrectAnswer");
        }

        _context.QuestionOptions.Remove(option);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }

    public async Task<Result<bool>> ArchiveOptionAsync(int optionId, CancellationToken cancellationToken = default)
    {
        var optionExists = await _context.QuestionOptions.AnyAsync(o => o.OptionId == optionId, cancellationToken);
        if (!optionExists)
            return Result.Failure<bool>("OptionNotFound");

        // Schema doesn't support DeleteFlag for options
        return Result.Failure<bool>("ArchiveNotSupported");
    }

    public async Task<Result<bool>> RestoreOptionAsync(int optionId, CancellationToken cancellationToken = default)
    {
        var optionExists = await _context.QuestionOptions.AnyAsync(o => o.OptionId == optionId, cancellationToken);
        if (!optionExists)
            return Result.Failure<bool>("OptionNotFound");

        // Schema doesn't support DeleteFlag for options
        return Result.Failure<bool>("ArchiveNotSupported");
    }

    // ─── Helpers ─────────────────────────────────────────────────

    private async Task ResequenceQuestionsAsync(int quizId, CancellationToken cancellationToken)
    {
        var questions = await _context.Questions
            .Where(q => q.QuizId == quizId)
            .OrderBy(q => q.DisplayOrder)
            .ToListAsync(cancellationToken);

        int index = 1;
        foreach (var q in questions)
        {
            if (q.DisplayOrder != index)
            {
                q.DisplayOrder = index;
            }
            index++;
        }
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<Result<QuestionResponse>> GetQuestionResponseAsync(int questionId, CancellationToken cancellationToken)
    {
        var question = await _context.Questions
            .AsNoTracking()
            .Include(q => q.QuestionOptions)
            .FirstOrDefaultAsync(q => q.QuestionId == questionId, cancellationToken);

        if (question == null)
            return Result.Failure<QuestionResponse>("QuestionNotFound");

        return Result.Success(new QuestionResponse
        {
            QuestionId = question.QuestionId,
            QuizId = question.QuizId,
            QuestionText = question.QuestionText,
            DisplayOrder = question.DisplayOrder,
            Options = question.QuestionOptions.Select(o => new QuestionOptionResponse
            {
                OptionId = o.OptionId,
                QuestionId = o.QuestionId,
                OptionText = o.OptionText,
                IsCorrect = o.IsCorrect
            }).ToList()
        });
    }

    private static QuizDetailResponse MapToDetail(Quiz quiz)
    {
        return new QuizDetailResponse
        {
            QuizId = quiz.QuizId,
            CourseId = quiz.CourseId,
            Title = quiz.Title,
            PassingScore = quiz.PassingScore,
            CreatedAt = quiz.CreatedAt,
            UpdatedAt = quiz.UpdatedAt,
            DeleteFlag = quiz.DeleteFlag,
            Questions = quiz.Questions
                .OrderBy(q => q.DisplayOrder)
                .Select(q => new QuestionResponse
                {
                    QuestionId = q.QuestionId,
                    QuizId = q.QuizId,
                    QuestionText = q.QuestionText,
                    DisplayOrder = q.DisplayOrder,
                    Options = q.QuestionOptions.Select(o => new QuestionOptionResponse
                    {
                        OptionId = o.OptionId,
                        QuestionId = o.QuestionId,
                        OptionText = o.OptionText,
                        IsCorrect = o.IsCorrect
                    }).ToList()
                }).ToList()
        };
    }
}
