namespace DispatchR;

public delegate Task<TResponse> RequestHandlerDelegate<TResponse>(CancellationToken t = default);

public interface IRequestPipeline<TRequest, TResponse>
{
    public virtual int Priority => 1;
    Task<TResponse> Handle(TRequest command, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken);
}