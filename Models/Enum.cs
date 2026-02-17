namespace GradeProgressMonitoring.Models
{
    public enum ClassType
    {
        Lecture = 0,
        Laboratory = 1
    }

    // IMPORTANT: keep this name as ComponentType because models reference it
    public enum ComponentType
    {
        Attendance = 0,

        // Lecture: In-Class Activities | Lab: Laboratory Activity
        Activity = 1,

        Recitation = 2,

        Quizzes = 3,

        // Lecture: Major Exams | Lab: Practical Tests
        MajorOrPractical = 4,

        // Lecture: Reporting | Lab: Final Project
        ReportingOrProject = 5
    }

    public enum GradeItemKind
    {
        Standard = 0,
        LabActivity = 1
    }

    // Updated to match rule: 20% per day late, plagiarism = 0
    public enum SubmissionStatus
    {
        OnTime = 0,
        Late1Day = 1,
        Late2Days = 2,
        Late3Days = 3,
        Late4Days = 4,
        Late5Plus = 5,
        Plagiarized = 6
    }
}
