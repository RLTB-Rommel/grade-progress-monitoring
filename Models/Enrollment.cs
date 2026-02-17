using GradeProgressMonitoring.Data;
using System.ComponentModel.DataAnnotations;

namespace GradeProgressMonitoring.Models
{
    public class Enrollment
    {
        public int EnrollmentId { get; set; }

        [Required]
        public int ClassOfferingId { get; set; }
        public ClassOffering? ClassOffering { get; set; }

        // OPTIONAL links (can be null)
        public int? StudentProfileId { get; set; }
        public StudentProfile? StudentProfile { get; set; }

        public string? StudentUserId { get; set; }
        public ApplicationUser? StudentUser { get; set; }

        // Encode these manually
        [Required, MaxLength(30)]
        public string StudentNo { get; set; } = "";

        [Required, MaxLength(150)]
        public string StudentName { get; set; } = "";

        [MaxLength(50)]
        public string? Section { get; set; }
    }
}
