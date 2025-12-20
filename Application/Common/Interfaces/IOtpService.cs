using server.Application.Request;

namespace server.Application.Common.Interfaces;

public interface IOtpService
{
    public Task<string> SendResetCode(string userEmail);
    public Task<string> ConfirmResetPassword(ResetPasswordRequest request);
    Task<(bool, string)> SendCodeForgotPassword(string userEmail);
    Task<(bool, string)> ConfirmOTPCodeForgotPasswordAsync(string email, string code);
}
