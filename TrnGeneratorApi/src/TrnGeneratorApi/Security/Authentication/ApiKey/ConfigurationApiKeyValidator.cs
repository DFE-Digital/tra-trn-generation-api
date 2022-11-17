namespace TrnGeneratorApi.Security.Authentication.ApiKey;

public class ConfigurationApiKeyValidator : IApiKeyValidator
{
    private const string ConfigurationSection = "ApiKeys";

    private HashSet<string> apiKeys = new HashSet<string>();

    public ConfigurationApiKeyValidator(IConfiguration configuration)
    {
        var section = configuration.GetSection(ConfigurationSection);
        section.Bind(apiKeys);
    }

    public Task<bool> IsValidAsync(string apiKey)
    {
        return Task.FromResult(apiKeys.Contains(apiKey));
    }
}
