using MediatR;

namespace MineOps.Planning.Application.Activities.Commands;

public sealed record CompleteActivityCommand(Guid Id) : IRequest<ActivityDto>;
