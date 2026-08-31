using ELearningManagementSystem.Application.Common;
using ELearningManagementSystem.Application.Features.CourseExams.DTOs;
using ELearningManagementSystem.Application.Features.AuditLogs.DTOs;
using ELearningManagementSystem.Application.Features.AuditLogs.Services;
using ELearningManagementSystem.Application.Interfaces;
using ELearningManagementSystem.Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ELearningManagementSystem.Application.Features.CourseExams.Services;

public class CourseExamService : ICourseExamService
{
    private readonly IAppDbContext _context;
    private readonly ILogger<CourseExamService> _logger;
    private readonly IValidator<CreateCourseExamRequest> _createExamValidator;
    private readonly IValidator<CreateExamQuestionRequest> _createQuestionValidator;
    private readonly IAuditLogService _auditLogService;

    public CourseExamService(
        IAppDbContext context,
        ILogger<CourseExamService> logger,
        IValidator<CreateCourseExamRequest> createExamValidator,
        IValidator<CreateExamQuestionRequest> createQuestionValidator,
        IAuditLogService auditLogService)
    {
        _context = context;
        _logger = logger;
        _createExamValidator = createExamValidator;
        _createQuestionValidator = createQuestionValidator;
        _auditLogService = auditLogService;
    }

    public async Task<Result<PagedResult<CourseExamResponse>>> GetPagedExamsAsync(ExamListQuery query, CancellationToken cancellationToken = default)
    {
        var dbQuery = _context.CourseExams
            .Include(e => e.Course)
            .Include(e => e.ExamQuestions.Where(q => !q.DeleteFlag))
            .AsNoTracking();

        if (query.IsArchived.HasValue)
        {
            dbQuery = dbQuery.Where(e => e.DeleteFlag == query.IsArchived.Value);
        }

        if (query.CourseId.HasValue)
        {
            dbQuery = dbQuery.Where(e => e.CourseId == query.CourseId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.Trim().ToLower();
            dbQuery = dbQuery.Where(e => e.Title.ToLower().Contains(term) || e.Course.Title.ToLower().Contains(term));
        }

        var totalCount = await dbQuery.CountAsync(cancellationToken);

        var items = await dbQuery
            .OrderByDescending(e => e.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(e => new CourseExamResponse
            {
                ExamId = e.ExamId,
                CourseId = e.CourseId,
                CourseTitle = e.Course.Title,
                Title = e.Title,
                Description = e.Description,
                ExamFee = e.ExamFee,
                QuestionCount = e.QuestionCount,
                DurationMinutes = e.DurationMinutes,
                PassingScore = e.PassingScore,
                MaxAttempts = e.MaxAttempts,
                PoolQuestionCount = e.ExamQuestions.Count(q => !q.DeleteFlag),
                Status = e.Status,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt,
                DeleteFlag = e.DeleteFlag
            })
            .ToListAsync(cancellationToken);

        return Result.Success(new PagedResult<CourseExamResponse>(items, totalCount, query.Page, query.PageSize));
    }

    public async Task<Result<CourseExamResponse>> GetExamByIdAsync(int examId, CancellationToken cancellationToken = default)
    {
        var exam = await _context.CourseExams
            .Include(e => e.Course)
            .Include(e => e.ExamQuestions.Where(q => !q.DeleteFlag))
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.ExamId == examId, cancellationToken);

        if (exam == null)
            return Result.Failure<CourseExamResponse>("ExamNotFound: Exam does not exist.");

        var response = new CourseExamResponse
        {
            ExamId = exam.ExamId,
            CourseId = exam.CourseId,
            CourseTitle = exam.Course.Title,
            Title = exam.Title,
            Description = exam.Description,
            ExamFee = exam.ExamFee,
            QuestionCount = exam.QuestionCount,
            DurationMinutes = exam.DurationMinutes,
            PassingScore = exam.PassingScore,
            MaxAttempts = exam.MaxAttempts,
            PoolQuestionCount = exam.ExamQuestions.Count(q => !q.DeleteFlag),
            Status = exam.Status,
            CreatedAt = exam.CreatedAt,
            UpdatedAt = exam.UpdatedAt,
            DeleteFlag = exam.DeleteFlag
        };

        return Result.Success(response);
    }

    public async Task<Result<CourseExamResponse>> CreateExamAsync(CreateCourseExamRequest request, int userId, CancellationToken cancellationToken = default)
    {
        var validation = await _createExamValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result.Failure<CourseExamResponse>($"ValidationError: {validation.Errors.First().ErrorMessage}");

        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.CourseId == request.CourseId && !c.DeleteFlag, cancellationToken);

        if (course == null)
            return Result.Failure<CourseExamResponse>("CourseNotFound: Associated course was not found or is archived.");

        var existingExam = await _context.CourseExams
            .FirstOrDefaultAsync(e => e.CourseId == request.CourseId && !e.DeleteFlag, cancellationToken);

        if (existingExam != null)
            return Result.Failure<CourseExamResponse>("ExamAlreadyExists: An active exam already exists for this course.");

        var exam = new CourseExam
        {
            CourseId = request.CourseId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            ExamFee = request.ExamFee,
            QuestionCount = request.QuestionCount,
            DurationMinutes = request.DurationMinutes,
            PassingScore = request.PassingScore,
            MaxAttempts = request.MaxAttempts,
            Status = true,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow,
            DeleteFlag = false
        };

        _context.CourseExams.Add(exam);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLogService.CreateAuditLogAsync(new CreateAuditLogRequest
        {
            UserId = userId,
            Action = "Create",
            TableName = "CourseExams",
            RecordId = exam.ExamId,
            Changes = new List<AuditLogChangeDto>
            {
                new() { Field = "CourseId", NewValue = request.CourseId.ToString() },
                new() { Field = "Title", NewValue = exam.Title },
                new() { Field = "ExamFee", NewValue = exam.ExamFee.ToString("N0") }
            }
        }, cancellationToken);

        _logger.LogInformation("Admin {UserId} created CourseExam {ExamId} for Course {CourseId}", userId, exam.ExamId, request.CourseId);

        return await GetExamByIdAsync(exam.ExamId, cancellationToken);
    }

