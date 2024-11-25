using ErrorOr;
using GymManagement.Application.Common.Interfaces;
using MediatR;

namespace GymManagement.Application.Subscriptions.Commands.DeleteSubscription;

public class DeleteSubscriptionCommandHandler : IRequestHandler<DeleteSubscriptionCommand, ErrorOr<Deleted>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISubscriptionsRepository _subscriptionsRepository;
    private readonly IAdminsRepository _adminsRepository;

    public DeleteSubscriptionCommandHandler(IUnitOfWork unitOfWork, ISubscriptionsRepository subscriptionsRepository, IAdminsRepository adminsRepository)
    {
        _unitOfWork = unitOfWork;
        _subscriptionsRepository = subscriptionsRepository;
        _adminsRepository = adminsRepository;
    }

    public async Task<ErrorOr<Deleted>> Handle(DeleteSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionsRepository.GetByIdAsync(request.SubscriptionId);
        if (subscription is null)
        {
            return Error.NotFound(description: "Subscription not found");
        }
        
        var admin = await _adminsRepository.GetByIdAsync(subscription.AdminId);
        if (admin is null)
        {
            return Error.NotFound(description: "Admin not found");
        }
        admin.DeleteSubscription(request.SubscriptionId);
        await _adminsRepository.UpdateAsync(admin);
        await _unitOfWork.CommitChangesAsync(cancellationToken);
        
        return Result.Deleted;
    }
}