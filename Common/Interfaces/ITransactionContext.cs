using System.Data;

namespace server.Common.Interfaces;

public interface ITransactionContext
{
    IDbTransaction? Current { get; set; }
    bool HasActiveTransaction { get; }
}
