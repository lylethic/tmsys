using System.Reflection;
using Asp.Versioning;
using Microsoft.OpenApi.Models;

namespace server.Common.Settings
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureApiDocumentation(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory,
                    $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));

                c.SwaggerGeneratorOptions.SwaggerDocs = new Dictionary<string, OpenApiInfo>
                {
                {
                    "v1", new OpenApiInfo
                    {
                        Title = "Task Management System",
                        Version = "v1"
                    }
                }
                };

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer 12345abcdef'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new List<string>()
                }
                });
            });

            return services;
        }
    }
}