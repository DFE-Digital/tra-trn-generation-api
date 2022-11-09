using Microsoft.EntityFrameworkCore;
using TrnGeneratorApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseKestrel(options =>
{
    options.AddServerHeader = false;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<TrnGeneratorDbContext>();

if (builder.Environment.IsDevelopment())
{
    builder.Configuration
        .AddUserSecrets<Program>();
}

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1"));

app.MapPost("/api/v1/trn", (TrnGeneratorDbContext dbContext) =>
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
});

app.Run();
