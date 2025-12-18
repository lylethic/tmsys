using System;
using System.Collections.Generic;
using Dapper.Contrib.Extensions;

namespace server.Domain.Entities;

[Table("v_notifications_grouped")]
public partial class VNotificationsGrouped
{
    public string? extend_user { get; set; }

    public string? extend_status { get; set; }

    public string? notifications { get; set; }
}
