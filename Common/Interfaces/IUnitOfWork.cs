using System;
using server.Application.Common.Interfaces;

namespace server.Common.Interfaces;

public interface IUnitOfWork
{
    IRoleRepository Roles { get; }
}