    public async Task<Result<CourseExamResponse>> UpdateExamAsync(int examId, UpdateCourseExamRequest request, int userId, CancellationToken cancellationToken = default)
    {
        var exam = await _context.CourseExams
            .FirstOrDefaultAsync(e => e.ExamId == examId, cancellationToken);

        if (exam == null)
            return Result.Failure<CourseExamResponse>("ExamNotFound: Exam does not exist.");

        var oldTitle = exam.Title;
        var oldFee = exam.ExamFee;
        var oldQuestionCount = exam.QuestionCount;
        var oldDuration = exam.DurationMinutes;
        var oldPassingScore = exam.PassingScore;
        var oldMaxAttempts = exam.MaxAttempts;

        exam.Title = request.Title.Trim();
        exam.Description = request.Description?.Trim();
        exam.ExamFee = request.ExamFee;
        exam.QuestionCount = request.QuestionCount;
        exam.DurationMinutes = request.DurationMinutes;
        exam.PassingScore = request.PassingScore;
        exam.MaxAttempts = request.MaxAttempts;
        exam.Status = request.Status;
        exam.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        await _auditLogService.CreateAuditLogAsync(new CreateAuditLogRequest
        {
            UserId = userId,
            Action = "Update",
            TableName = "CourseExams",
            RecordId = exam.ExamId,
            Changes = new List<AuditLogChangeDto>
            {
                new() { Field = "Title", OldValue = oldTitle, NewValue = exam.Title },
                new() { Field = "ExamFee", OldValue = oldFee.ToString("N0"), NewValue = exam.ExamFee.ToString("N0") },
                new() { Field = "QuestionCount", OldValue = oldQuestionCount.ToString(), NewValue = exam.QuestionCount.ToString() },
                new() { Field = "DurationMinutes", OldValue = oldDuration.ToString(), NewValue = exam.DurationMinutes.ToString() },
                new() { Field = "PassingScore", OldValue = oldPassingScore.ToString(), NewValue = exam.PassingScore.ToString() },
                new() { Field = "MaxAttempts", OldValue = oldMaxAttempts.ToString(), NewValue = exam.MaxAttempts.ToString() }
            }
        }, cancellationToken);

        _logger.LogInformation("Admin {UserId} updated CourseExam {ExamId}", userId, examId);

        return await GetExamByIdAsync(examId, cancellationToken);
    }

