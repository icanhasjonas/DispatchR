using Microsoft.Extensions.DependencyInjection;

namespace DispatchR;

public interface IPublisher
{
	ValueTask PublishAsync<TNotification>(TNotification notification, CancellationToken cancellationToken = default);
}

public sealed class Publisher(IServiceProvider serviceProvider) : IPublisher
{
	public ValueTask PublishAsync<TNotification>(TNotification notification, CancellationToken cancellationToken) =>
		serviceProvider
			.GetRequiredService<INotificationHandler<TNotification>>()
			.Handle(notification, cancellationToken);
}
