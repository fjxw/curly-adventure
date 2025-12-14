using HRManagement.Shared.Common.Models;

namespace HRManagement.Employees.Api.Domain.Entities;

public class PositionHistory : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    
    public Guid PositionId { get; set; }
    public Position Position { get; set; } = null!;
    
    public Guid DepartmentId { get; set; }
    public Department Department { get; set; } = null!;
    
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal Salary { get; set; }
    public string? ChangeReason { get; set; }
    
    public bool IsCurrent => EndDate == null;
}
