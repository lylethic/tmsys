using System;
using server.Application.Request;
using server.Common.Models;

namespace server.Application.Common.Interfaces;

public interface IAuth
{
    public Task<AuthResponse> Login(AuthRequest model);
    Task<AuthResponse> RevokeRefreshToken(string? refreshToken);
    Task Logout();
    Task<bool> UpdateRemoveToken(string email);
}
