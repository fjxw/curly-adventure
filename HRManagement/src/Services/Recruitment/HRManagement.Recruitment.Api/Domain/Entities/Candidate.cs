using HRManagement.Shared.Common.Models;

namespace HRManagement.Recruitment.Api.Domain.Entities;

public class Candidate : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    
    public string? ResumeUrl { get; set; }
    public string? ResumePath { get; set; }
    public string? PhotoPath { get; set; }
    public string? CoverLetter { get; set; }
    public int YearsOfExperience { get; set; }
    public string? Education { get; set; }
    public string? Skills { get; set; }
    
    public Guid VacancyId { get; set; }
    public Vacancy Vacancy { get; set; } = null!;
    
    public CandidateStatus Status { get; set; } = CandidateStatus.New;
    public string? Notes { get; set; }
    
    public ICollection<Interview> Interviews { get; set; } = new List<Interview>();
}

public enum CandidateStatus
{
    New,
    Screening,
    Interview,
    Testing,
    Offer,
    Hired,
    Rejected,
    Withdrawn
}
