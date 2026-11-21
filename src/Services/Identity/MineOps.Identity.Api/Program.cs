using Microsoft.AspNetCore.Mvc;
using MineOps.Identity.Api.Errors;
using MineOps.Identity.Api.Health;
using MineOps.Identity.Api.OpenApi;
using MineOps.Identity.Infrastructure;
using MineOps.Identity.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("Identity")
    ?? throw new InvalidOperationException("Connection string 'Identity' is required.");

builder.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState.Values
                .SelectMany(entry => entry.Errors)
                .Select(error => error.ErrorMessage)
                .Where(message => !string.IsNullOrWhiteSpace(message))
                .ToArray();
            return new BadRequestObjectResult(new ErrorResponse(false, "The request is invalid.", errors));
        };
    });
builder.Services.AddIdentityInfrastructure(builder.Configuration, connectionString);
builder.Services.AddMineOpsOpenApi();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks()
    .AddCheck<IdentityDatabaseHealthCheck>("sqlserver");

var app = builder.Build();

app.UseExceptionHandler();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapControllers();
app.MapHealthChecks("/health");

await using (var scope = app.Services.CreateAsyncScope())
{
    await scope.ServiceProvider.GetRequiredService<IdentitySeeder>().SeedAsync();
}

app.Run();

public partial class Program;
