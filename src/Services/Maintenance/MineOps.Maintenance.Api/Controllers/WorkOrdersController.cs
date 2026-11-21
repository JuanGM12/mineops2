using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MineOps.Maintenance.Api.Contracts;
using MineOps.Maintenance.Application.Common;
using MineOps.Maintenance.Application.WorkOrders;
using MineOps.Maintenance.Application.WorkOrders.Commands;
using MineOps.Maintenance.Application.WorkOrders.Queries;

namespace MineOps.Maintenance.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/work-orders")]
public sealed class WorkOrdersController(ISender sender) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Administrator,MaintenanceSupervisor")]
    public async Task<ActionResult<WorkOrderDto>> Create(CreateWorkOrderRequest request, CancellationToken token)
    {
        var order = await sender.Send(new CreateWorkOrderCommand(request.Code, request.EquipmentId, request.Title,
            request.Description, request.MaintenanceType, request.Priority, request.ScheduledDate), token);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpGet]
    public Task<PagedResult<WorkOrderDto>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken token = default) => sender.Send(new GetWorkOrdersQuery(page, pageSize), token);
    [HttpGet("{id:guid}")]
    public Task<WorkOrderDto> GetById(Guid id, CancellationToken token) => sender.Send(new GetWorkOrderByIdQuery(id), token);
    [HttpPatch("{id:guid}/start")]
    [Authorize(Roles = "Administrator,MaintenanceSupervisor")]
    public Task<WorkOrderDto> Start(Guid id, CancellationToken token) => sender.Send(new StartWorkOrderCommand(id), token);
    [HttpPatch("{id:guid}/complete")]
    [Authorize(Roles = "Administrator,MaintenanceSupervisor")]
    public Task<WorkOrderDto> Complete(Guid id, CancellationToken token) => sender.Send(new CompleteWorkOrderCommand(id), token);
    [HttpPatch("{id:guid}/cancel")]
    [Authorize(Roles = "Administrator,MaintenanceSupervisor")]
    public Task<WorkOrderDto> Cancel(Guid id, CancellationToken token) => sender.Send(new CancelWorkOrderCommand(id), token);
}
