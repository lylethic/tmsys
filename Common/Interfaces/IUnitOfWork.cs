namespace server.Common.Interfaces;

public interface IUnitOfWork : IAsyncDisposable
{
    bool HasActiveTransaction { get; }

    /// <summary>Bắt đầu một transaction mới trên connection hiện tại.</summary>
    Task BeginAsync();

    /// <summary>Commit transaction đang active.</summary>
    Task CommitAsync();

    /// <summary>Rollback transaction đang active. An toàn khi gọi dù không có transaction.</summary>
    Task RollbackAsync();
}
