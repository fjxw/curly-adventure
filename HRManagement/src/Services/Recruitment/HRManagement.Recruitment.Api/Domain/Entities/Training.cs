using HRManagement.Shared.Common.Models;

namespace HRManagement.Recruitment.Api.Domain.Entities;

public class Training : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TrainingType Type { get; set; }
    
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int DurationHours { get; set; }
    
    public string? Provider { get; set; }
    public string? Location { get; set; }
    public decimal Cost { get; set; }
    
    public int MaxParticipants { get; set; }
    public bool IsActive { get; set; } = true;
    
    public ICollection<TrainingParticipant> Participants { get; set; } = new List<TrainingParticipant>();
}

public enum TrainingType
{
    Onboarding,
    Technical,
    Soft_Skills,
    Leadership,
    Compliance,
    Certification,
    Workshop,
    Conference
}
