using HRManagement.Shared.Common.Models;

namespace HRManagement.Recruitment.Api.Domain.Entities;

public class Interview : BaseEntity
{
    public Guid CandidateId { get; set; }
    public Candidate Candidate { get; set; } = null!;
    
    public DateTime ScheduledDate { get; set; }
    public string InterviewerName { get; set; } = string.Empty;
    public string? Notes { get; set; }
    
    public InterviewStatus Status { get; set; } = InterviewStatus.Scheduled;
}

public enum InterviewStatus
{
    Scheduled,
    InProgress,
    Completed,
    Cancelled
}
