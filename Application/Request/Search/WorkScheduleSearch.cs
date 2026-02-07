using Microsoft.AspNetCore.Mvc;
using server.Application.Search;

namespace server.Application.Request.Search
{
    public class WorkScheduleSearch : BaseSearch
    {
        [FromQuery(Name = "internId")]
        public Guid? InternId { get; set; }

        [FromQuery(Name = "mentorId")]
        public string? InternEmail { get; set; }

        [FromQuery(Name = "mentorEmail")]
        public string? MentorEmail { get; set; }

        [FromQuery(Name = "weekStart")]
        public DateTimeOffset? WeekStart { get; set; }

        [FromQuery(Name = "weekEnd")]
        public DateTimeOffset? WeekEnd { get; set; }

        [FromQuery(Name = "monday")]
        public string? Monday { get; set; }

        [FromQuery(Name = "tuesday")]
        public string? Tuesday { get; set; }

        [FromQuery(Name = "wednesday")]
        public string? Wednesday { get; set; }

        [FromQuery(Name = "thursday")]
        public string? Thursday { get; set; }

        [FromQuery(Name = "friday")]
        public string? Friday { get; set; }

        [FromQuery(Name = "fullName")]
        public string? Full_name { get; set; }
    }
}
