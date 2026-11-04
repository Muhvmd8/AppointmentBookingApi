using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models.Security
{
    [Table("BlockedIps", Schema = "Security")]
    public class BlockedIp:BaseEntity<int>
    {
        public string IpAddress { get; set; } = string.Empty;
        public string Reason { get; set; } = "Auto Ban";
        public DateTimeOffset BlockedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? ExpiresAt { get; set; } // null = Permanent ban
    }
}
