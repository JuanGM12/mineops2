using MediatR;

namespace MineOps.Planning.Application.Activities.Queries;

public sealed record GetActivityByIdQuery(Guid Id) : IRequest<ActivityDto>;
