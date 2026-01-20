using System;

namespace server.Application.DTOs;

public class PopupDto
{
    public string Content { get; set; } = null!;

    public DateOnly Validity_start { get; set; }

    public DateOnly Validity_end { get; set; }

    public short Type { get; set; }

    public DateTime? Display_from { get; set; }

    public DateTime? Display_to { get; set; }
}
