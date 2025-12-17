using HRManagement.Shared.Common.Models;

namespace HRManagement.Payroll.Api.Domain.Entities;

public class LaborNorm : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid PositionId { get; set; } 
    public string PositionName { get; set; } = string.Empty;
    
    public decimal StandardHoursPerDay { get; set; } = 8;
    public decimal StandardHoursPerWeek { get; set; } = 40;
    public decimal StandardHoursPerMonth { get; set; } = 160;
    
    public decimal OvertimeMultiplier { get; set; } = 1.5m;
    public decimal NightShiftMultiplier { get; set; } = 1.2m;
    public decimal HolidayMultiplier { get; set; } = 2.0m;
}
