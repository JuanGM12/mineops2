using MediatR;
using MineOps.Maintenance.Application.Abstractions;
using MineOps.Maintenance.Application.Exceptions;

namespace MineOps.Maintenance.Application.WorkOrders.Commands;

public sealed class StartWorkOrderCommandHandler(IWorkOrderRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<StartWorkOrderCommand, WorkOrderDto>
{
    public async Task<WorkOrderDto> Handle(StartWorkOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await Get(repository, request.Id, cancellationToken);
        order.Start(DateTimeOffset.UtcNow);
        await Save(unitOfWork, cancellationToken);
        return WorkOrderDto.FromDomain(order);
    }

    internal static async Task<Domain.WorkOrders.WorkOrder> Get(IWorkOrderRepository repository, Guid id, CancellationToken token) =>
        await repository.GetByIdAsync(id, token) ?? throw new EntityNotFoundException($"Work order '{id}' was not found.");

    internal static Task Save(IUnitOfWork unitOfWork, CancellationToken token) =>
        unitOfWork.ExecuteInTransactionAsync(t => unitOfWork.SaveChangesAsync(t), token);
}

public sealed class CompleteWorkOrderCommandHandler(IWorkOrderRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<CompleteWorkOrderCommand, WorkOrderDto>
{
    public async Task<WorkOrderDto> Handle(CompleteWorkOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await StartWorkOrderCommandHandler.Get(repository, request.Id, cancellationToken);
        order.Complete(DateTimeOffset.UtcNow);
        await StartWorkOrderCommandHandler.Save(unitOfWork, cancellationToken);
        return WorkOrderDto.FromDomain(order);
    }
}

public sealed class CancelWorkOrderCommandHandler(IWorkOrderRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<CancelWorkOrderCommand, WorkOrderDto>
{
    public async Task<WorkOrderDto> Handle(CancelWorkOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await StartWorkOrderCommandHandler.Get(repository, request.Id, cancellationToken);
        order.Cancel();
        await StartWorkOrderCommandHandler.Save(unitOfWork, cancellationToken);
        return WorkOrderDto.FromDomain(order);
    }
}
