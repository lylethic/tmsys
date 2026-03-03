using System.Data;
using Npgsql;
using server.Common.Interfaces;

namespace server.Common.Repository;

/// <summary>
/// Scoped: quản lý vong đời của một database transaction trên IDbConnection duy nhất trong request.
/// Sử dụng cùng ITransactionContext để chia sẻ transaction với các repository.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly IDbConnection _connection;
    private readonly ITransactionContext _transactionContext;

    public UnitOfWork(IDbConnection connection, ITransactionContext transactionContext)
    {
        _connection = connection;
        _transactionContext = transactionContext;
    }

    public bool HasActiveTransaction => _transactionContext.HasActiveTransaction;

    public async Task BeginAsync()
    {
        if (_transactionContext.HasActiveTransaction)
            throw new InvalidOperationException("A transaction is already in progress.");

        if (_connection.State != ConnectionState.Open)
            _connection.Open();

        _transactionContext.Current = _connection is NpgsqlConnection npgsql
            ? await npgsql.BeginTransactionAsync()
            : _connection.BeginTransaction();
    }

    public Task CommitAsync()
    {
        if (!_transactionContext.HasActiveTransaction)
            throw new InvalidOperationException("No active transaction to commit.");

        _transactionContext.Current!.Commit();
        _transactionContext.Current = null;
        return Task.CompletedTask;
    }

    public Task RollbackAsync()
    {
        if (!_transactionContext.HasActiveTransaction)
            return Task.CompletedTask;

        _transactionContext.Current!.Rollback();
        _transactionContext.Current = null;
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        // Tự động rollback nếu quên commit (bảo vệ an toàn)
        if (_transactionContext.HasActiveTransaction)
            await RollbackAsync();
    }
}
