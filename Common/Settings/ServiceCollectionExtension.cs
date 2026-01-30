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
        services.AddScoped<IAssistantService, AssistantService>();
        services.AddTransient<SeedDataService>();

        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<IProgressUpdateRepository, ProgressUpdateRepository>();
        services.AddScoped<IProjectTypeRepository, ProjectTypeRepository>();
        services.AddScoped<IOtpRepository, OTPRepository>();
        services.AddScoped<IApprovedStatus, ApprovedRepository>();
        services.AddScoped<IDepartment, DepartmentRepository>();
        services.AddScoped<IUserDepartment, UserDepartmentRepository>();
        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<IWorkSchedule, WorkScheduleRepository>();
        services.AddScoped<IPopup, PopupRepository>();
        services.AddScoped<IAttendanceService, AttendanceCheckinRepository>();
        services.AddScoped<ICompanyGeofence, CompanyGeofenceRepository>();
        services.AddScoped<ISubmissionRepository, SubmissionRepository>();
        services.AddScoped<IProjectMember, ProjectMemberRepository>();

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
