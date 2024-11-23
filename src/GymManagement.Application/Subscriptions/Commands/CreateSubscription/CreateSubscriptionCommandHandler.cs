using ErrorOr;
using GymManagement.Application.Common.Interfaces;
using GymManagement.Domain.Subscriptions;
using MediatR;

namespace GymManagement.Application.Subscriptions.Commands.CreateSubscription;

public class CreateSubscriptionCommandHandler : IRequestHandler<CreateSubscriptionCommand, ErrorOr<Subscription>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISubscriptionsRepository _subscriptionsRepository;
    private readonly IAdminsRepository _adminsRepository;

    public CreateSubscriptionCommandHandler(IUnitOfWork unitOfWork, ISubscriptionsRepository subscriptionsRepository,
        IAdminsRepository adminsRepository)
    {
        _unitOfWork = unitOfWork;
        _subscriptionsRepository = subscriptionsRepository;
        _adminsRepository = adminsRepository;
    }

    public async Task<ErrorOr<Subscription>> Handle(CreateSubscriptionCommand request,
        CancellationToken cancellationToken)
    {
        var admin = await _adminsRepository.GetByIdAsync(request.AdminId);
        if(admin is null)
        {
            return Error.NotFound(description: "Admin not found");
        }
        
        if(admin.SubscriptionId is not null)
        {
            return Error.Conflict(description: "Admin already has an active subscription");
        }

        var subscription = new Subscription(
            subscriptionType: request.SubscriptionType,
            adminId: request.AdminId);
        await _subscriptionsRepository.AddSubscriptionAsync(subscription);
        
        admin.SetSubscription(subscription);
        await _adminsRepository.UpdateAsync(admin);

        await _unitOfWork.CommitChangesAsync(cancellationToken);

        return subscription;
    }
}