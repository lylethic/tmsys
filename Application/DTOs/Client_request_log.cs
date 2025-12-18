using System;
using Medo;
using server.Common.Domain.Entities;
using server.Common.Domain.Request.Create;
using server.Common.Domain.Request.Update;

namespace server.Application.DTOs;

public class Client_request_logDto
{

}

public class Client_request_logCreate : DomainCreate
{
    // User / Session info
    public Guid? User_id { get; set; }
    public Guid? Session_id { get; set; } = Uuid7.NewUuid7().ToGuid();
    public string? Client_ip { get; set; }
    /// <summary>
    /// trình duyệt, thiết bị
    /// </summary>
    public string? User_agent { get; set; }

    // Request info
    public string Url { get; set; } = string.Empty;
    public string? Feature_name { get; set; }
    public string Method { get; set; } = string.Empty;
    public string? Headers { get; set; }   // JSON, có thể dùng Dictionary nếu serialize
    public string? Body { get; set; }
    public int? Status_code { get; set; }
    public int? Response_time_ms { get; set; }
}

public class Client_request_logUpdate : DomainUpdate
{
    // User / Session info
    public Guid? User_id { get; set; }
    public Guid? Session_id { get; set; }
    public string? Client_ip { get; set; }
    public string? User_agent { get; set; }

    // Request info
    public string Url { get; set; } = string.Empty;
    public string? Feature_name { get; set; }
    public string Method { get; set; } = string.Empty;
    public string? Headers { get; set; }   // JSON, có thể dùng Dictionary nếu serialize
    public string? Body { get; set; }
    public int? Status_code { get; set; }
    public int? Response_time_ms { get; set; }
}


public class Client_request_logModel : DomainModel
{
    // User / Session info
    public Guid? User_id { get; set; }
    public Guid? Session_id { get; set; }
    public string? Client_ip { get; set; }
    public string? User_agent { get; set; }

    // Request info
    public string Url { get; set; } = string.Empty;
    public string? Feature_name { get; set; }
    public string Method { get; set; } = string.Empty;
    public string? Headers { get; set; }   // JSON, có thể dùng Dictionary nếu serialize
    public string? Body { get; set; }
    public int? Status_code { get; set; }
    public int? Response_time_ms { get; set; }
}
