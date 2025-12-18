namespace server.Application.DTOs
{
    public class UpSertDepartment
    {
        public string Code { get; set; }

        public string Name { get; set; }

        public string? Description { get; set; }

        public Guid? Parent_id { get; set; } = null;
    }
}
