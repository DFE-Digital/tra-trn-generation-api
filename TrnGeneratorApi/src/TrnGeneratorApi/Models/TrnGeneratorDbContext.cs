namespace TrnGeneratorApi.Models;

using Microsoft.EntityFrameworkCore;

public class TrnGeneratorDbContext : DbContext
{
    private string connectionString;

    public TrnGeneratorDbContext(IConfiguration configuration)
    {
        connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public DbSet<TrnInfo> TrnInfos => Set<TrnInfo>();

    public DbSet<TrnRange> TrnRanges => Set<TrnRange>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var trnInfoBuilder = modelBuilder.Entity<TrnInfo>();
        trnInfoBuilder
            .ToTable("trn_info")
            .HasKey(e => e.Trn);
        trnInfoBuilder
            .HasIndex(e => e.Trn)
            .HasFilter("is_claimed IS FALSE")
            .HasDatabaseName("ix_trn_info_unclaimed_trns");

        var trnRangeBuilder = modelBuilder.Entity<TrnRange>();
        trnRangeBuilder
            .ToTable("trn_range")
            .HasKey(e => e.FromTrn);
        trnRangeBuilder
            .HasIndex(e => e.FromTrn)
            .HasFilter("is_exhausted IS FALSE")
            .HasDatabaseName("ix_trn_range_unexhausted_trn_ranges");

        modelBuilder
            .Entity<IntReturn>().HasNoKey()
            .ToView(null);
    }
}
