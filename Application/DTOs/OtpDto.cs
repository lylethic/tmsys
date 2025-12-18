using System;
using server.Common.Domain.Request.Create;

namespace server.Application.DTOs;

public class OtpDto
{

}

public class OtpCreate : DomainCreate
{
    public string Code { get; set; }
    public Guid User_id { get; set; }
    public DateTime Created_at { get; set; }
    public DateTime Expire_at { get; set; }
}