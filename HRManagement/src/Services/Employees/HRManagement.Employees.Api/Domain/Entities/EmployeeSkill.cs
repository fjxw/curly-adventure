using HRManagement.Shared.Common.Models;

namespace HRManagement.Employees.Api.Domain.Entities;

public class EmployeeSkill : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    
    public Guid SkillId { get; set; }
    public Skill Skill { get; set; } = null!;
    
    public SkillLevel Level { get; set; }
    public int? YearsOfExperience { get; set; }
    public DateTime? CertifiedDate { get; set; }
    public string? CertificateNumber { get; set; }
}

public class Skill : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    
    public ICollection<EmployeeSkill> EmployeeSkills { get; set; } = new List<EmployeeSkill>();
}

public enum SkillLevel
{
    Beginner,
    Elementary,
    Intermediate,
    Advanced,
    Expert
}
