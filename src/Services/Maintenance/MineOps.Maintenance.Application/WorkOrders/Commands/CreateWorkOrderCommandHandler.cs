using MediatR;
using MineOps.Maintenance.Application.Abstractions;
using MineOps.Maintenance.Application.Exceptions;
using MineOps.Maintenance.Domain.WorkOrders;

namespace MineOps.Maintenance.Application.WorkOrders.Commands;

public sealed class CreateWorkOrderCommandHandler(
    IWorkOrderRepository orders, IEquipmentRepository equipment, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateWorkOrderCommand, WorkOrderDto>
{
    public async Task<WorkOrderDto> Handle(CreateWorkOrderCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Title))
            throw new RequestValidationException("Code and title are required.");
        if (await orders.CodeExistsAsync(request.Code.Trim(), cancellationToken))
            throw new ApplicationConflictException($"A work order with code '{request.Code.Trim()}' already exists.");
        var assignedEquipment = await equipment.GetByIdAsync(request.EquipmentId, cancellationToken)
            ?? throw new EntityNotFoundException($"Equipment '{request.EquipmentId}' was not found.");
        if (!assignedEquipment.IsActive)
            throw new ApplicationConflictException("The selected equipment is inactive.");

        var order = new WorkOrder(request.Code, request.EquipmentId, request.Title, request.Description,
            request.MaintenanceType, request.Priority, request.ScheduledDate);
        await unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            await orders.AddAsync(order, token);
            await unitOfWork.SaveChangesAsync(token);
        }, cancellationToken);
        return WorkOrderDto.FromDomain(order);
    }
}
