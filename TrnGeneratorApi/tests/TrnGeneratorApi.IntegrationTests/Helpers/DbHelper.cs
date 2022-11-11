namespace TrnGeneratorApi.IntegrationTests.Helpers;

using Microsoft.EntityFrameworkCore;
using TrnGeneratorApi.Models;

public static class DbHelper
{
    public static async Task ResetSchema(TrnGeneratorDbContext dbContext)
    {
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.MigrateAsync();
    }
}
