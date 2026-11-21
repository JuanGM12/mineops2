using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MineOps.Planning.Application.Abstractions;
using MineOps.Planning.Infrastructure.Persistence;
using MineOps.Planning.Infrastructure.Persistence.Repositories;

namespace MineOps.Planning.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPlanningInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<PlanningDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IActivityRepository, EfActivityRepository>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        return services;
    }
}
