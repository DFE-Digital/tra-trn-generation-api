namespace TrnGeneratorApi.IntegrationTests.Api.V1;

using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using TrnGeneratorApi.IntegrationTests.Helpers;
using TrnGeneratorApi.Models;

public class DeleteTrnRangeTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public DeleteTrnRangeTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Delete_WithoutApiKey_Returns401Unauthorised()
    {
        // Arrange
        var fromTrn = 2000000;
        var client = _factory.CreateClient();

        // Act
        var response = await client.DeleteAsync($"/api/v1/trn-ranges/{fromTrn}");

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, (int)response.StatusCode);
    }

    [Fact]
    public async Task Delete_WithInvalidApiKey_Returns401Unauthorised()
    {
        // Arrange
        var fromTrn = 2000000;
        var testConfig = new Dictionary<string, string?>()
        {
            { "ApiKeys:0", "12345" },
            { "ApiKeys:1", "09876" }
        };

        var customFactory = _factory
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration(
                    c =>
                    {
                        _ = c.AddUserSecrets<DeleteTrnRangeTests>()
                            .AddInMemoryCollection(testConfig);
                    });
            });

        var client = customFactory
            .CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "xyz");

        // Act
        var response = await client.DeleteAsync($"/api/v1/trn-ranges/{fromTrn}");

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, (int)response.StatusCode);
    }

    [Fact]
    public async Task Delete_WithValidApiKeyAndFromTrnMatchingExistingRange_DeletesTrnRangeAndReturns200OKAndMatchingTrnRange()
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

        var fromTrn = 2000000;

        var customFactory = _factory
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration(
                    c =>
                    {
                        _ = c.AddUserSecrets<DeleteTrnRangeTests>()
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

            // Act
            var response = await client.DeleteAsync($"/api/v1/trn-ranges/{fromTrn}");

            // Assert
            Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<TrnRange>();
            Assert.NotNull(result);
            Assert.Equal(trnRange1.FromTrn, result.FromTrn);
            Assert.Equal(trnRange1.ToTrn, result.ToTrn);
            Assert.Equal(trnRange1.NextTrn, result.NextTrn);
            Assert.Equal(trnRange1.IsExhausted, result.IsExhausted);

            var trnRangeExists = await db.TrnRanges.AnyAsync(r => r.FromTrn == fromTrn);
            Assert.False(trnRangeExists);
        }
    }

    [Fact]
    public async Task Delete_WithValidApiKeyAndFromTrnMatchingAlreadyExhaustedRange_Returns400BadRequest()
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
            IsExhausted = true
        };
        var trnRanges = new[]
        {
            trnRange1,
            trnRange2
        };

        var fromTrn = 3000000;

        var customFactory = _factory
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration(
                    c =>
                    {
                        _ = c.AddUserSecrets<DeleteTrnRangeTests>()
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

            // Act
            var response = await client.DeleteAsync($"/api/v1/trn-ranges/{fromTrn}");

            // Assert
            Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        }
    }

    [Fact]
    public async Task Delete_WithValidApiKeyAndFromTrnMatchingPartiallyUsedRange_Returns400BadRequest()
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
            NextTrn = 2000011,
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

        var fromTrn = 2000000;

        var customFactory = _factory
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration(
                    c =>
                    {
                        _ = c.AddUserSecrets<DeleteTrnRangeTests>()
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

            // Act
            var response = await client.DeleteAsync($"/api/v1/trn-ranges/{fromTrn}");

            // Assert
            Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        }
    }

    [Fact]
    public async Task Delete_WithValidApiKeyButFromTrnNotMatchingExistingRange_Returns404NotFound()
    {
        // Arrange
        var fromTrn = 2000000;
        var testConfig = new Dictionary<string, string?>()
        {
            { "ApiKeys:0", "12345" },
            { "ApiKeys:1", "09876" }
        };

        var customFactory = _factory
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration(
                    c =>
                    {
                        _ = c.AddUserSecrets<DeleteTrnRangeTests>()
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
        var response = await client.DeleteAsync($"/api/v1/trn-ranges/{fromTrn}");

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }
}
