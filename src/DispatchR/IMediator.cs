using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace DispatchR;

public interface IMediator
{
    Task<TResponse> Send<TRequest, TResponse>(IRequest<TRequest, TResponse> command,
        CancellationToken cancellationToken) where TRequest : IRequest;
}

public class Mediator(IServiceProvider serviceProvider) : IMediator
{
    public Task<TResponse> Send<TRequest, TResponse>(IRequest<TRequest, TResponse> command,
        CancellationToken cancellationToken) where TRequest : IRequest
    {
        var request = (TRequest)command;

        var pipelines = serviceProvider.GetServices<IRequestPipeline<TRequest, TResponse>>().ToList();

        var handler = serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();
        
        Task<TResponse> RequestHandler(CancellationToken t = default) => handler
            .Handle(request, cancellationToken);

        if (pipelines.Any())
        {
            var handlerWithPipeline = pipelines
                .OrderByDescending(p => p.Priority)
                .Aggregate((RequestHandlerDelegate<TResponse>)RequestHandler,
                (next, pipeline) => (ct) =>
                    pipeline.Handle(request, next, ct == CancellationToken.None ? cancellationToken : ct));
            
            return handlerWithPipeline(cancellationToken);
        }

        return handler.Handle(request, cancellationToken);
    }
}