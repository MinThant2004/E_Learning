using ELearningManagementSystem.Application.Features.ExamPayments.DTOs;
using FluentValidation;

namespace ELearningManagementSystem.Application.Features.ExamPayments.Validators;

public class SubmitExamPaymentRequestValidator : AbstractValidator<SubmitExamPaymentRequest>
{
    public SubmitExamPaymentRequestValidator()
    {
        RuleFor(x => x.PaymentMethod)
            .NotEmpty().WithMessage("Payment method is required.")
            .MaximumLength(50).WithMessage("Payment method must not exceed 50 characters.");

        RuleFor(x => x.TransactionId)
            .NotEmpty().WithMessage("Transaction ID is required.")
            .MaximumLength(100).WithMessage("Transaction ID must not exceed 100 characters.");
    }
}
