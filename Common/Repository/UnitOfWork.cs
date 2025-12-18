using System;
using server.Application.Common.Interfaces;
using server.Common.Interfaces;

namespace server.Common.Repository;

public class UnitOfWork : IUnitOfWork
{
    public UnitOfWork(IRoleRepository roleRepo)
    {
        Roles = roleRepo;
    }

    public IRoleRepository Roles { get; set; }
}
