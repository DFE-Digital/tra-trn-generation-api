namespace TrnGeneratorApi.BenchmarkTests.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

public class TrnGeneratorDbContext : DbContext
{
    private string connectionString;

    public TrnGeneratorDbContext(IConfiguration configuration)
    {
        connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseNpgsql(connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<IntReturn>().HasNoKey()
            .ToView(null);
    }
}
