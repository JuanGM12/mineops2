using MediatR;
using MineOps.Planning.Application.Abstractions;
using MineOps.Planning.Application.Exceptions;

namespace MineOps.Planning.Application.Activities.Commands;

public sealed class CancelActivityCommandHandler(
    IActivityRepository repository,
    IUnitOfWork unitOfWork) : IRequestHandler<CancelActivityCommand, ActivityDto>
{
    public async Task<ActivityDto> Handle(CancelActivityCommand request, CancellationToken cancellationToken)
    {
        var activity = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new EntityNotFoundException($"Activity '{request.Id}' was not found.");

        activity.Cancel();
        await unitOfWork.ExecuteInTransactionAsync(
            token => unitOfWork.SaveChangesAsync(token),
            cancellationToken);
        return ActivityDto.FromDomain(activity);
    }
}
