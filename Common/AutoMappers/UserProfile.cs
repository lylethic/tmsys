using System;
using AutoMapper;
using server.Application.DTOs;
using server.Application.Models;
using server.Domain.Entities;

namespace server.Common.AutoMappers;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<UserDto, User>();
        CreateMap<User, UserModel>();
        CreateMap<UpdateUserDto, User>();
        CreateMap<UserUpdate, User>();
        CreateMap<CreateUserDto, User>().ReverseMap();
    }
}
