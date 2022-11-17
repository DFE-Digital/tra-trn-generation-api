namespace TrnGeneratorApi.IntegrationTests.Api.V1;

using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using TrnGeneratorApi.IntegrationTests.Helpers;
using TrnGeneratorApi.Models;
using TrnGeneratorApi.Requests;
using TrnGeneratorApi.Responses;

public class PostTrnRangeTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public PostTrnRangeTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Post_WithoutApiKey_Returns401Unauthorised()
    {
        // Arrange
        var newTrnRange = new CreateTrnRangeRequest()
        {
            FromTrn = 2000000,
            ToTrn = 2000999
        };

        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/trn-ranges", newTrnRange);

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WithInvalidApiKey_Returns401Unauthorised()
    {
        // Arrange
        var testConfig = new Dictionary<string, string?>()
        {
            { "ApiKeys:0", "12345" },
            { "ApiKeys:1", "09876" }
        };

        var newTrnRange = new CreateTrnRangeRequest()
        {
            FromTrn = 2000000,
            ToTrn = 2000999
        };

        using var customFactory = _factory
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration(
                    c =>
                    {
                        _ = c.AddUserSecrets<PostTrnRangeTests>()
                            .AddInMemoryCollection(testConfig);
                    });
            });

        var client = customFactory
            .CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "xyz");

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/trn-ranges", newTrnRange);

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WithValidApiKey_InsertsNewTrnRangeAndReturns201CreatedAndNewTrnRange()
    {
        // Arrange
        var testConfig = new Dictionary<string, string?>()
        {
            { "ApiKeys:0", "12345" },
            { "ApiKeys:1", "09876" }
        };

        var newTrnRange = new CreateTrnRangeRequest()
        {
            FromTrn = 2000000,
            ToTrn = 2000999
        };

        using var customFactory = _factory
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration(
                    c =>
                    {
                        _ = c.AddUserSecrets<PostTrnRangeTests>()
                            .AddInMemoryCollection(testConfig);
                    });
            });

        var client = customFactory
            .CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "09876");

        using (var scope = customFactory.Services.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<TrnGeneratorDbContext>();

            await DbHelper.ResetSchema(db);

            // Act
            var response = await client.PostAsJsonAsync("/api/v1/trn-ranges", newTrnRange);

            // Assert
            Assert.Equal(StatusCodes.Status201Created, (int)response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<CreateTrnRangeResponse>();
            Assert.NotNull(result);
            Assert.Equal(newTrnRange.FromTrn, result.FromTrn);
            Assert.Equal(newTrnRange.ToTrn, result.ToTrn);
            Assert.Equal(newTrnRange.FromTrn, result.NextTrn);
            Assert.False(result.IsExhausted);

            var trnRangeExists = await db.TrnRanges.AnyAsync(r => r.FromTrn == newTrnRange.FromTrn);
            Assert.True(trnRangeExists);
        }
    }

    [Fact]
    public async Task Post_WithValidApiKeyButOverlappingRange_Returns400BadRequest()
    {
        // Arrange
        var testConfig = new Dictionary<string, string?>()
        {
            { "ApiKeys:0", "12345" },
            { "ApiKeys:1", "09876" }
        };

        var trnRange1 = new TrnRange()
        {
            FromTrn = 2000000,
            ToTrn = 2000999,
            NextTrn = 2000000,
            IsExhausted = false
        };
        var trnRange2 = new TrnRange()
        {
            FromTrn = 3000000,
            ToTrn = 3000999,
            NextTrn = 3000000,
            IsExhausted = false
        };
        var trnRanges = new[]
        {
            trnRange1,
            trnRange2
        };

        var newTrnRange = new CreateTrnRangeRequest()
        {
            FromTrn = 2000500,
            ToTrn = 3000450
        };

        using var customFactory = _factory
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration(
                    c =>
                    {
                        _ = c.AddUserSecrets<PostTrnRangeTests>()
                            .AddInMemoryCollection(testConfig);
                    });
            });

        var client = customFactory
            .CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "09876");

        using (var scope = customFactory.Services.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<TrnGeneratorDbContext>();

            await DbHelper.ResetSchema(db);
            await db.TrnRanges.AddRangeAsync(trnRanges);
            await db.SaveChangesAsync();
        }

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/trn-ranges", newTrnRange);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WithValidApiKeyButFromTrnGreaterThanToTrn_Returns400BadRequest()
    {
        // Arrange
        var testConfig = new Dictionary<string, string?>()
        {
            { "ApiKeys:0", "12345" },
            { "ApiKeys:1", "09876" }
        };

        var newTrnRange = new CreateTrnRangeRequest()
        {
            FromTrn = 2000000,
            ToTrn = 1000000
        };

        using var customFactory = _factory
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration(
                    c =>
                    {
                        _ = c.AddUserSecrets<PostTrnRangeTests>()
                            .AddInMemoryCollection(testConfig);
                    });
            });

        var client = customFactory
            .CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "09876");

        using (var scope = customFactory.Services.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<TrnGeneratorDbContext>();

            await DbHelper.ResetSchema(db);
        }

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/trn-ranges", newTrnRange);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }
}
