namespace TrnGeneratorApi.Security.Authentication.ApiKey;

public interface IApiKeyValidator
{
    Task<bool> IsValidAsync(string apiKey);
}
