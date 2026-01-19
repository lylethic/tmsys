namespace server.Application.DTOs;

public class DepartmentTreeDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public Guid? Parent_id { get; set; }
    public bool? Active { get; set; }
    public bool? Deleted { get; set; }
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
    public Guid? Created_by { get; set; }
    public Guid? Updated_by { get; set; }

    public List<DepartmentTreeDto> Children { get; set; } = new();
}
