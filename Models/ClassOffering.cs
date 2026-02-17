using System.ComponentModel.DataAnnotations;

namespace GradeProgressMonitoring.Models
{
    public class ClassOffering
    {
        public int ClassOfferingId { get; set; }

        [Required]
        public string SubjectCode { get; set; } = "";

        [Required]
        public string SubjectTitle { get; set; } = "";

        public decimal Units { get; set; }

        [Required]
        public string CourseYearSection { get; set; } = "";

        public ClassType ClassType { get; set; }

        [Required]
        public string Term { get; set; } = "";

        [Required]
        public string SchoolYear { get; set; } = "";

        public string? Days { get; set; }
        public string? Time { get; set; }

        public ICollection<GradingComponent> Components { get; set; }
            = new List<GradingComponent>();
    }
}
