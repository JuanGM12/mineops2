using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MineOps.Planning.Api.Contracts;
using MineOps.Planning.Application.Activities;
using MineOps.Planning.Application.Activities.Commands;
using MineOps.Planning.Application.Activities.Queries;
using MineOps.Planning.Application.Common;

namespace MineOps.Planning.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/activities")]
public sealed class ActivitiesController(ISender sender) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Administrator,Planner")]
    [ProducesResponseType<ActivityDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<ActivityDto>> Create(
        CreateActivityRequest request,
        CancellationToken cancellationToken)
    {
        var activity = await sender.Send(new CreateActivityCommand(
            request.Code,
            request.Title,
            request.Description,
            request.Area,
            request.Location,
            request.ResponsibleId,
            request.Priority,
            request.ScheduledDate,
            request.DueDate), cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = activity.Id }, activity);
    }

    [HttpGet]
    public Task<PagedResult<ActivityDto>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        sender.Send(new GetActivitiesQuery(page, pageSize), cancellationToken);

    [HttpGet("{id:guid}")]
    public Task<ActivityDto> GetById(Guid id, CancellationToken cancellationToken) =>
        sender.Send(new GetActivityByIdQuery(id), cancellationToken);

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Administrator,Planner")]
    public Task<ActivityDto> Update(
        Guid id,
        UpdateActivityRequest request,
        CancellationToken cancellationToken) =>
        sender.Send(new UpdateActivityCommand(
            id,
            request.Title,
            request.Description,
            request.Area,
            request.Location,
            request.ResponsibleId,
            request.Priority,
            request.ScheduledDate,
            request.DueDate), cancellationToken);

    [HttpPatch("{id:guid}/start")]
    [Authorize(Roles = "Administrator,Planner,Operator")]
    public Task<ActivityDto> Start(Guid id, CancellationToken cancellationToken) =>
        sender.Send(new StartActivityCommand(id), cancellationToken);

    [HttpPatch("{id:guid}/complete")]
    [Authorize(Roles = "Administrator,Planner,Operator")]
    public Task<ActivityDto> Complete(Guid id, CancellationToken cancellationToken) =>
        sender.Send(new CompleteActivityCommand(id), cancellationToken);

    [HttpPatch("{id:guid}/cancel")]
    [Authorize(Roles = "Administrator,Planner")]
    public Task<ActivityDto> Cancel(Guid id, CancellationToken cancellationToken) =>
        sender.Send(new CancelActivityCommand(id), cancellationToken);

    [HttpGet("pending")]
    public Task<PagedResult<ActivityDto>> GetPending(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        sender.Send(new GetPendingActivitiesQuery(page, pageSize), cancellationToken);

    [HttpGet("overdue")]
    public Task<PagedResult<ActivityDto>> GetOverdue(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        sender.Send(new GetOverdueActivitiesQuery(page, pageSize), cancellationToken);

    [HttpGet("responsible/{responsibleId:guid}")]
    public Task<PagedResult<ActivityDto>> GetByResponsible(
        Guid responsibleId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        sender.Send(
            new GetActivitiesByResponsibleQuery(responsibleId, page, pageSize),
            cancellationToken);
}
