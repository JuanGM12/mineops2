using MediatR;
using MineOps.Planning.Application.Abstractions;
using MineOps.Planning.Application.Exceptions;
using MineOps.Planning.Domain.Activities;

namespace MineOps.Planning.Application.Activities.Commands;

public sealed class UpdateActivityCommandHandler(
    IActivityRepository repository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateActivityCommand, ActivityDto>
{
    public async Task<ActivityDto> Handle(UpdateActivityCommand request, CancellationToken cancellationToken)
    {
        ActivityRequestValidator.ValidateTitle(request.Title);
        var activity = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new EntityNotFoundException($"Activity '{request.Id}' was not found.");

        if (request.Priority is ActivityPriority.Critical)
        {
            activity.Reschedule(request.ScheduledDate, request.DueDate);
            UpdateDetails(activity, request);
        }
        else
        {
            UpdateDetails(activity, request);
            activity.Reschedule(request.ScheduledDate, request.DueDate);
        }

        if (request.ResponsibleId is Guid responsibleId)
        {
            activity.AssignResponsible(responsibleId);
        }

        await unitOfWork.ExecuteInTransactionAsync(
            token => unitOfWork.SaveChangesAsync(token),
            cancellationToken);

        return ActivityDto.FromDomain(activity);
    }

    private static void UpdateDetails(
        Activity activity,
        UpdateActivityCommand request) => activity.UpdateDetails(
            request.Title,
            request.Description,
            request.Area,
            request.Location,
            request.Priority);
}
