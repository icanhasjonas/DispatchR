using Microsoft.Extensions.DependencyInjection;

namespace DispatchR.Tests;

public class SimpleParallelHandlers
{
	private interface INestedInterface : INotificationHandler<int>;

	private class A : INestedInterface
	{
		public static int LastObservedValue { get; private set; }

		public ValueTask Handle(int notification, CancellationToken cancellationToken) {
			LastObservedValue = notification;
			return ValueTask.CompletedTask;
		}
	}

	private class B : INotificationHandler<int>
	{
		public static int LastObservedValue { get; private set; }

		public ValueTask Handle(int notification, CancellationToken cancellationToken) {
			LastObservedValue = notification;
			return ValueTask.CompletedTask;
		}
	}


	[Fact]
	public async Task ShouldExecuteBothHandlers() {
		var services = new ServiceCollection()
			.AddDispatchR(typeof(A).Assembly, ServiceLifetime.Singleton)
			.BuildServiceProvider();


		var mediator = services.GetRequiredService<IPublisher>();
		await mediator.PublishAsync(123);

		Assert.Equal(123, A.LastObservedValue);
		Assert.Equal(123, B.LastObservedValue);
	}
}
