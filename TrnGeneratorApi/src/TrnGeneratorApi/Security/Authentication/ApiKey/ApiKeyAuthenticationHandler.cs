using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace TrnGeneratorApi.Security.Authentication.ApiKey;

public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    private const string AuthenticationHeaderScheme = "Bearer";

    private readonly IApiKeyValidator _apiKeyValidator;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IApiKeyValidator apiKeyValidator) : base(options, logger, encoder)
    {
        _apiKeyValidator = apiKeyValidator;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string? authorizationHeader = Request.Headers["Authorization"];

        if (string.IsNullOrEmpty(authorizationHeader))
        {
            return AuthenticateResult.NoResult();
        }

        if (!authorizationHeader.StartsWith(AuthenticationHeaderScheme + " ", StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.NoResult();
        }

        var key = authorizationHeader[(AuthenticationHeaderScheme.Length + 1)..];
        var isValidApiKey = await _apiKeyValidator.IsValidAsync(key);

        if (!isValidApiKey)
        {
            return AuthenticateResult.Fail($"Invalid API key.");
        }

        var principal = CreatePrincipal();
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }

    private static ClaimsPrincipal CreatePrincipal()
    {
        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.Name, "Service")
            },
            authenticationType: ApiKeyAuthenticationDefaults.AuthenticationScheme,
            nameType: ClaimTypes.Name,
            roleType: ClaimTypes.Role);

        return new ClaimsPrincipal(identity);
    }
}
