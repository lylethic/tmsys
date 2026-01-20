using System;

namespace server.Application.DTOs;

public class PopupDto
{
    public string Content { get; set; } = null!;

    public DateTime Validity_start { get; set; }

    public DateTime Validity_end { get; set; }

    public short Type { get; set; }

    public DateTime? Display_from { get; set; }

    public DateTime? Display_to { get; set; }
}
