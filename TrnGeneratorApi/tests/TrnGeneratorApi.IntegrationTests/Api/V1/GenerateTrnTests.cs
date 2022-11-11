namespace TrnGeneratorApi.IntegrationTests.Api.V1;

using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Testing;

public class GenerateTrnTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public GenerateTrnTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Post_WithoutApiKey_Returns401Unauthorised()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsync("/api/v1/trn", null);

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WithInvalidApiKey_Returns401Unauthorised()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "xyz");

        // Act
        var response = await client.PostAsync("/api/v1/trn", null);

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WithValidApiKey_Returns200OKAndTrn()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "12345");

        // Act
        var response = await client.PostAsync("/api/v1/trn", null);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var result = await response.Content.ReadAsStringAsync();
        Assert.NotNull(result);
        Assert.True(int.TryParse(result, out int value), "Result is an integer");
    }
}
