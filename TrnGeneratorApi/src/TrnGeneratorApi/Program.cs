using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TrnGeneratorApi.Models;
using TrnGeneratorApi.Security.Authentication.ApiKey;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseKestrel(options =>
{
    options.AddServerHeader = false;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<TrnGeneratorDbContext>();
builder.Services.AddAuthentication(ApiKeyAuthenticationDefaults.AuthenticationScheme)
    .AddApiKey()
    .AddConfigurationApiKeyValidator();

builder.Services.AddAuthorization(o => o.AddPolicy("AuthenticatedUsersOnly",
                                  b => b.RequireAuthenticatedUser()));

if (builder.Environment.IsDevelopment())
{
    builder.Configuration
        .AddUserSecrets<Program>();
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();
}

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1"));

app.MapPost("/api/v1/trn",
[Authorize]
(TrnGeneratorDbContext dbContext) =>
{
    var nextTrn = dbContext
                    .Set<IntReturn>()
                    .FromSqlRaw("SELECT \"fn_generate_trn\" as Value FROM fn_generate_trn()")
                    .AsEnumerable()
                    .FirstOrDefault();

    if (nextTrn != null && nextTrn.Value.HasValue)
    {
        return Results.Ok(nextTrn.Value);
    }
    else
    {
        return Results.NotFound();
    }
})
.Produces<int>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status401Unauthorized)
.Produces(StatusCodes.Status404NotFound)
.Produces(StatusCodes.Status500InternalServerError);

app.Run();

public partial class Program { }
