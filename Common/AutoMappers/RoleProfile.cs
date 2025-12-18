using AutoMapper;
using server.Application.DTOs;
using server.Domain.Entities;

namespace server.Common.AutoMappers
{
    public class RoleProfile : Profile
    {
        public RoleProfile()
        {
            CreateMap<RoleDto, Role>();
        }
    }
}
