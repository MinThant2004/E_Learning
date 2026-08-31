using ELearningManagementSystem.Application.Common;
using ELearningManagementSystem.Application.Features.PaymentMethods.DTOs;

namespace ELearningManagementSystem.Application.Features.PaymentMethods.Services;

public interface IPaymentMethodService
{
    Task<Result<List<PaymentMethodResponse>>> GetActivePaymentMethodsAsync(CancellationToken cancellationToken = default);
    Task<Result<List<PaymentMethodResponse>>> GetAllPaymentMethodsAdminAsync(CancellationToken cancellationToken = default);
    Task<Result<PaymentMethodResponse>> CreatePaymentMethodAsync(CreatePaymentMethodRequest request, int userId, CancellationToken cancellationToken = default);
    Task<Result<PaymentMethodResponse>> UpdatePaymentMethodAsync(int id, UpdatePaymentMethodRequest request, int userId, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeletePaymentMethodAsync(int id, int userId, CancellationToken cancellationToken = default);
}
