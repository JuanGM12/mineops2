using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using MineOps.Maintenance.Api.Errors;
using MineOps.Maintenance.Api.Health;
using MineOps.Maintenance.Api.OpenApi;
using MineOps.Maintenance.Api.Security;
using MineOps.Maintenance.Application.WorkOrders.Commands;
using MineOps.Maintenance.Infrastructure;
using MineOps.Maintenance.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
var connection = builder.Configuration.GetConnectionString("Maintenance")
    ?? throw new InvalidOperationException("Connection string 'Maintenance' is required.");
builder.Services.AddControllers().AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
    .ConfigureApiBehaviorOptions(o => o.InvalidModelStateResponseFactory = context =>
        new BadRequestObjectResult(new ErrorResponse(false, "The request is invalid.",
            context.ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray())));
builder.Services.AddMediatR(c => c.RegisterServicesFromAssemblyContaining<CreateWorkOrderCommand>());
builder.Services.AddMaintenanceInfrastructure(connection);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddMineOpsOpenApi();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>(); builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks().AddCheck<MaintenanceDatabaseHealthCheck>("sqlserver");
var app = builder.Build();
app.UseExceptionHandler();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers(); app.MapHealthChecks("/health");
await using (var scope = app.Services.CreateAsyncScope())
    await scope.ServiceProvider.GetRequiredService<MaintenanceSeeder>().SeedAsync();
app.Run();
public partial class Program;
