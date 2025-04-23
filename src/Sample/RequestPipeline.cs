using DispatchR;

namespace Sample;

public class RequestPipeline : IRequestPipeline<MyCommand, int>
{
    public int Priority => 3;
    public Task<int> Handle(MyCommand command, RequestHandlerDelegate<int> next, CancellationToken cancellationToken)
    {
        Console.WriteLine("RP 1");
        return next(cancellationToken);
    }
}

public class RequestPipeline2 : IRequestPipeline<MyCommand, int>
{
    public int Priority => 1;

    public Task<int> Handle(MyCommand command, RequestHandlerDelegate<int> next, CancellationToken cancellationToken)
    {
        Console.WriteLine("RP 2");
        return next(cancellationToken);
    }
}