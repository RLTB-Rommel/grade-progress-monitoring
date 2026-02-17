using System;
using System.ComponentModel.DataAnnotations;

namespace GradeProgressMonitoring.Models
{
    public class ComponentItem
    {
        public int ComponentItemId { get; set; }

        public int GradingComponentId { get; set; }
        public GradingComponent? GradingComponent { get; set; }

        [Required, MaxLength(150)]
        public string Label { get; set; } = "";

        public int OrderNo { get; set; } = 1;

        public decimal MaxScore { get; set; } = 100m;

        // For progressive encoding (admin/checker controls; scorer limited)
        public bool IsOpenForEncoding { get; set; } = true;

        // Checker workflow / finalization controls
        public bool IsLocked { get; set; } = false;          // locked prevents scorer edits
        public bool IsApproved { get; set; } = false;        // approved means finalized
        public DateTime? ApprovedAt { get; set; }            // when approved
        public string? ApprovedByUserId { get; set; }        // who approved (Identity user id)
    }
}
