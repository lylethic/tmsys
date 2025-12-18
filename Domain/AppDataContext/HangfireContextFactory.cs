using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace server.Domain.AppDataContext;

public class HangfireContextFactory : IDesignTimeDbContextFactory<HangfireContext>
{
    public HangfireContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<HangfireContext>();

        // Lấy connection string
        var connectionString = "Host=localhost;Port=5432;Database=tms_server;Username=postgres;Password=111111";

        optionsBuilder.UseNpgsql(connectionString);

        return new HangfireContext(optionsBuilder.Options);
    }
}
