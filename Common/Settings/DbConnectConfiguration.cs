using System;
using System.Data;
using Npgsql;

namespace server.Common.Settings;

public static class DbConnectConfig
{
    public static IServiceCollection AddDbConnectConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // Testing connection
        try
        {
            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("=============== Database connected! ===============");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Database connection failed: {ex.Message}");
            Console.ResetColor();
        }

        services.AddScoped<IDbConnection>(sp => new NpgsqlConnection(connectionString));

        return services;
    }
}
