using System;
using System.Data;
using Dapper;
using Medo;
using server.Application.Common.Interfaces;
using server.Application.Common.Respository;
using server.Common.Exceptions;
using server.Common.Utils;
using server.Domain.Entities;

namespace server.Repositories;

public class OTPRepository : SimpleCrudRepository<Otps, Guid>, IOtpRepository
{
    public OTPRepository(IDbConnection connection) : base(connection)
    {
        this._connection = connection;
    }

    public async Task<bool> AddAsync(Guid userId, string resetCode)
    {
        var idV7 = Uuid7.NewUuid7().ToGuid();
        var insertOtp = """
            INSERT INTO otp_codes (
                id,
                user_id,
                code, 
                created_at,
                expired_at,
                created,
                active
            )
            VALUES (@idV7, @userId, @resetCode, NOW(), NOW() + INTERVAL '5 minute', Now(), true)
        """;
        var inserted = await _connection.ExecuteAsync(insertOtp, new { idV7, userId, resetCode });
        if (inserted <= 0) throw new BadRequestException("Failed to generate reset code.");
        return true;
    }

    public async Task<Otps> GetOTPCodeAsync(string code, Guid userId)
    {
        var existingOtp = await _connection.QueryFirstOrDefaultAsync<Otps>(
            """
                SELECT * 
                FROM otp_codes
                WHERE user_id = @UserId 
                AND code = @Code 
                AND expired_at > NOW() 
                AND is_used = false
                AND active = true;
            """,
            new { UserId = userId, Code = code }
        );
        if (existingOtp != null) return existingOtp;
        throw new BadRequestException("OTP expired or invalid.");
    }

    public async Task UpdateOTPAsync(Guid otpId)
    {
        var updated = await _connection.ExecuteAsync("""
                UPDATE otp_codes SET 
                    is_used = true,
                    active = false,
                    updated = Now()
                WHERE id = @OtpId
            """, new { OtpId = otpId });
        if (updated <= 0) throw new BadRequestException("Failed to update OTP.");
    }
}
