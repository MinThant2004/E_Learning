using ELearningManagementSystem.Application.Features.CourseExams.DTOs;
using FluentValidation;

namespace ELearningManagementSystem.Application.Features.CourseExams.Validators;

public class CreateExamQuestionRequestValidator : AbstractValidator<CreateExamQuestionRequest>
{
    public CreateExamQuestionRequestValidator()
    {
        RuleFor(x => x.QuestionText)
            .NotEmpty().WithMessage("Question text is required.");

        RuleFor(x => x.Options)
            .NotEmpty().WithMessage("At least two options are required.")
            .Must(x => x != null && x.Count >= 2).WithMessage("A question must have at least 2 options.")
            .Must(x => x != null && x.Count(o => o.IsCorrect) == 1).WithMessage("Exactly one option must be marked as correct.");

        RuleForEach(x => x.Options).ChildRules(option =>
        {
            option.RuleFor(o => o.OptionText)
                .NotEmpty().WithMessage("Option text is required.");
        });
    }
}
