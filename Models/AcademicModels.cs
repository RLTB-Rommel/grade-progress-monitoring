using System.ComponentModel.DataAnnotations;

namespace GradeProgressMonitoring.Models
{
    /// <summary>
    /// Student master record.
    /// Separate from Identity user to allow academic data
    /// even before account creation.
    /// </summary>
    public class StudentProfile
    {
        public int StudentProfileId { get; set; }

        [MaxLength(30)]
        public string? StudentNo { get; set; }

        [Required, MaxLength(150)]
        public string FullName { get; set; } = "";

        // Optional academic metadata
        [MaxLength(50)]
        public string? Program { get; set; }

        [MaxLength(20)]
        public string? YearLevel { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}