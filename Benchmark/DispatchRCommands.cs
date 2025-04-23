using DispatchR;

namespace Benchmark;

// Request
public class PingDispatchR : IRequest<PingDispatchR, string> { }

// Handler
public class PingHandlerDispatchR : IRequestHandler<PingDispatchR, string>
{
    public Task<string> Handle(PingDispatchR request, CancellationToken cancellationToken)
    {
        return Task.FromResult("Pong");
    }
}

public class LoggingBehaviorDispatchR : IRequestPipeline<PingDispatchR, string>
{
    public Task<string> Handle(PingDispatchR command, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
    {
        return next(cancellationToken);
    }
}