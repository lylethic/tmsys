using Dapper.Contrib.Extensions;

namespace server.Common.Domain.Entities;

public abstract class BaseEntity<T>
{
    [ExplicitKey]
    public required T id { get; set; }
}
