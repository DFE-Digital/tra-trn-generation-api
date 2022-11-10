namespace TrnGeneratorApi.Security.Authentication.ApiKey;

using Microsoft.AspNetCore.Authentication;

public static class ApiKeyExtensions
{
    public static AuthenticationBuilder AddApiKey(this AuthenticationBuilder builder)
    => builder.AddApiKey(ApiKeyAuthenticationDefaults.AuthenticationScheme);

    public static AuthenticationBuilder AddApiKey(this AuthenticationBuilder builder, string authenticationScheme)
        => builder.AddApiKey(authenticationScheme, _ => {});

    public static AuthenticationBuilder AddApiKey(this AuthenticationBuilder builder, Action<ApiKeyAuthenticationOptions>? configureOptions)
        => builder.AddApiKey(ApiKeyAuthenticationDefaults.AuthenticationScheme, configureOptions);

    public static AuthenticationBuilder AddApiKey(
        this AuthenticationBuilder builder,
        string authenticationScheme,
        Action<ApiKeyAuthenticationOptions>? configureOptions)
        => builder.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(authenticationScheme, configureOptions);

    public static AuthenticationBuilder AddConfigurationApiKeyValidator(
        this AuthenticationBuilder builder)
    {
        builder.Services.AddSingleton<IApiKeyValidator, ConfigurationApiKeyValidator>();
        return builder;
    }
}
