#pragma warning disable IDE0290

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using GradeProgressMonitoring.Models;

namespace GradeProgressMonitoring.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Academic Structure
        public DbSet<ClassOffering> ClassOfferings => Set<ClassOffering>();
        public DbSet<StudentProfile> StudentProfiles => Set<StudentProfile>();
        public DbSet<Enrollment> Enrollments => Set<Enrollment>();

        // Grading Configuration
        public DbSet<GradingComponent> GradingComponents => Set<GradingComponent>();
        public DbSet<ComponentItem> ComponentItems => Set<ComponentItem>();
        public DbSet<LabRubricCriterion> LabRubricCriteria => Set<LabRubricCriterion>();

        // Scores / Submissions
        public DbSet<ScoreEntry> ScoreEntries => Set<ScoreEntry>();
        public DbSet<LabActivitySubmission> LabActivitySubmissions => Set<LabActivitySubmission>();
        public DbSet<LabRubricScore> LabRubricScores => Set<LabRubricScore>();

        // Subject Catalog
        public DbSet<SubjectCatalog> SubjectCatalogs => Set<SubjectCatalog>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ✅ Prevent duplicates: include ClassType so Lecture + Lab can exist for same section
            builder.Entity<ClassOffering>()
                .HasIndex(c => new { c.SubjectCode, c.Term, c.SchoolYear, c.CourseYearSection, c.ClassType })
                .IsUnique();

            // ✅ Roster rule: one studentNo per class offering
            builder.Entity<Enrollment>()
                .HasIndex(e => new { e.ClassOfferingId, e.StudentNo })
                .IsUnique();

            // ✅ Enrollment -> StudentProfile (optional FK)
            builder.Entity<Enrollment>()
                .HasOne(e => e.StudentProfile)
                .WithMany(sp => sp.Enrollments)
                .HasForeignKey(e => e.StudentProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            // ✅ Enrollment -> ApplicationUser (optional FK)
            builder.Entity<Enrollment>()
                .HasOne(e => e.StudentUser)
                .WithMany()
                .HasForeignKey(e => e.StudentUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ✅ ApplicationUser -> StudentProfile (optional FK)
            builder.Entity<ApplicationUser>()
                .HasOne(u => u.StudentProfile)
                .WithMany()
                .HasForeignKey(u => u.StudentProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            // One score per student per component item
            builder.Entity<ScoreEntry>()
                .HasIndex(s => new { s.ComponentItemId, s.StudentUserId })
                .IsUnique();

            // One lab submission per student per lab item
            builder.Entity<LabActivitySubmission>()
                .HasIndex(x => new { x.ComponentItemId, x.StudentUserId })
                .IsUnique();

            // Decimal precision
            builder.Entity<ClassOffering>()
                .Property(c => c.Units)
                .HasPrecision(5, 2);

            builder.Entity<GradingComponent>()
                .Property(g => g.WeightPercent)
                .HasPrecision(5, 2);

            builder.Entity<LabRubricCriterion>()
                .Property(r => r.WeightPercent)
                .HasPrecision(5, 2);
        }
    }
}

#pragma warning restore IDE0290