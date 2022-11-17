namespace TrnGeneratorApi.Models;

using Microsoft.EntityFrameworkCore.Design;

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
