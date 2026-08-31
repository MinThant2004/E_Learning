using ELearningManagementSystem.Application.Interfaces;
using ELearningManagementSystem.Infrastructure.Email;
using ELearningManagementSystem.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ELearningManagementSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        return AddInfrastructure(services, default);
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration? configuration)
    {
        services.AddHttpContextAccessor();

        // Security services
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Email
        if (configuration is not null)
        {
            var smtpSettings = configuration.GetSection("SmtpSettings").Get<SmtpSettings>()
                ?? new SmtpSettings();
            services.AddSingleton(smtpSettings);
            services.AddScoped<IEmailSender, SmtpEmailSender>();
        }

        // Dynamic RBAC Authorization pipeline components
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        return services;
    }
}
