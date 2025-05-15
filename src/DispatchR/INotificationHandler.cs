namespace DispatchR;

public interface INotificationHandler<in TNotification>
{
    ValueTask Handle(TNotification notification, CancellationToken cancellationToken);
}