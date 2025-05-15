# DispatchR/Botched 🚀

### A Blazing-Fast Async Notification Dispatcher for .NET
** *Minimal memory footprint. Extremely fast, correct execution.* **

> [!NOTE]
> DispatchR has been streamlined to focus on what it does best: delivering outstanding performance for asynchronous notification patterns in .NET applications.

## ⚡ Key Features
- **Purely Notification Focused**: Designed exclusively for high-speed, efficient asynchronous notification dispatch.
- **`ValueTask` Native**: All notification handlers are `ValueTask`-based by default for optimal async performance and reduced allocations.
- **Efficient Parallel Handler Execution (Default)**: All registered handlers for a notification are executed concurrently.
- **No `INotification` Constraint**: Notification types can be any class or struct; they do not need to implement a marker interface like `INotification`.
- **Dependency Injection Centric**: Built entirely on top of .NET's Dependency Injection for clean and manageable code.
- **Optimized Registration & Execution**: Streamlined and corrected registration process. Zero runtime reflection after initial setup ensures raw speed.
- **Minimal Allocations**: Engineered to minimize heap allocations, making it ideal for high-throughput and memory-sensitive applications.
- **Improved Type Safety**: Removed improper use of `Unsafe.As<>` for casting, ensuring more robust and type-safe operations.
- **Simplified API**: A clear and concise API, distinct from other mediator libraries, focusing purely on the notification pattern.

> :bulb: **Tip:** *If you need a library that excels at dispatching notifications with raw speed, minimal overhead, improved safety, and a simple API, DispatchR is built for you.*

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
    public ValueTask Handle(OrderCreatedNotification notification, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Emailing confirmation for order {notification.OrderId} placed at {notification.Timestamp}.");
        // Actual email sending logic here
        return default;
    }
}

// Handler to update inventory
public sealed class InventoryUpdateHandler : INotificationHandler<OrderCreatedNotification>
{
    public ValueTask Handle(OrderCreatedNotification notification, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Updating inventory for order {notification.OrderId}.");
        // Actual inventory update logic here
        return default;
    }
}
```
DispatchR will discover and execute these handlers in parallel by default.

## 3. Publish a Notification
Use the [`IPublisher`](src/DispatchR/IPublisher.cs:1) interface to publish notifications. DispatchR ensures that all relevant handlers are invoked.

```csharp
using System.Threading.Tasks;

public class OrderService
{
    private readonly IPublisher _mediator;

    public OrderService(IPublisher mediator)
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
3.  **Parallel Execution (Default)**: For notifications with multiple handlers, DispatchR executes them in parallel by default (internally using an optimized approach similar to `Task.WhenAll` for `ValueTask`s), maximizing throughput for I/O-bound operations.
4.  **Minimal Memory Overhead**: By avoiding complex object graphs and unnecessary allocations during the dispatch process, DispatchR keeps its memory footprint low.

When you call `PublishAsync<TNotification>(notification, cancellationToken)` on the [`IPublisher`](src/DispatchR/IPublisher.cs:1) interface:
```csharp
// Simplified conceptual logic within IPublisher.PublishAsync
// public ValueTask PublishAsync<TNotification>(TNotification notification, CancellationToken cancellationToken)
// {
//     // DispatchR resolves a single, effective INotificationHandler<TNotification>
//     // for the given TNotification. This handler is internally:
//     //  - A direct instance if only one handler is registered for TNotification.
//     //  - An internal AggregateHandler if multiple handlers are registered,
//     //    which then dispatches to all of them in parallel.
//     //  - A no-op handler if no handlers are registered for TNotification (or configured to do so).
//     var effectiveHandler = serviceProvider.GetRequiredService<INotificationHandler<TNotification>>();
//
//     return effectiveHandler.Handle(notification, cancellationToken); // Dispatches to the effective handler
// }
```
This direct approach, combined with compile-time type safety and efficient use of DI, results in blazing-fast notification handling.

# 🪴 How to use?
Register DispatchR and your notification handlers in your `Program.cs` or `Startup.cs`:

```csharp
// In your service configuration (e.g., Program.cs for .NET 6+)
builder.Services.AddDispatchR(typeof(Program).Assembly); // Or any assembly containing your handlers
```
This will scan the specified assembly for classes implementing `INotificationHandler<T>` for any type `T`. Your notification type `T` can be any class or struct and does not need to implement a specific marker interface.

### 💡 Key Notes:
1.  **Simplified Handler Registration**: `AddDispatchR` automatically discovers and registers all implementations of `INotificationHandler<T>`. Your notification types (`T`) do not need to implement any specific interface (e.g., `INotification`).
2.  **Corrected Handler Aggregation**: If multiple handlers are found for the same notification type, DispatchR correctly aggregates them into a single dispatcher that executes them in parallel by default.
3.  **Configurable Lifetime**: Handlers are registered with a `ServiceLifetime.Scoped` lifetime by default. You can provide a different `ServiceLifetime` as an optional argument to `AddDispatchR` if needed.

# ✨ How to install?
```bash
dotnet add package DispatchR.Botched --version X.Y.Z
```
*(Ensure you use the latest version of the `DispatchR.Botched` package.)*

# 🧪 Benchmark Result:
> [!IMPORTANT]
> All previous benchmarks from the original `hasanxdev/DispatchR` repository, or those comparing request-response patterns, have been removed as they are no longer applicable to this streamlined, notification-only version. Practices such as using `Parallel.For()` for list manipulations in tests, which could be unsafe, have also been discontinued.
>
> New benchmarks, focusing solely on the high-performance asynchronous notification dispatching capabilities of this version, are planned and will be published here. Expect to see results that clearly demonstrate DispatchR's speed and efficiency in its specialized domain.

# ✨ Contribute & Help Grow This Fork! ✨
This project is a streamlined fork, departing from the original `hasanxdev/DispatchR`. Contributions to *this* version are welcome! ❤️
 - Found a bug? 🐛 → Open an issue in the current repository.
 - Have an idea? 💡 → Suggest a feature in the current repository.
 - Want to code? 👩💻 → Submit a PR to the current repository.

Let's build something amazing together! 🚀
