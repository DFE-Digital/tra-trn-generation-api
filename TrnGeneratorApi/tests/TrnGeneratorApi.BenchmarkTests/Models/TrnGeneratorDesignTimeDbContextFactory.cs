namespace TrnGeneratorApi.BenchmarkTests.Models;

using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

public class TrnGeneratorDesignTimeDbContextFactory : IDesignTimeDbContextFactory<TrnGeneratorDbContext>
{
    public TrnGeneratorDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<TrnGeneratorDesignTimeDbContextFactory>(optional: true)  // Optional for CI
            .Build();

        return new TrnGeneratorDbContext(configuration);
    }
}
