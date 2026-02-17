using System.Collections.Generic;

namespace GradeProgressMonitoring.Models
{
    public class GradingComponent
    {
        public int GradingComponentId { get; set; }

        public int ClassOfferingId { get; set; }
        public ClassOffering? ClassOffering { get; set; }

        public ComponentType ComponentType { get; set; }

        public bool IsEnabled { get; set; } = true;

        public decimal WeightPercent { get; set; } = 0;

        public ICollection<ComponentItem> Items { get; set; } = new List<ComponentItem>();
        public ICollection<LabRubricCriterion> LabRubricCriteria { get; set; } = new List<LabRubricCriterion>();
    }
}