#nullable disable

using System.ComponentModel.DataAnnotations.Schema;
using server.Common.Domain.Entities;

namespace server.Domain.Entities;

[Table("client_request_log")]
public class Client_request_log : SystemLogEntity<Guid>
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
