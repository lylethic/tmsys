using System;
using Microsoft.EntityFrameworkCore;

namespace server.Domain.AppDataContext;

public class HangfireContext : DbContext
{
    public HangfireContext(DbContextOptions<HangfireContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Không map bất kỳ entity 
        base.OnModelCreating(modelBuilder);
    }
}
