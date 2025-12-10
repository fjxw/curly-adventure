using HRManagement.Shared.Common.Models;

namespace HRManagement.Payroll.Api.Domain.Entities;

public class TimeSheet : BaseEntity
{
    public Guid EmployeeId { get; set; } // Reference to Employees service
    public string EmployeeName { get; set; } = string.Empty;
    
    public int Month { get; set; }
    public int Year { get; set; }
    
    public decimal WorkedHours { get; set; }
    public decimal OvertimeHours { get; set; }
    public decimal NightHours { get; set; }
    public decimal HolidayHours { get; set; }
    public decimal SickLeaveHours { get; set; }
    public decimal VacationHours { get; set; }
    
    public bool IsApproved { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? ApprovedById { get; set; }
}
