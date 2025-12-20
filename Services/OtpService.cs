using Dapper;
using server.Application.Common.Interfaces;
using server.Application.DTOs;
using server.Application.Request;
using server.Common.Exceptions;
using server.Common.Interfaces;
using server.Common.Utils;
using server.Domain.Entities;
using System.Data;
using System.Text.Json;

namespace server.Services;

public class OtpService : IOtpService
{
    private readonly IDbConnection _connection;
    private readonly IUserRepository _userRepo;
    private readonly IOtpRepository _otpRepo;
    private readonly IAuth _authService;
    private readonly IMailService _gmailService;
    private readonly ILogManager _loggerManager;

    public OtpService(
        IDbConnection dbConnection,
        IUserRepository userRepo,
        IOtpRepository otpRepo,
        IAuth authService,
        IMailService gmailService,
        ILogManager loggerManager)
    {
        _connection = dbConnection;
        _userRepo = userRepo;
        _otpRepo = otpRepo;
        _authService = authService;
        _gmailService = gmailService;
        _loggerManager = loggerManager;
    }

    #region Forgot pasword
    public async Task<(bool, string)> SendCodeForgotPassword(string userEmail)
    {
        try
        {
            var checkUserSql = "SELECT id, last_otp_sent_at FROM users WHERE email = @Email AND deleted = false";
            var user = await _connection.QueryFirstOrDefaultAsync<dynamic>(checkUserSql, new { Email = userEmail });

            if (user == null)
            {
                return (false, "User not found");
            }
            // Lock spam
            if (user.last_otp_sent_at != null)
            {
                DateTime lastSent = user.last_otp_sent_at;
                var timeDiff = DateTime.UtcNow - lastSent;

                if (timeDiff.TotalSeconds < 180)
                {
                    int secondsToWait = 180 - (int)timeDiff.TotalSeconds;
                    return (false, $"Please wait {secondsToWait} seconds before requesting a new code.");
                }
            }

            string codeToSend = "";

            var queryGetLatestOtp = """
                SELECT id, code 
                FROM public.otp_codes 
                WHERE user_id = @UserId AND is_used = false 
                ORDER BY created_at DESC 
                LIMIT 1
            """;

            var existingOtp = await _connection.QueryFirstOrDefaultAsync<dynamic>(
                queryGetLatestOtp,
                new { UserId = user.id }
            );

            if (existingOtp != null)
            {
                var updateSql = """
                    UPDATE public.otp_codes 
                    SET expired_at = @NewExpiry, updated = @Now 
                    WHERE id = @Id
                """;
                await _connection.ExecuteAsync(updateSql, new
                {
                    NewExpiry = DateTime.UtcNow.AddMinutes(5),
                    Now = DateTime.UtcNow,
                    Id = existingOtp.id
                });
                codeToSend = existingOtp.code;
            }
            else
            {
                codeToSend = ValidatorHepler.GenerateRandomNumberList(6);
                var isAdded = await _otpRepo.AddAsync(user.id, codeToSend);
                if (!isAdded) return (false, "System busy. Please try again.");
            }

            // Send Email
            string emailBody = await EmailTemplateManager.GetCodeToResetPasswordAsync(userEmail, codeToSend);
            var emailRequest = new SendEmailRequest(userEmail, "Your Password Reset Code", emailBody);
            await _gmailService.SendEmailAsync(emailRequest);
            var updateLastSentSql = "UPDATE users SET last_otp_sent_at = @Now WHERE id = @Id";
            await _connection.ExecuteAsync(updateLastSentSql, new { Now = DateTime.UtcNow, Id = user.id });

            return (true, "Code sent successfully. Please check your email.");
        }
        catch (Exception ex)
        {
            return (false, $"Failed to send reset code: {ex.Message}");
        }
    }

    public async Task<(bool, string)> ConfirmOTPCodeForgotPasswordAsync(string email, string code)
    {
        var user = await _userRepo.GetEmailAsync(email);

        if (user == null)
        {
            return (false, "User not found.");
        }
        var (isSuccess, message) = await VerifyOtpAsync(user.Id, code);

        if (!isSuccess)
        {
            return (isSuccess, message);
        }

        return (true, "Code verified successfully.");
    }
    #endregion

    #region Reset password
    public async Task<string> SendResetCode(string userEmail)
    {
        try
        {
            var user = await _userRepo.GetEmailAsync(userEmail);
            if (user == null)
                return "User not found";
            else
            {
                var resetCode = ValidatorHepler.GenerateRandomNumberList(6);

                var rows = await _otpRepo.AddAsync(user.Id, resetCode);
                if (!rows)
                    return "Failed to generate reset code. Please try again later!";

                string emailBody = await EmailTemplateManager.GetPasswordResetEmailAsync(userEmail, resetCode);
                var subject = "Your Password Reset Code for Loopy";
                var emailRequest = new SendEmailRequest(userEmail, subject, emailBody);
                await _gmailService.SendEmailAsync(emailRequest);

                return "Reset code sent successfully. Please check your email.";
            }
        }
        catch (Exception ex)
        {
            return $"Failed to send reset code: {ex.Message}";
        }
    }

