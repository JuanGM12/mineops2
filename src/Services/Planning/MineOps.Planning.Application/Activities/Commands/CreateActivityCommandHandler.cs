using MediatR;
using MineOps.Planning.Application.Abstractions;
using MineOps.Planning.Application.Exceptions;
using MineOps.Planning.Domain.Activities;

namespace MineOps.Planning.Application.Activities.Commands;

public sealed class CreateActivityCommandHandler(
    IActivityRepository repository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateActivityCommand, ActivityDto>
{
    public async Task<ActivityDto> Handle(CreateActivityCommand request, CancellationToken cancellationToken)
    {
        ActivityRequestValidator.ValidateRequired(request.Code, request.Title);

        if (await repository.CodeExistsAsync(request.Code.Trim(), null, cancellationToken))
        {
            throw new ApplicationConflictException($"An activity with code '{request.Code.Trim()}' already exists.");
        }

        var activity = new Activity(
            request.Code,
            request.Title,
            request.Description,
            request.Area,
            request.Location,
            request.ResponsibleId,
            request.Priority,
            request.ScheduledDate,
            request.DueDate);

        await unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            await repository.AddAsync(activity, token);
            await unitOfWork.SaveChangesAsync(token);
        }, cancellationToken);

        return ActivityDto.FromDomain(activity);
    }
}
