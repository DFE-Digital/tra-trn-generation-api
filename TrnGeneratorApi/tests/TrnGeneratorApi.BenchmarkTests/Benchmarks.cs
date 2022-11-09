namespace TrnGeneratorApi.BenchmarkTests;

using System.Linq;
using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TrnGeneratorApi.BenchmarkTests.Models;

[MaxIterationCount(100)]
[MinColumn, MaxColumn, IterationsColumn]
[InvocationCount(100, 50)]
[InProcess]
public class Benchmarks
{
    private TrnGeneratorDbContext dbContext;

    public Benchmarks()
    {
        var config = new ConfigurationBuilder()
            .AddUserSecrets<Benchmarks>()
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(config);
        services.AddDbContext<TrnGeneratorDbContext>();

        var serviceProvider = services.BuildServiceProvider();
        dbContext = serviceProvider.GetRequiredService<TrnGeneratorDbContext>();
    }

    [Benchmark]
    public int? GenerateTrnsUsingRowPerTrn()
    {
        var nextTrn = dbContext
                    .Set<IntReturn>()
                    .FromSqlRaw("SELECT \"fn_generate_trn\" as Value FROM fn_generate_trn()")
                    .AsEnumerable()
                    .FirstOrDefault();

        if (nextTrn != null)
        {
            return nextTrn.Value;
        }
        else
        {
            return -1;
        }
    }

    [Benchmark]
    public int? GenerateTrnsUsingRowPerTrnRange()
    {
        var nextTrn = dbContext
                    .Set<IntReturn>()
                    .FromSqlRaw("SELECT \"fn_generate_trn_from_range\" as Value FROM fn_generate_trn_from_range()")
                    .AsEnumerable()
                    .FirstOrDefault();

        if (nextTrn != null)
        {
            return nextTrn.Value;
        }
        else
        {
            return -1;
        }
    }
}
