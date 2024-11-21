using ErrorOr;
using GymManagement.Application.Common.Interfaces;
using MediatR;

namespace GymManagement.Application.Subscriptions.Commands.DeleteSubscription;

public class DeleteSubscriptionCommandHandler : IRequestHandler<DeleteSubscriptionCommand, ErrorOr<Deleted>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISubscriptionsRepository _subscriptionsesRepository;

    public DeleteSubscriptionCommandHandler(IUnitOfWork unitOfWork, ISubscriptionsRepository subscriptionsesRepository)
    {
        _unitOfWork = unitOfWork;
        _subscriptionsesRepository = subscriptionsesRepository;
    }

    public async Task<ErrorOr<Deleted>> Handle(DeleteSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionsesRepository.GetByIdAsync(request.SubscriptionId);
        if (subscription is null)
        {
            return Error.NotFound(description: "Subscription not found");
        }

        await _subscriptionsesRepository.RemoveSubscriptionAsync(subscription);
        await _unitOfWork.CommitChangesAsync(cancellationToken);

        return Result.Deleted;
    }
}