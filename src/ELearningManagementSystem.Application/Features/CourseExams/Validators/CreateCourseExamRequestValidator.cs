using ELearningManagementSystem.Application.Features.CourseExams.DTOs;
using FluentValidation;

namespace ELearningManagementSystem.Application.Features.CourseExams.Validators;

public class CreateCourseExamRequestValidator : AbstractValidator<CreateCourseExamRequest>
{
    public CreateCourseExamRequestValidator()
    {
        RuleFor(x => x.CourseId)
            .GreaterThan(0).WithMessage("CourseId must be valid.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.ExamFee)
            .GreaterThanOrEqualTo(0).WithMessage("ExamFee must be non-negative.");

        RuleFor(x => x.QuestionCount)
            .GreaterThan(0).WithMessage("QuestionCount must be at least 1.");

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0).WithMessage("DurationMinutes must be at least 1 minute.");

        RuleFor(x => x.PassingScore)
            .InclusiveBetween(1, 100).WithMessage("PassingScore must be between 1% and 100%.");

        RuleFor(x => x.MaxAttempts)
            .GreaterThan(0).WithMessage("MaxAttempts must be at least 1.");
    }
}
