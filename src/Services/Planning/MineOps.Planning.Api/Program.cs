using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using MineOps.Planning.Api.Errors;
using MineOps.Planning.Api.Health;
using MineOps.Planning.Api.OpenApi;
using MineOps.Planning.Api.Security;
using MineOps.Planning.Application.Activities.Commands;
using MineOps.Planning.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("Planning")
    ?? throw new InvalidOperationException("Connection string 'Planning' is required.");

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
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
builder.Services.AddMediatR(configuration =>
    configuration.RegisterServicesFromAssemblyContaining<CreateActivityCommand>());
builder.Services.AddPlanningInfrastructure(connectionString);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddMineOpsOpenApi();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks()
    .AddCheck<PlanningDatabaseHealthCheck>("sqlserver");

var app = builder.Build();

app.UseExceptionHandler();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");
app.Run();

public partial class Program;
