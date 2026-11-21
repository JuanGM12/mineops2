using MediatR;

namespace MineOps.Planning.Application.Activities.Commands;

public sealed record StartActivityCommand(Guid Id) : IRequest<ActivityDto>;
