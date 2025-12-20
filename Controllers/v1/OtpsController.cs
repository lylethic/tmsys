using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using server.Application.Common.Interfaces;
using server.Application.DTOs;
using server.Application.Request;
using server.Common.Interfaces;
using server.Common.Settings;

namespace server.Controllers.v1;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/otp")]
public class OtpsController : BaseApiController
{
    private readonly IOtpService _otpService;

    public OtpsController(
        IServiceProvider serviceProvider,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        ILogManager loggerManager,
        IOtpService otpService
    ) : base(mapper, httpContextAccessor, loggerManager)
    {
        this._otpService = otpService;

    }

    /// <summary>
    /// Send code for reset password
    /// </summary>
    /// <param name="userEmail"></param>
    /// <returns></returns>
    [HttpPost("send-code")]
    public async Task<IActionResult> SendCode(string userEmail)
    {
        try
        {
            var result = await _otpService.SendResetCode(userEmail);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    /// <summary>
    /// Confirm reset password
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("confirm-reset-password")]
    public async Task<IActionResult> ConfirmResetPassword(ResetPasswordRequest request)
    {
        try
        {
            var result = await _otpService.ConfirmResetPassword(request);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    /// <summary>
    /// Send code for forgot password
    /// </summary>
    /// <param name="userEmail"></param>
    /// <returns></returns>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> SendCodeForForgotPassword(string userEmail)
    {
        try
        {
            var (isSuccess, message) = await _otpService.SendCodeForgotPassword(userEmail);
            if (isSuccess)
                return Success(message);
            return Error(message);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    /// <summary>
    /// Confirm code forgot password
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("confirm-forgot-password")]
    public async Task<IActionResult> ConfirmForgotPassword([FromBody] ConfirmCodeRequest request)
    {
        try
        {

            var (isSuccess, message) = await _otpService.ConfirmOTPCodeForgotPasswordAsync(request.Email, request.Code);

            if (isSuccess)
            {
                return Success(message);
            }
            return Error(message);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);

        }
    }
}
