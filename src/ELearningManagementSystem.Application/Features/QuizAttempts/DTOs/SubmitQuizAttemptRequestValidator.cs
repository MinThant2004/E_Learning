using FluentValidation;

namespace ELearningManagementSystem.Application.Features.QuizAttempts.DTOs;

public class SubmitQuizAttemptRequestValidator : AbstractValidator<SubmitQuizAttemptRequest>
{
    public SubmitQuizAttemptRequestValidator()
    {
        RuleFor(x => x.Answers)
            .NotEmpty().WithMessage("Answers cannot be empty.");

        RuleForEach(x => x.Answers)
            .SetValidator(new SubmitQuizAnswerRequestValidator());
            
        RuleFor(x => x.Answers)
            .Must(x => x.Select(a => a.QuestionId).Distinct().Count() == x.Count)
            .WithMessage("Duplicate answers for the same question are not allowed.");
    }
}

public class SubmitQuizAnswerRequestValidator : AbstractValidator<SubmitQuizAnswerRequest>
{
    public SubmitQuizAnswerRequestValidator()
    {
        RuleFor(x => x.QuestionId)
            .GreaterThan(0).WithMessage("Valid QuestionId is required.");

        RuleFor(x => x.SelectedOptionId)
            .GreaterThan(0).WithMessage("Valid SelectedOptionId is required.");
    }
}
