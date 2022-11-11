namespace TrnGeneratorApi.IntegrationTests.Api.V1;

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
}
