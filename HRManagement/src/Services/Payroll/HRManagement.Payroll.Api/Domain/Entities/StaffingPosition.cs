using HRManagement.Shared.Common.Models;

namespace HRManagement.Payroll.Api.Domain.Entities;

public class StaffingPosition : BaseEntity
{
    public Guid StaffingTableId { get; set; }
    public StaffingTable StaffingTable { get; set; } = null!;
    
    public Guid DepartmentId { get; set; } // Reference to Employees service
    public Guid PositionId { get; set; } // Reference to Employees service
    
    public string PositionName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    
    public int HeadCount { get; set; } // Количество штатных единиц
    public int OccupiedCount { get; set; } // Занято
    public decimal Salary { get; set; }
}
