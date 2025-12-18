using System;

namespace server.Common.Models;

public class RateLimitRuleModel
{
    public int Limit { get; set; }
    /// <summary>
    /// TimeSpan(int days, int hours, int minutes, int seconds, int milliseconds)
    /// </summary>
    public TimeSpan Window { get; set; }
}
