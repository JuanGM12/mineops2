using MediatR;

namespace MineOps.Planning.Application.Activities.Commands;

public sealed record CancelActivityCommand(Guid Id) : IRequest<ActivityDto>;
