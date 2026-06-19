using ICMarkets.Application.Abstractions;
using ICMarkets.Domain.Common.Exceptions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ICMarkets.Infrastructure.Persistence;

public class UnitOfWork(IcMarketsDbContext context) : IUnitOfWork
{
    private IDbContextTransaction? _transaction;
    private bool _committed;

    private const int
        SqliteUniqueConstraintErrorCode =
            19; // SQLITE_CONSTRAINT primary result code (covers UNIQUE / PRIMARY KEY violations).

    public async Task BeginAsync(CancellationToken cancellationToken = default)
    {
        _transaction ??= await context.Database.BeginTransactionAsync(cancellationToken);
        _committed = false;
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await context.SaveChangesAsync(cancellationToken);

            if (_transaction is not null)
            {
                await _transaction.CommitAsync(cancellationToken);
                _committed = true;
            }
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var entityName = ex.Entries.FirstOrDefault()?.Metadata.ClrType.Name ?? "UNKNOWN";
            throw new ConcurrentException(entityName);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqliteException
                                           {
                                               SqliteErrorCode: SqliteUniqueConstraintErrorCode
                                           })
        {
            var entityName = ex.Entries.FirstOrDefault()?.Metadata.ClrType.Name ?? "UNKNOWN";
            throw new ConcurrentException(entityName);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction is null)
        {
            return;
        }

        if (!_committed)
        {
            await _transaction.RollbackAsync();
        }

        await _transaction.DisposeAsync();
        _transaction = null;
    }
}