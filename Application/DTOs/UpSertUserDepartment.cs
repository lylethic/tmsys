using System;

namespace server.Application.DTOs;

public class UpSertUserDepartment
{
    public Guid User_id { get; set; }

    public Guid Department_id { get; set; }

    public bool Is_primary { get; set; }

    public DateTime? Start_date { get; set; }

    public DateTime? End_date { get; set; }
}
