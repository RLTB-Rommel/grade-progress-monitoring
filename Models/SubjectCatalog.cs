using System.ComponentModel.DataAnnotations;

namespace GradeProgressMonitoring.Models
{
    public class SubjectCatalog
    {
        public int SubjectCatalogId { get; set; }

        [Required, MaxLength(30)]
        public string SubjectCode { get; set; } = "";

        [Required, MaxLength(200)]
        public string SubjectTitle { get; set; } = "";

        [Range(0, 30)]
        public decimal Units { get; set; } = 0m;

        // Optional future fields
        [MaxLength(80)]
        public string? Program { get; set; }

        [MaxLength(20)]
        public string? YearLevel { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