    public async Task<Result> ArchiveExamAsync(int examId, int userId, CancellationToken cancellationToken = default)
    {
        var exam = await _context.CourseExams.FirstOrDefaultAsync(e => e.ExamId == examId, cancellationToken);
        if (exam == null)
            return Result.Failure("ExamNotFound: Exam does not exist.");

        exam.DeleteFlag = true;
        exam.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLogService.CreateAuditLogAsync(new CreateAuditLogRequest
        {
            UserId = userId,
            Action = "Archive",
            TableName = "CourseExams",
            RecordId = exam.ExamId
        }, cancellationToken);

        _logger.LogInformation("Admin {UserId} archived CourseExam {ExamId}", userId, examId);
        return Result.Success();
    }

    public async Task<Result> RestoreExamAsync(int examId, int userId, CancellationToken cancellationToken = default)
    {
        var exam = await _context.CourseExams.FirstOrDefaultAsync(e => e.ExamId == examId, cancellationToken);
        if (exam == null)
            return Result.Failure("ExamNotFound: Exam does not exist.");

        exam.DeleteFlag = false;
        exam.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLogService.CreateAuditLogAsync(new CreateAuditLogRequest
        {
            UserId = userId,
            Action = "Restore",
            TableName = "CourseExams",
            RecordId = exam.ExamId
        }, cancellationToken);

        _logger.LogInformation("Admin {UserId} restored CourseExam {ExamId}", userId, examId);
        return Result.Success();
    }

    public async Task<Result<List<ExamQuestionResponse>>> GetQuestionPoolAsync(int examId, CancellationToken cancellationToken = default)
    {
        var questions = await _context.ExamQuestions
            .Include(q => q.ExamQuestionOptions.Where(o => !o.DeleteFlag))
            .Where(q => q.ExamId == examId && !q.DeleteFlag)
            .OrderBy(q => q.DisplayOrder)
            .Select(q => new ExamQuestionResponse
            {
                ExamQuestionId = q.ExamQuestionId,
                ExamId = q.ExamId,
                QuestionText = q.QuestionText,
                DisplayOrder = q.DisplayOrder,
                DifficultyLevel = q.DifficultyLevel,
                Options = q.ExamQuestionOptions.Select(o => new ExamQuestionOptionResponse
                {
                    OptionId = o.OptionId,
                    ExamQuestionId = o.ExamQuestionId,
                    OptionText = o.OptionText,
                    IsCorrect = o.IsCorrect
                }).ToList()
            })
            .ToListAsync(cancellationToken);

        return Result.Success(questions);
    }

    public async Task<Result<ExamQuestionResponse>> AddQuestionToPoolAsync(int examId, CreateExamQuestionRequest request, int userId, CancellationToken cancellationToken = default)
    {
        var validation = await _createQuestionValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result.Failure<ExamQuestionResponse>($"ValidationError: {validation.Errors.First().ErrorMessage}");

        var exam = await _context.CourseExams.FirstOrDefaultAsync(e => e.ExamId == examId && !e.DeleteFlag, cancellationToken);
        if (exam == null)
            return Result.Failure<ExamQuestionResponse>("ExamNotFound: Exam does not exist.");

        var maxOrder = await _context.ExamQuestions
            .Where(q => q.ExamId == examId && !q.DeleteFlag)
            .MaxAsync(q => (int?)q.DisplayOrder, cancellationToken) ?? 0;

        var question = new ExamQuestion
        {
            ExamId = examId,
            QuestionText = request.QuestionText.Trim(),
            DisplayOrder = maxOrder + 1,
            DifficultyLevel = string.IsNullOrWhiteSpace(request.DifficultyLevel) ? "Medium" : request.DifficultyLevel.Trim(),
            CreatedAt = DateTime.UtcNow,
            DeleteFlag = false,
            ExamQuestionOptions = request.Options.Select(o => new ExamQuestionOption
            {
                OptionText = o.OptionText.Trim(),
                IsCorrect = o.IsCorrect,
                DeleteFlag = false
            }).ToList()
        };

        _context.ExamQuestions.Add(question);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLogService.CreateAuditLogAsync(new CreateAuditLogRequest
        {
            UserId = userId,
            Action = "Create",
            TableName = "ExamQuestions",
            RecordId = question.ExamQuestionId,
            Changes = new List<AuditLogChangeDto>
            {
                new() { Field = "ExamId", NewValue = examId.ToString() },
                new() { Field = "QuestionText", NewValue = question.QuestionText }
            }
        }, cancellationToken);

        _logger.LogInformation("Admin {UserId} added question {QuestionId} to QuestionPool for Exam {ExamId}", userId, question.ExamQuestionId, examId);

        var response = new ExamQuestionResponse
        {
            ExamQuestionId = question.ExamQuestionId,
            ExamId = question.ExamId,
            QuestionText = question.QuestionText,
            DisplayOrder = question.DisplayOrder,
            DifficultyLevel = question.DifficultyLevel,
            Options = question.ExamQuestionOptions.Select(o => new ExamQuestionOptionResponse
            {
                OptionId = o.OptionId,
                ExamQuestionId = o.ExamQuestionId,
                OptionText = o.OptionText,
                IsCorrect = o.IsCorrect
            }).ToList()
        };

        return Result.Success(response);
    }

