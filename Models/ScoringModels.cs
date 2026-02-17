using System.ComponentModel.DataAnnotations;

namespace GradeProgressMonitoring.Models
{
    /// <summary>
    /// One submission per student per Lab Activity item (e.g., Week 01 Lab Activity).
    /// Stores late/plagiarism metadata used to compute final raw score.
    /// </summary>
    public class LabActivitySubmission
    {
        public int LabActivitySubmissionId { get; set; }

        // The specific dropdown item (e.g., "Week 05" under Laboratory Activity component)
        public int ComponentItemId { get; set; }
        public ComponentItem? ComponentItem { get; set; }

        // The student (Identity user id)
        [Required]
        public string StudentUserId { get; set; } = "";

        public int DaysLate { get; set; } = 0;

        public bool IsPlagiarized { get; set; } = false;

        public DateTime? EncodedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public ICollection<LabRubricScore> RubricScores { get; set; } = new List<LabRubricScore>();
    }

    /// <summary>
    /// One row per criterion per submission (Performance, Data Analysis, etc.)
    /// </summary>
    public class LabRubricScore
    {
        public int LabRubricScoreId { get; set; }

        public int LabActivitySubmissionId { get; set; }
        public LabActivitySubmission? Submission { get; set; }

        /// <summary>
        /// Criterion name stored as text for flexibility (matches LabRubricCriterion.Name).
        /// </summary>
        [Required, MaxLength(120)]
        public string CriterionName { get; set; } = "";

        /// <summary>
        /// Score for the criterion (raw points).
        /// Admin-defined rubric weights total 100.
        /// </summary>
        public decimal Score { get; set; } = 0m;
    }

    /// <summary>
    /// Helper computation methods (optional).
    /// These are not mapped by EF Core; keep as static utility if you want.
    /// </summary>
    public static class LabScoring
    {
        /// <summary>
        /// Computes final raw lab activity score (0..100) after plagiarism and late penalty.
        /// rawTotal is assumed 0..100.
        /// Penalty: 20% per day late.
        /// </summary>
        public static decimal ComputeFinal(decimal rawTotal, int daysLate, bool plagiarized)
        {
            if (plagiarized) return 0m;

            var factor = 1m - (0.20m * daysLate);
            if (factor < 0m) factor = 0m;

            return rawTotal * factor;
        }
    }
}