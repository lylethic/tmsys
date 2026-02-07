using Microsoft.AspNetCore.Mvc;
using server.Application.Search;

namespace server.Application.Request.Search
{
    public class TaskSearch : BaseSearch
    {
        [FromQuery(Name = "projectId")]
        public Guid? ProjectId { get; set; }

        [FromQuery(Name = "assignedTo")]
        public Guid? AssignedTo { get; set; }

        [FromQuery(Name = "status")]
        public string? Status { get; set; }

        [FromQuery(Name = "dueDate")]
        public DateTime? DueDate { get; set; }

        [FromQuery(Name = "allowLate")]
        public bool? AllowLate { get; set; }

        [FromQuery(Name = "allowResubmit")]
        public bool? AllowResubmit { get; set; }

        [FromQuery(Name = "passPoint")]
        public decimal? PassPoint { get; set; }

        [FromQuery(Name = "completedAt")]
        public DateTime? CompletedAt { get; set; }
    }
}
