using System;

namespace server.Common.Interfaces;

public interface IHasTotalCount
{
    long? Total_count { get; }
}
