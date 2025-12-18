#nullable disable
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Domain.Entities
{
    [Table("media_assets")]
    public class MediaAsset
    {
        public Guid id { get; set; }
        public string public_id { get; set; }
        public string url { get; set; }
        public string secure_url { get; set; }
        public string resource_type { get; set; } = "image";
        public string format { get; set; }
        public long bytes { get; set; }
        public int? width { get; set; }
        public int? height { get; set; }

        // Foreign Keys
        public Guid? user_id { get; set; }
        public Guid? project_id { get; set; }
        public Guid? task_id { get; set; }

        // Metadata
        public string entity_type { get; set; }
        public Guid? entity_id { get; set; }
        public string description { get; set; }

        // Audit fields
        public DateTime created { get; set; }
        public DateTime? updated { get; set; }
        public Guid? created_by { get; set; }
        public Guid? updated_by { get; set; }
        public bool deleted { get; set; }
        public bool active { get; set; }

        public MediaAsset()
        {
        }
    }
}
