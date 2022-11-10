using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace TrnGeneratorApi.Security.Authentication.ApiKey;

public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    private const string AuthenticationHeaderScheme = "Bearer";

    private readonly IApiKeyValidator apiKeyValidator;

    public ApiKeyAuthenticationHandler(
        IApiKeyValidator apiKeyValidator,
        IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
        this.apiKeyValidator = apiKeyValidator;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string authorizationHeader = Request.Headers["Authorization"];

        if (string.IsNullOrEmpty(authorizationHeader))
        {
            return AuthenticateResult.NoResult();
        }

        if (!authorizationHeader.StartsWith(AuthenticationHeaderScheme + " ", StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.NoResult();
        }

        var key = authorizationHeader[(AuthenticationHeaderScheme.Length + 1)..];
        var isValidApiKey = await this.apiKeyValidator.IsValidAsync(key);

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
