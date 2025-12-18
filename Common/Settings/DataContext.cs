using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;
using server.Common.Models;
using System.Data;

namespace server.Common.Settings;

public class DataContext
{
    private readonly DbSettings _dbSettings;
    private IConfiguration _config;

    public DataContext(IOptions<DbSettings> dbSettings, IConfiguration config)
    {
        _dbSettings = dbSettings.Value;
        _config = config;
    }

    public IDbConnection CreateConnection()
    {
        var connectionString = _config["ConnectionStrings:DefaultConnection"];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Missing POSTGRE_STRING in environment.");
        }

        return new NpgsqlConnection(connectionString);
    }

    public async Task Init()
    {
        await _initTables();
    }

    private async Task _initTables()
    {
        using var connection = CreateConnection();
        await _initUsers();

        async Task _initUsers()
        {
            var sql = """
                CREATE TABLE IF NOT EXISTS Users (
                    id UUID PRIMARY KEY,
                    name VARCHAR(255),
                    email TEXT,
                    password TEXT,
                    created_at VARCHAR(255),
                    updated_at VARCHAR(255)
                );
            """;
            await connection.ExecuteAsync(sql);
        }
    }
}
