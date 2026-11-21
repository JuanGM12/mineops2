using MediatR;
using MineOps.Planning.Application.Abstractions;
using MineOps.Planning.Application.Exceptions;

namespace MineOps.Planning.Application.Activities.Commands;

public sealed class StartActivityCommandHandler(
    IActivityRepository repository,
    IUnitOfWork unitOfWork) : IRequestHandler<StartActivityCommand, ActivityDto>
{
    public async Task<ActivityDto> Handle(StartActivityCommand request, CancellationToken cancellationToken)
    {
        var activity = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new EntityNotFoundException($"Activity '{request.Id}' was not found.");

        activity.Start();
        await unitOfWork.ExecuteInTransactionAsync(
            token => unitOfWork.SaveChangesAsync(token),
            cancellationToken);
        return ActivityDto.FromDomain(activity);
    }
}
