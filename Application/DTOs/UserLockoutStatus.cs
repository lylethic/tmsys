namespace server.Application.DTOs;

public class UserLockoutStatus
{
    public Guid Id { get; set; }
    public int FailedOtpAttempts { get; set; }
    public DateTime? LockoutEndAt { get; set; }
}
