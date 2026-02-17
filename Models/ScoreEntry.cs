using GradeProgressMonitoring.Data;

namespace GradeProgressMonitoring.Models
{
    public class ScoreEntry
    {
        public int ScoreEntryId { get; set; }

        public int ComponentItemId { get; set; }
        public ComponentItem? ComponentItem { get; set; }

        public string StudentUserId { get; set; } = "";
        public ApplicationUser? StudentUser { get; set; }

        public decimal? Score { get; set; }

        public string? EncodedByUserId { get; set; }
        public DateTime? EncodedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}