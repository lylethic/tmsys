using System;
using server.Application.Request;
using server.Common.Models;

namespace server.Application.Common.Interfaces;

public interface IAuth
{
    public Task<AuthResponse> Login(AuthRequest model);
    public Task<string> SendResetCode(string userEmail);
    public Task<string> ConfirmResetPassword(ResetPasswordRequest request);
    Task Logout();

}
