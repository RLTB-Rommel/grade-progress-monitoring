using System.ComponentModel.DataAnnotations;

namespace GradeProgressMonitoring.Models
{
    public class LabRubricCriterion
    {
        public int LabRubricCriterionId { get; set; }

        public int GradingComponentId { get; set; }
        public GradingComponent? GradingComponent { get; set; }

        [Required, MaxLength(120)]
        public string Name { get; set; } = "";

        // Weights within Lab Activity must total 100
        public decimal WeightPercent { get; set; } = 0m;
    }
}