using System.Data;
using server.Common.Interfaces;

namespace server.Common.Repository;

/// <summary>
/// Scoped: giữ IDbTransaction đang active trong phạm vi một HTTP request.
/// Được inject vào cả SimpleCrudRepository lẫn UnitOfWork.
/// </summary>
public class TransactionContext : ITransactionContext
{
    public IDbTransaction? Current { get; set; }
    public bool HasActiveTransaction => Current is not null;
}
