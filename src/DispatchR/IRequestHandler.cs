namespace DispatchR;

public interface IRequestHandler<TRequest, TResponse>
{
    Task<TResponse> Handle(TRequest command, CancellationToken cancellationToken);
}