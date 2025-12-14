using HRManagement.Shared.Common.Models;

namespace HRManagement.Employees.Api.Domain.Entities;

public class LeaveRequest : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    
    public LeaveType Type { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Reason { get; set; }
    
    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;
    public Guid? ApprovedById { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApproverComment { get; set; }
    
    public int TotalDays => (EndDate - StartDate).Days + 1;
}

public enum LeaveType
{
    Annual,
    Sick,
    Unpaid,
    Maternity,
    Paternity,
    Study,
    Bereavement
}

public enum LeaveStatus
{
    Pending,
    Approved,
    Rejected,
    Cancelled
}
