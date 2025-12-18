using System;
using AutoMapper;
using server.Common.Interfaces;

namespace server.Common.Utils;

public abstract class BaseService(IServiceProvider services)
{
    protected ILogManager _logger = services.GetRequiredService<ILogManager>();
    protected IServiceProvider _services = services;
    protected IMapper _mapper = services.GetRequiredService<IMapper>();
}