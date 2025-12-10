using HRManagement.Shared.Common.Models;

namespace HRManagement.Recruitment.Api.Domain.Entities;

public class Vacancy : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Requirements { get; set; } = string.Empty;
    
    public Guid DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    
    public Guid PositionId { get; set; }
    public string PositionName { get; set; } = string.Empty;
    
    public decimal SalaryFrom { get; set; }
    public decimal SalaryTo { get; set; }
    
    public VacancyStatus Status { get; set; } = VacancyStatus.Open;
    public DateTime? ClosedAt { get; set; }
    
    public ICollection<Candidate> Candidates { get; set; } = new List<Candidate>();
}

public enum VacancyStatus
{
    Draft,
    Open,
    OnHold,
    Closed,
    Filled
}
