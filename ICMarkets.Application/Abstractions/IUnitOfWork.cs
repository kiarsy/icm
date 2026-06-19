namespace ICMarkets.Application.Abstractions;

public interface IUnitOfWork: IAsyncDisposable
{
    Task BeginAsync(CancellationToken cancellationToken = default);

    Task CommitAsync(CancellationToken cancellationToken = default);
}
