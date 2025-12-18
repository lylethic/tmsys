using CloudinaryDotNet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using server.Application.Common.Interfaces;
using server.Application.Common.Respository;
using server.Common.Filter;
using server.Common.Interfaces;
using server.Common.Models;
using server.Common.Providers;
using server.Common.Repository;
using server.Logging;
using server.Notifications;
using server.Repositories;
using server.Services;
using server.Services.Templates;

namespace server.Common.Settings;

public static class ServiceCollectionExtension
{
    public static void RegisterServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        //  Register the custom authorization filter
        services.AddScoped<AuthorizationFilter>();

        // Email
        services.AddScoped<IMailService, GmailAssistantService>();

        // automatically binds the values from appsettings.json → GmailOptions class.
        services.Configure<GmailOptions>(configuration.GetSection("GmailOptions"));

        // Notification
        services.AddSingleton<INotificationCategoryProvider, NotificationCategoryProvider>();
        services.AddScoped<NotificationService>();
        services.AddScoped<NotificationRepository>();
        services.AddScoped<IRecurringJobSetup, RecurringJobSetupService>();


        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IPermissionService, JwtPermissionService>();
        services.AddScoped<ILogManager, LoggerManager>();

        // Others
        services.AddSingleton<JwtConfig>();
        services.AddTransient<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IAuth, AuthenticationRepository>();

        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRolePermission, RolePermissionRepository>();

        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, JwtPermissionHandler>();
        services.AddTransient<IAssistantService, AssistantService>();
        services.AddTransient<SeedDataService>();

        services.AddTransient<IProjectRepository, ProjectRepository>();
        services.AddTransient<IReportRepository, ReportRepository>();
        services.AddTransient<ITaskRepository, TaskRepository>();
        services.AddTransient<IProgressUpdateRepository, ProgressUpdateRepository>();
        services.AddTransient<IProjectTypeRepository, ProjectTypeRepository>();
        services.AddTransient<IOtpRepository, OTPRepository>();
        services.AddTransient<IApprovedStatus, ApprovedRepository>();
        services.AddTransient<IDepartment, DepartmentRepository>();
        services.AddTransient<IUserDepartment, UserDepartmentRepository>();

        // Hangfire
        services.AddTransient<IJobRunService, JobRunService>();

        // Request middleware
        services.AddScoped<ClientRequestLogRepository>();

        // Cloudinary
        services.Configure<CloudinarySettings>(configuration.GetSection("CloudinarySettings"));
        services.AddSingleton(provider =>
        {
            var config = provider.GetRequiredService<IOptions<CloudinarySettings>>().Value;
            var account = new Account(
                config.CloudName,
                config.ApiKey,
                config.ApiSecret
            );
            return new Cloudinary(account);
        });
        services.AddScoped<ICloudinaryService, CloudinaryService>();
        services.AddScoped<IMediaAssetRepository, MediaAssetRepository>();

    }
}
