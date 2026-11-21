using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MineOps.Maintenance.Application.Abstractions;
using MineOps.Maintenance.Infrastructure.Persistence;
using MineOps.Maintenance.Infrastructure.Persistence.Repositories;

namespace MineOps.Maintenance.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddMaintenanceInfrastructure(this IServiceCollection services, string connection)
    {
        services.AddDbContext<MaintenanceDbContext>(o => o.UseSqlServer(connection));
        services.AddScoped<IEquipmentRepository, EfEquipmentRepository>();
        services.AddScoped<IWorkOrderRepository, EfWorkOrderRepository>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped<MaintenanceSeeder>();
        return services;
    }
}
