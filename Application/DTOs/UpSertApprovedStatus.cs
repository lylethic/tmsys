
namespace server.Application.DTOs;

public class UpSertApprovedStatus
{
    public required string name { get; set; }
    public required string code { get; set; }
    public string color { get; set; } = string.Empty;
    public short? sort_order { get; set; }
    public string bgcolor { get; set; } = string.Empty;
    public string type { get; set; } = string.Empty;
}
