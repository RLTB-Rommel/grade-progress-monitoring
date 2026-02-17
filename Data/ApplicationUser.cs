using Microsoft.AspNetCore.Identity;
using GradeProgressMonitoring.Models;

namespace GradeProgressMonitoring.Data;

public class ApplicationUser : IdentityUser
{
    public int? StudentProfileId { get; set; }
    public StudentProfile? StudentProfile { get; set; }
}