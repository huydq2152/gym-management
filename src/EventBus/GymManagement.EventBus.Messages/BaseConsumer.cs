using MassTransit;
using Microsoft.Extensions.Logging;

namespace GymManagement.EventBus.Messages;

public abstract class BaseConsumer<T> : IConsumer<T> where T : class
{
    private readonly ILogger<BaseConsumer<T>> _logger;

    protected BaseConsumer(ILogger<BaseConsumer<T>> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<T> context)
    {
        _logger.LogInformation("Received message: {MessageType} with Id: {MessageId}", typeof(T).Name, context.MessageId);
        try
        {
            await ConsumeInternal(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error consuming message: {MessageType} with Id: {MessageId}", typeof(T).Name, context.MessageId);
            throw; 
        }
    }

    protected abstract Task ConsumeInternal(ConsumeContext<T> context);
}