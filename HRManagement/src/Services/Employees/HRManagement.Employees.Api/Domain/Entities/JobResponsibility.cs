using HRManagement.Shared.Common.Models;

namespace HRManagement.Employees.Api.Domain.Entities;

public class JobResponsibility : BaseEntity
{
    public string Description { get; set; } = string.Empty;
    public int Priority { get; set; }
    
    public Guid PositionId { get; set; }
    public Position Position { get; set; } = null!;
}