    public async Task<Result<ExamQuestionResponse>> UpdateQuestionInPoolAsync(int questionId, UpdateExamQuestionRequest request, int userId, CancellationToken cancellationToken = default)
    {
        var createRequest = new CreateExamQuestionRequest
        {
            QuestionText = request.QuestionText,
            DifficultyLevel = request.DifficultyLevel,
            Options = request.Options
        };

        var validation = await _createQuestionValidator.ValidateAsync(createRequest, cancellationToken);
        if (!validation.IsValid)
            return Result.Failure<ExamQuestionResponse>($"ValidationError: {validation.Errors.First().ErrorMessage}");

        var question = await _context.ExamQuestions
            .Include(q => q.ExamQuestionOptions)
            .FirstOrDefaultAsync(q => q.ExamQuestionId == questionId && !q.DeleteFlag, cancellationToken);

        if (question == null)
            return Result.Failure<ExamQuestionResponse>("QuestionNotFound: Question does not exist.");

        question.QuestionText = request.QuestionText.Trim();
        question.DifficultyLevel = string.IsNullOrWhiteSpace(request.DifficultyLevel) ? "Medium" : request.DifficultyLevel.Trim();
        question.UpdatedAt = DateTime.UtcNow;

        // Remove old options
        _context.QuestionOptions.RemoveRange(_context.QuestionOptions.Where(o => o.QuestionId == questionId)); // Safety check if any mapped
        foreach (var existingOpt in question.ExamQuestionOptions)
        {
            existingOpt.DeleteFlag = true;
        }

        // Add new options
        foreach (var opt in request.Options)
        {
            question.ExamQuestionOptions.Add(new ExamQuestionOption
            {
                OptionText = opt.OptionText.Trim(),
                IsCorrect = opt.IsCorrect,
                DeleteFlag = false
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        await _auditLogService.CreateAuditLogAsync(new CreateAuditLogRequest
        {
            UserId = userId,
            Action = "Update",
            TableName = "ExamQuestions",
            RecordId = question.ExamQuestionId,
            Changes = new List<AuditLogChangeDto>
            {
                new() { Field = "QuestionText", NewValue = question.QuestionText },
                new() { Field = "DifficultyLevel", NewValue = question.DifficultyLevel }
            }
        }, cancellationToken);

        _logger.LogInformation("Admin {UserId} updated ExamQuestion {QuestionId}", userId, questionId);

        var response = new ExamQuestionResponse
        {
            ExamQuestionId = question.ExamQuestionId,
            ExamId = question.ExamId,
            QuestionText = question.QuestionText,
            DisplayOrder = question.DisplayOrder,
            DifficultyLevel = question.DifficultyLevel,
            Options = question.ExamQuestionOptions.Where(o => !o.DeleteFlag).Select(o => new ExamQuestionOptionResponse
            {
                OptionId = o.OptionId,
                ExamQuestionId = o.ExamQuestionId,
                OptionText = o.OptionText,
                IsCorrect = o.IsCorrect
            }).ToList()
        };

        return Result.Success(response);
    }

    public async Task<Result> DeleteQuestionFromPoolAsync(int questionId, int userId, CancellationToken cancellationToken = default)
    {
        var question = await _context.ExamQuestions.FirstOrDefaultAsync(q => q.ExamQuestionId == questionId, cancellationToken);
        if (question == null)
            return Result.Failure("QuestionNotFound: Question does not exist.");

        question.DeleteFlag = true;
        question.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        await _auditLogService.CreateAuditLogAsync(new CreateAuditLogRequest
        {
            UserId = userId,
            Action = "Delete",
            TableName = "ExamQuestions",
            RecordId = question.ExamQuestionId
        }, cancellationToken);

        _logger.LogInformation("Admin {UserId} deleted ExamQuestion {QuestionId} from pool", userId, questionId);
        return Result.Success();
    }
}
