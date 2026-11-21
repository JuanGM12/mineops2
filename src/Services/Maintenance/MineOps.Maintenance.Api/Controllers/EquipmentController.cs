using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MineOps.Maintenance.Infrastructure.Persistence;

namespace MineOps.Maintenance.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/equipment")]
public sealed class EquipmentController(MaintenanceDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IReadOnlyList<EquipmentResponse>> GetAll(CancellationToken cancellationToken) =>
        await dbContext.Equipment
            .AsNoTracking()
            .OrderBy(equipment => equipment.Code)
            .Select(equipment => new EquipmentResponse(
                equipment.Id,
                equipment.Code,
                equipment.Name,
                equipment.IsActive))
            .ToArrayAsync(cancellationToken);
}

public sealed record EquipmentResponse(Guid Id, string Code, string Name, bool IsActive);
