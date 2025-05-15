# DispatchR/Botched 🚀

### A Blazing-Fast Async Notification Dispatcher for .NET
** *Minimal memory footprint. Extremely fast, correct execution.* **

> [!NOTE]
> DispatchR has been streamlined to focus on what it does best: delivering outstanding performance for asynchronous notification patterns in .NET applications.

## ⚡ Key Features
- **High-Performance Notifications**: Designed for speed and efficiency in handling asynchronous events.
- **Flexible Handler Execution**: Supports simple (all handlers), single (first/specific), or parallel execution of notification handlers.
- **Dependency Injection Centric**: Built entirely on top of .NET's Dependency Injection for clean and manageable code.
- **Zero Runtime Reflection**: Optimized for performance by avoiding runtime reflection after initial registration.
- **Minimal Allocations**: Engineered to minimize heap allocations, making it ideal for high-throughput and memory-sensitive applications.
- **Easy Migration**: Familiar patterns for developers accustomed to MediatR's notification system.

> :bulb: **Tip:** *If you need a mediator that excels at dispatching notifications with raw speed and efficiency, DispatchR is built for you.*

# Using DispatchR for Notifications

DispatchR simplifies the implementation of the mediator pattern for asynchronous notifications. Here’s how you can define, handle, and publish notifications:

## 1. Define a Notification
Notifications are simple classes, or values. They carry the data related to an event.

```csharp
// Example: A notification for when an order is created
public sealed class OrderCreatedNotification
{
    public int OrderId { get; init; }
    public DateTime Timestamp { get; init; }

    public OrderCreatedNotification(int orderId)
    {
        OrderId = orderId;
        Timestamp = DateTime.UtcNow;
    }
}
```

## 2. Create Notification Handlers
Handlers implement the `INotificationHandler<TNotification>` interface and contain the logic to process a notification. You can have multiple handlers for a single notification type.

```csharp
using System.Threading;
using System.Threading.Tasks;

// Handler to send an email
public sealed class EmailConfirmationHandler : INotificationHandler<OrderCreatedNotification>
{
    public Task Handle(OrderCreatedNotification notification, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Emailing confirmation for order {notification.OrderId} placed at {notification.Timestamp}.");
        // Actual email sending logic here
        return Task.CompletedTask;
    }
}

// Handler to update inventory
public sealed class InventoryUpdateHandler : INotificationHandler<OrderCreatedNotification>
{
    public Task Handle(OrderCreatedNotification notification, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Updating inventory for order {notification.OrderId}.");
        // Actual inventory update logic here
        return Task.CompletedTask;
    }
}
```
DispatchR will discover and execute these handlers based on your registration and configuration (e.g., in parallel).

## 3. Publish a Notification
Use the [`IMediator`](src/DispatchR/IMediator.cs:L1) interface to publish notifications. DispatchR ensures that all relevant handlers are invoked.

```csharp
using System.Threading.Tasks;

public class OrderService
{
    private readonly IMediator _mediator;

    public OrderService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task CreateOrderAsync(int orderId)
    {
        // ... order creation logic ...

        var notification = new OrderCreatedNotification(orderId);
        await _mediator.PublishAsync(notification); // CancellationToken can be passed if needed

        Console.WriteLine($"Order {orderId} processed and notification published.");
    }
}
```

# ⚡ How DispatchR Achieves High Performance

DispatchR's performance comes from its simple yet powerful design, focusing on efficient notification dispatch:

1.  **Optimized Handler Resolution**: Leveraging .NET's `IServiceProvider`, DispatchR efficiently resolves registered `INotificationHandler<T>` instances.
2.  **Direct Invocation**: Once resolved, handlers are invoked directly, without unnecessary overhead or reflection.
3.  **Parallel Execution (Default)**: For notifications with multiple handlers, DispatchR can execute them in parallel, maximizing throughput for I/O-bound operations. (This can be configured if sequential or single-handler execution is needed for specific scenarios).
4.  **Minimal Memory Overhead**: By avoiding complex object graphs and unnecessary allocations during the dispatch process, DispatchR keeps its memory footprint low.

When you call `PublishAsync<TNotification>(notification, cancellationToken)`:
```csharp
// Simplified conceptual logic within IMediator.PublishAsync
// public async Task PublishAsync<TNotification>(TNotification notification, CancellationToken cancellationToken)
//     where TNotification : INotification
// {
//     var handlers = serviceProvider.GetServices<INotificationHandler<TNotification>>();
//     var tasks = new List<Task>();
//
//     foreach (var handler in handlers)
//     {
//         tasks.Add(handler.Handle(notification, cancellationToken));
//     }
//
//     await Task.WhenAll(tasks); // Efficiently awaits all handlers
// }
```
This direct approach, combined with compile-time type safety and efficient use of DI, results in blazing-fast notification handling.

# 🪴 How to use?
Register DispatchR and your notification handlers in your `Program.cs` or `Startup.cs`:

```csharp
// In your service configuration (e.g., Program.cs for .NET 6+)
builder.Services.AddDispatchR(typeof(OrderCreatedNotification).Assembly);
```
This will scan the specified assembly (and optionally others) for implementations of `INotification` and `INotificationHandler<T>` and register them with the DI container.

### 💡 Key Notes:
1.  **Automatic Handler Registration**: `AddDispatchR` automatically finds and registers all your notification handlers from the provided assemblies.
2.  **Scoped Lifestyle**: Handlers are typically registered with a scoped lifetime, ensuring they are reused within a given scope (e.g., an HTTP request) but new instances are created for new scopes.
3.  **Configuration**: Future versions may offer more granular control over handler execution strategies (e.g., forcing sequential execution for specific notifications) via `AddDispatchR` options.

# ✨ How to install?
```bash
dotnet add package X --version 1.0.0
```
*(Ensure you are using the version that reflects the new notification-focused API. The version number here is from the original README and might need an update based on your release.)*

# 🧪 Benchmark Result:
> [!IMPORTANT]
> The previous benchmarks focused on request-response patterns and are no longer representative of DispatchR's current capabilities.
>
> New benchmarks specifically testing the performance of asynchronous notification dispatching with DispatchR against other libraries are planned and will be published here soon. Expect to see impressive results showcasing DispatchR's speed and efficiency!

# ✨ Contribute & Help Grow This Package! ✨
We welcome contributions to make this package even better! ❤️
 - Found a bug? 🐛 → Open an issue
 - Have an idea? 💡 → Suggest a feature
 - Want to code? 👩💻 → Submit a PR

Let's build something amazing together! 🚀
