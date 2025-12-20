using DotNetEnv;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using server.Application.Common.Respository;
using server.Common.AutoMappers;
using server.Common.CoreConstans;
using server.Common.Interfaces;
using server.Common.Middlewares;
using server.Common.Models;
using server.Common.Settings;
using server.Hubs;
using server.Services;
using System.Text.Json.Serialization;

public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
        LoadEnvironmentVariables();
        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    // Configure Services
    public void ConfigureServices(IServiceCollection services, WebApplicationBuilder builder)
    {
        // HTTP Clients
        ConfigureHttpClients(services);

        // Controllers & JSON
        services
            .AddControllers(options => options.SuppressAsyncSuffixInActionNames = false)
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

        // Database
        services.AddDbConnectConfiguration(Configuration);

        // API Documentation (sử dụng extension method có sẵn)
        services.ConfigureApiDocumentation();

        // CORS
        ConfigureCors(services);

        // Application Services
        services.RegisterServices(Configuration);
        services.AddConfiguredApiVersioning();

        // AutoMapper
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        services.AddAutoMapper(typeof(AppAutoMapper));

        // Background Jobs (Hangfire)
        ConfigureHangfire(services);

        // Real-time Communication
        services.AddSignalR();
        services.AddHostedService<Worker>();

        // Caching & Security
        services.AddMemoryCache();
        services.AddAuthorization();
        services.AddJwtAuthentication(Configuration);

        // Logging
        ConfigureLogging(builder);
    }

    // Configure Application & Middleware Pipeline
    public void Configure(WebApplication app)
    {
        // Initialize email Templates
        var env = app.Services.GetRequiredService<IWebHostEnvironment>();
        EmailTemplateManager.Initialize(env);

        // Base Path Configuration
        app.UsePathBase("/tms/api");

        // Development Environment
        if (app.Environment.IsDevelopment())
        {
            ConfigureSwaggerUI(app);
        }

        // Middleware Pipeline
        app.UseHttpsRedirection();
        app.UseCors("MyCors");

        app.UseDefaultFiles(); // serve index.html by default
        // Static Files should be served before routing
        app.UseStaticFiles();
        app.UseRouting();

        // Custom Middlewares
        app.UseMiddleware<InterceptorHttpLoggingMiddleware>();
        app.UseMiddleware<ErrorHandlerMiddleware>();

        // Hangfire Dashboard & Jobs
        ConfigureHangfireJobs(app);

        // Authentication & Authorization
        app.UseAuthentication();
        app.UseAuthorization();

        // Rate Limiting
        app.UseMiddleware<RequestLoggingMiddleware>();
        ConfigureRateLimiting(app);

        // Endpoints
        app.MapHub<NotificationHub>("/hubs/notifications");
        app.MapControllers();
    }

    public void ConfigureHttpClients(IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddHttpClient("ChatpGPT", client =>
        {
            client.BaseAddress = new Uri("https://api.openai.com/");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });
    }

    // ========================================================================
    // PRIVATE HELPER METHODS
    // ========================================================================

    public void LoadEnvironmentVariables()
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

        if (File.Exists($".env.{environment}"))
        {
            Env.Load($".env.{environment}");
        }
        else if (File.Exists(".env"))
        {
            Env.Load();
        }
    }

    public void ConfigureCors(IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("MyCors", policy =>
            {
                policy.WithOrigins("http://localhost:3000", "http://localhost:3001", "http://localhost:3002", "http://localhost:4000", "https://yourdomain.com")
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });
        });
    }

    public void ConfigureHangfire(IServiceCollection services)
    {
        services.AddHangfire(config =>
            config.UsePostgreSqlStorage(
                Configuration.GetConnectionString("DefaultConnection"),
                new PostgreSqlStorageOptions
                {
                    SchemaName = "hangfire",
                    PrepareSchemaIfNecessary = true
                }
            )
        );
        services.AddHangfireServer();
    }

    public void ConfigureLogging(WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.AddDebug();
    }

    public void ConfigureSwaggerUI(WebApplication app)
    {
        app.UseSwagger(c =>
        {
            c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
            {
                var serverUrl = $"{httpReq.Scheme}://{httpReq.Host.Value}/tms/api";
                swaggerDoc.Servers =
                [
                    new OpenApiServer { Url = serverUrl, Description = "Current Server" }
                ];
            });
        });

        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "My system v1");
            c.RoutePrefix = "swagger";
        });
    }

    public void ConfigureHangfireJobs(WebApplication app)
    {
        app.UseHangfireDashboard("/tms/hangfire");

        using var scope = app.Services.CreateScope();
        var recurringJobs = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
        var setups = scope.ServiceProvider.GetServices<IRecurringJobSetup>();

        foreach (var setup in setups)
        {
            setup.RegisterJobs(recurringJobs);
        }
    }

    public void ConfigureRateLimiting(WebApplication app)
    {
        var rateLimitRule = new RateLimitRuleModel
        {
            Limit = 5,
            Window = TimeSpan.FromSeconds(10)
        };

        app.UseMiddleware<RateLimitingMiddleware>(
            app.Services.GetRequiredService<IMemoryCache>(),
            rateLimitRule
        );
    }
}