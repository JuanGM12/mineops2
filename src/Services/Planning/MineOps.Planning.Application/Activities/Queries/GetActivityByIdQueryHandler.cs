using MediatR;
using MineOps.Planning.Application.Abstractions;
using MineOps.Planning.Application.Exceptions;

namespace MineOps.Planning.Application.Activities.Queries;

public sealed class GetActivityByIdQueryHandler(IActivityRepository repository)
    : IRequestHandler<GetActivityByIdQuery, ActivityDto>
{
    public async Task<ActivityDto> Handle(GetActivityByIdQuery request, CancellationToken cancellationToken)
    {
        var activity = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new EntityNotFoundException($"Activity '{request.Id}' was not found.");

        return ActivityDto.FromDomain(activity);
    }
}
