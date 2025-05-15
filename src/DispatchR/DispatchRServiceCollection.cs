using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DispatchR;

public static class DispatchRServiceCollection
{
	private static bool IsClosedGenericOf(this Type type, Type openGeneric) =>
		type.IsGenericType &&
		type.GetGenericTypeDefinition() == openGeneric;

	private static bool IsNotificationHandler(this Type type) => type
		.IsClosedGenericOf(typeof(INotificationHandler<>));

	private static bool HasNotificationHandler(Type type) => type
		.GetInterfaces()
		.Any(x => x.IsNotificationHandler());

	private class AggregateHandler<TNotification>(List<INotificationHandler<TNotification>> handlers) : INotificationHandler<TNotification>
	{
		public ValueTask Handle(TNotification notification, CancellationToken cancellationToken) {
			Span<ValueTask> tasks = handlers
				.Select(x => x.Handle(notification, cancellationToken))
				.ToArray();
			return WhenAll(tasks);
		}

		private static ValueTask WhenAll(in ReadOnlySpan<ValueTask> tasks) {
			List<Task>? pendingTasks = null;
			foreach( var task in tasks ) {
				if( !task.IsCompletedSuccessfully ) {
					(pendingTasks ??= []).Add(task.AsTask());
				}
			}

			return pendingTasks is { Count: > 0 }
				? new ValueTask(Task.WhenAll(CollectionsMarshal.AsSpan(pendingTasks)))
				: ValueTask.CompletedTask;
		}
	}

	private abstract class Registration
	{
		public abstract void RegisterHandlers(IServiceCollection services, IEnumerable<Type> handlers, ServiceLifetime lifetime);
	}

	private class Registration<TNotification> : Registration
	{
		public override void RegisterHandlers(IServiceCollection services, IEnumerable<Type> handlers, ServiceLifetime lifetime) {
			switch( handlers.ToList() ) {
				case []: break;
				case [var handler]:
					services.Add(new ServiceDescriptor(
						typeof(INotificationHandler<TNotification>),
						s => s.GetRequiredService(handler),
						lifetime
					));
					break;
				case { } list:
					services.Add(new ServiceDescriptor(
						typeof(INotificationHandler<TNotification>),
						s => new AggregateHandler<TNotification>(list
							.Select(x => (INotificationHandler<TNotification>)s.GetRequiredService(x))
							.ToList()
						),
						lifetime
					));
					break;
			}
		}
	}

	public static IServiceCollection AddDispatchR(this IServiceCollection services, Assembly assembly, ServiceLifetime lifetime = ServiceLifetime.Scoped) {
		services.TryAddScoped<IPublisher, Publisher>();

		var handlers = assembly
			.GetTypes()
			.Where(type => (type.IsClass || type.IsValueType) && HasNotificationHandler(type))
			.ToList();

		foreach( var handler in handlers ) {
			services.TryAdd(new ServiceDescriptor(handler, handler, lifetime));
		}

		var handlersByInterface = handlers
			.SelectMany(x => x
				.GetInterfaces()
				.Where(IsNotificationHandler)
				.Distinct()
				.Select(i => (Handler: x, Interface: i))
			)
			.GroupBy(x => x.Interface, x => x.Handler);

		foreach( var r in handlersByInterface ) {
			var registration = (Registration)Activator.CreateInstance(typeof(Registration<>).MakeGenericType(r.Key.GetGenericArguments()))!;
			registration.RegisterHandlers(services, r, lifetime);
		}

		return services;
	}
}
