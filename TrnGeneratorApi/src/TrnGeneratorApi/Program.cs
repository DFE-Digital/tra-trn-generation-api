using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using TrnGeneratorApi.Models;
using TrnGeneratorApi.Requests;
using TrnGeneratorApi.Responses;
using TrnGeneratorApi.Security.Authentication.ApiKey;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseKestrel(options =>
{
    options.AddServerHeader = false;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Scheme = "Bearer",
        Type = SecuritySchemeType.Http
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        [
            new OpenApiSecurityScheme()
            {
                Reference = new OpenApiReference()
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            }
        ] = new List<string>()
    });
});
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

if (builder.Environment.IsProduction() &&
    Environment.GetEnvironmentVariable("WEBSITE_ROLE_INSTANCE_ID") == "0")
{
    await MigrateDatabase();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1"));

var trnRequestsGroup = app.MapGroup("/api/v1/trn-requests");
trnRequestsGroup.MapPost("/",
[Authorize]
async (TrnGeneratorDbContext dbContext) =>
{
    var nextTrn = await dbContext
                    .Set<IntReturn>()
                    .FromSqlRaw("SELECT \"fn_generate_trn\" as Value FROM fn_generate_trn()")                    
                    .FirstOrDefaultAsync();

    if (nextTrn != null && nextTrn.Value.HasValue)
    {
        return Results.Ok(nextTrn.Value);
    }
    else
    {
        return Results.NotFound();
    }
}
)
.WithTags("TRN Requests")
.Produces<int>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status401Unauthorized)
.Produces(StatusCodes.Status404NotFound);

var trnRangesGroup = app.MapGroup("/api/v1/trn-ranges");
trnRangesGroup.MapGet("/{fromTrn}",
[Authorize]
async (int fromTrn, TrnGeneratorDbContext dbContext) =>
    await dbContext.TrnRanges.FindAsync(fromTrn)
        is TrnRange trnRange
            ? Results.Ok(trnRange.Adapt<GetTrnRangeResponse>())
            : Results.NotFound()
)
.WithTags("TRN Ranges")
.Produces<TrnRange>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status401Unauthorized)
.Produces(StatusCodes.Status404NotFound);

trnRangesGroup.MapDelete("/{fromTrn}",
[Authorize]
async (int fromTrn, TrnGeneratorDbContext dbContext) =>
{
    if (await dbContext.TrnRanges.FindAsync(fromTrn) is TrnRange trnRange)
    {
        if (trnRange.IsExhausted || trnRange.NextTrn != trnRange.FromTrn)
        {
            return Results.BadRequest("TRN ranges which have been fully or partially used cannot be deleted.");
        }

        dbContext.TrnRanges.Remove(trnRange);
        await dbContext.SaveChangesAsync();
        return Results.Ok(trnRange.Adapt<DeleteTrnRangeResponse>());
    }

    return Results.NotFound();
}
)
.WithTags("TRN Ranges")
.Produces<TrnRange>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status401Unauthorized)
.Produces(StatusCodes.Status404NotFound);

trnRangesGroup.MapGet("/",
[Authorize]
async (TrnGeneratorDbContext dbContext) =>
{
    var trnRanges = await dbContext.TrnRanges.Select(r => r.Adapt<GetTrnRangeResponseBody>()).ToListAsync();
    var response = new GetAllTrnRangesResponse
    {
        TrnRanges = trnRanges
    };
    return Results.Ok(response);
}
)
.WithTags("TRN Ranges")
.Produces<List<TrnRange>>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status401Unauthorized);

trnRangesGroup.MapPost("/",
[Authorize]
async (CreateTrnRangeRequest createTrnRangeRequest, TrnGeneratorDbContext dbContext) =>
{
    if (createTrnRangeRequest.ToTrn < createTrnRangeRequest.FromTrn)
    {
        return Results.BadRequest("toTrn should be greater than or equal to fromTrn.");
    }

    if (await dbContext.TrnRanges.AnyAsync(r => Math.Max(0, Math.Min(createTrnRangeRequest.ToTrn, r.ToTrn) - Math.Max(createTrnRangeRequest.FromTrn, r.FromTrn) + 1) != 0))
    {
        return Results.BadRequest("New TRN range overlaps existing TRN range.");
    }

    var trnRange = createTrnRangeRequest.Adapt<TrnRange>();
    trnRange.NextTrn = trnRange.FromTrn;    
    dbContext.TrnRanges.Add(trnRange);
    await dbContext.SaveChangesAsync();
    var createTrnRangeResponse = trnRange.Adapt<CreateTrnRangeResponse>();
    return Results.Created($"/api/v1/trn-ranges/{createTrnRangeResponse.FromTrn}", createTrnRangeResponse);
})
.WithTags("TRN Ranges")
.Produces(StatusCodes.Status201Created)
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status401Unauthorized);

app.Run();

async Task MigrateDatabase()
{
    await using var scope = app.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<TrnGeneratorDbContext>();
    await dbContext.Database.MigrateAsync();
}

public partial class Program { }
