using System;
using server.Domain.Entities;

namespace server.Application.Common.Interfaces;

public interface IOtpRepository
{
    Task<bool> AddAsync(Guid userId, string resetCode);
    Task<Otps> GetOTPCodeAsync(string code, Guid userId);
    Task UpdateOTPAsync(Guid otpId);
}
