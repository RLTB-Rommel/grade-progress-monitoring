using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GradeProgressMonitoring.Data;
using GradeProgressMonitoring.Models;
using Microsoft.EntityFrameworkCore;

namespace GradeProgressMonitoring.Services
{
    public class GradingTemplateService
    {
        private readonly ApplicationDbContext _db;

        public GradingTemplateService(ApplicationDbContext db)
        {
            _db = db;
        }

        public sealed record ComponentConfig(
            ComponentType ComponentType,
            bool IsEnabled,
            decimal WeightPercent
        );

        /// <summary>
        /// Creates/updates grading components for a class offering and auto-generates items.
        /// </summary>
        public async Task ApplyTemplateAsync(
            int classOfferingId,
            IReadOnlyList<ComponentConfig> configs,
            bool openAllItemsForEncoding = true)
        {
            var enabled = configs.Where(c => c.IsEnabled).ToList();
            var totalWeight = enabled.Sum(c => c.WeightPercent);

            if (enabled.Count == 0)
                throw new InvalidOperationException("At least one grading component must be enabled.");

            // Allow small decimal tolerance
            if (Math.Abs(totalWeight - 100m) > 0.01m)
                throw new InvalidOperationException(
                    $"Enabled component weights must total 100. Current total: {totalWeight}");

            var offering = await _db.ClassOfferings
                .Include(c => c.Components)
                    .ThenInclude(gc => gc.Items)
                .Include(c => c.Components)
                    .ThenInclude(gc => gc.LabRubricCriteria)
                .FirstOrDefaultAsync(c => c.ClassOfferingId == classOfferingId);

            if (offering is null)
                throw new InvalidOperationException("Class offering not found.");

            // Upsert components
            foreach (var cfg in configs)
            {
                var existing = offering.Components
                    .FirstOrDefault(x => x.ComponentType == cfg.ComponentType);

                if (existing is null)
                {
                    existing = new GradingComponent
                    {
                        ClassOfferingId = offering.ClassOfferingId,
                        ComponentType = cfg.ComponentType
                    };
                    offering.Components.Add(existing);
                }

                existing.IsEnabled = cfg.IsEnabled;
                existing.WeightPercent = cfg.IsEnabled ? cfg.WeightPercent : 0m;
            }

            // Generate items and rubric where applicable
            foreach (var comp in offering.Components.Where(c => c.IsEnabled))
            {
                EnsureItems(comp, offering.ClassType, openAllItemsForEncoding);
                EnsureLabRubricCriteria(comp, offering.ClassType);
            }

            await _db.SaveChangesAsync();
        }

        private static void EnsureItems(
            GradingComponent comp,
            ClassType classType,
            bool openAll)
        {
            void AddIfMissing(string label, int orderNo, decimal maxScore = 100m)
            {
                if (comp.Items.Any(i => i.Label == label)) return;

                comp.Items.Add(new ComponentItem
                {
                    Label = label,
                    OrderNo = orderNo,
                    MaxScore = maxScore,
                    IsOpenForEncoding = openAll
                });
            }

            switch (comp.ComponentType)
            {
                case ComponentType.Attendance:
                case ComponentType.Activity:
                case ComponentType.Recitation:
                    for (int w = 1; w <= 18; w++)
                        AddIfMissing($"Week {w:00}", w);
                    break;

                case ComponentType.Quizzes:
                    var quizLabels = new[]
                    {
                        "Prelim - Short Quiz",
                        "Prelim - Long Quiz",
                        "Prelim - Consideration Quiz",
                        "Midterm - Short Quiz",
                        "Midterm - Long Quiz",
                        "Midterm - Consideration Quiz",
                        "Finals - Short Quiz",
                        "Finals - Long Quiz",
                        "Finals - Consideration Quiz"
                    };
                    for (int i = 0; i < quizLabels.Length; i++)
                        AddIfMissing(quizLabels[i], i + 1);
                    break;

                case ComponentType.MajorOrPractical:
                    if (classType == ClassType.Lecture)
                    {
                        AddIfMissing("Prelim Exam", 1);
                        AddIfMissing("Midterm Exam", 2);
                        AddIfMissing("Final Exam", 3);
                    }
                    else
                    {
                        AddIfMissing("Practical Test 1", 1);
                        AddIfMissing("Practical Test 2", 2);
                        AddIfMissing("Practical Test 3", 3);
                    }
                    break;

                case ComponentType.ReportingOrProject:
                    AddIfMissing(
                        classType == ClassType.Lecture ? "Reporting" : "Final Project",
                        1);
                    break;
            }
        }

        private static void EnsureLabRubricCriteria(
            GradingComponent comp,
            ClassType classType)
        {
            if (classType != ClassType.Laboratory) return;
            if (comp.ComponentType != ComponentType.Activity) return;
            if (comp.LabRubricCriteria.Any()) return;

            comp.LabRubricCriteria.Add(new LabRubricCriterion { Name = "Performance", WeightPercent = 10m });
            comp.LabRubricCriteria.Add(new LabRubricCriterion { Name = "Results and Discussion", WeightPercent = 20m });
            comp.LabRubricCriteria.Add(new LabRubricCriterion { Name = "Data Analysis", WeightPercent = 20m });
            comp.LabRubricCriteria.Add(new LabRubricCriterion { Name = "Conclusion", WeightPercent = 20m });
            comp.LabRubricCriteria.Add(new LabRubricCriterion { Name = "Laboratory Assessment", WeightPercent = 30m });
        }

        /// <summary>
        /// UI helper: display names per class type.
        /// </summary>
        public static string GetComponentDisplayName(
            ComponentType componentType,
            ClassType classType)
        {
            return componentType switch
            {
                ComponentType.Attendance => "Attendance",
                ComponentType.Activity =>
                    classType == ClassType.Lecture ? "In-Class Activities" : "Laboratory Activity",
                ComponentType.Recitation => "Recitation",
                ComponentType.Quizzes => "Quizzes",
                ComponentType.MajorOrPractical =>
                    classType == ClassType.Lecture ? "Major Exams" : "Practical Tests",
                ComponentType.ReportingOrProject =>
                    classType == ClassType.Lecture ? "Reporting" : "Final Project",
                _ => componentType.ToString()
            };
        }
    }
}