    public async Task<string> ConfirmResetPassword(ResetPasswordRequest request)
    {
        var user = await _connection.QuerySingleOrDefaultAsync<User>(
            "SELECT id FROM users WHERE LOWER(email) = LOWER(@Email)",
            new { Email = request.Email }
        );

        if (user == null)
            return "User not found";

        var existingOtp = await _otpRepo.GetOTPCodeAsync(request.Code, user.Id);
        if (existingOtp != null)
        {
            var affected = await _userRepo.SetPassword(user.Id, request.NewPassword);

            // mark otp as used
            await _otpRepo.UpdateOTPAsync(existingOtp.Id);
            await _authService.UpdateRemoveToken(request.Email);

            if (affected)
                return "Password has been reset successfully.";
            return "Failed to update password.";
        }
        return "OTP expired or invalid.";
    }
    #endregion

    public async Task<(bool, string)> VerifyOtpAsync(Guid userId, string inputCode)
    {
        if (_connection.State != ConnectionState.Open)
            _connection.Open();

        using var transaction = _connection.BeginTransaction();
        try
        {
            string userSql = """
                SELECT id, failed_otp_attempts, lockout_end_at 
                FROM users 
                WHERE id = @Id AND active = true AND deleted = false
            """;

            var user = await _connection.QueryFirstOrDefaultAsync<UserLockoutStatus>(
                userSql, new { Id = userId }, transaction);

            if (user == null) return (false, "User not found.");

            // CHECK IF CURRENTLY LOCKED
            if (user.LockoutEndAt.HasValue && user.LockoutEndAt.Value > DateTime.UtcNow)
            {
                var timeLeft = user.LockoutEndAt.Value - DateTime.UtcNow;
                return (false, $"Account is temporarily locked. Please try again in {Math.Ceiling(timeLeft.TotalMinutes)} minutes.");
            }

            // CHECK OTP VALIDITY
            string otpCheckSql = """
                SELECT id FROM otp_codes 
                WHERE user_id = @UserId 
                    AND code = @Code 
                    AND is_used = false 
                    AND expired_at > @Now 
                    AND active = true 
                    AND deleted = false
            """;

            var otpId = await _connection.QueryFirstOrDefaultAsync<Guid?>(
                otpCheckSql,
                new { UserId = userId, Code = inputCode, Now = DateTime.UtcNow },
                transaction);

            // HANDLE INVALID OTP (FAILURE)
            if (otpId == null)
            {
                user.FailedOtpAttempts += 1;
                string message;
                DateTime? newLockoutTime = null;

                // Check if limit reached
                if (user.FailedOtpAttempts >= 5)
                {
                    newLockoutTime = DateTime.UtcNow.AddMinutes(15); // Lock for 15 minutes
                    message = "Maximum failed attempts reached. Account locked for 15 minutes.";
                }
                else
                {
                    int remaining = 5 - user.FailedOtpAttempts;
                    message = $"Invalid OTP. You have {remaining} attempts remaining.";
                    // Keep existing lockout time (usually null here)
                    newLockoutTime = user.LockoutEndAt;
                }

                string updateFailSql = """
                    UPDATE users 
                    SET failed_otp_attempts = @Count, 
                        lockout_end_at = @LockoutTime,
                        updated = @Now
                    WHERE id = @Id
                """;

                await _connection.ExecuteAsync(updateFailSql, new
                {
                    Count = user.FailedOtpAttempts,
                    LockoutTime = newLockoutTime,
                    Now = DateTime.UtcNow,
                    Id = userId
                }, transaction);

                transaction.Commit();
                return (false, message);
            }

            // HANDLE VALID OTP (SUCCESS)

            // Reset user counters
            string updateUserSuccessSql = """
                UPDATE users 
                SET failed_otp_attempts = 0, 
                    lockout_end_at = NULL,
                    last_login_time = @Now,
                    updated = @Now
                WHERE id = @UserId
            """;

            // Mark OTP as used
            string updateOtpSql = """
                UPDATE otp_codes 
                SET is_used = true, 
                    updated = @Now 
                WHERE id = @OtpId
            """;

            await _connection.ExecuteAsync(updateUserSuccessSql, new { UserId = userId, Now = DateTime.UtcNow }, transaction);
            await _connection.ExecuteAsync(updateOtpSql, new { OtpId = otpId, Now = DateTime.UtcNow }, transaction);

            transaction.Commit();
            return (true, "Authentication successful.");
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            throw new InternalErrorException($"Internal error: {ex.Message}");
        }
        finally
        {
            // _dbConnection.Close(); 
        }
    }
}