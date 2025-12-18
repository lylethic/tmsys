using System;

namespace server.Application.Request;

public class UserSearch : PaginationRequest
{
    public string? Email { get; set; }
    public string? Name { get; set; }
    public bool? IsActive { get; set; }
    public string? Role_name { get; set; }
}
