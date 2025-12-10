using HRManagement.Shared.Common.Models;

namespace HRManagement.Payroll.Api.Domain.Entities;

public class StaffingTable : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; } = true;
    
    public ICollection<StaffingPosition> Positions { get; set; } = new List<StaffingPosition>();
}
