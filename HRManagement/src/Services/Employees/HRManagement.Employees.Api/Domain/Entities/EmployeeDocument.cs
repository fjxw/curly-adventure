using HRManagement.Shared.Common.Models;

namespace HRManagement.Employees.Api.Domain.Entities;

public class EmployeeDocument : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string? FilePath { get; set; }
    public DateTime? ExpirationDate { get; set; }
    
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
}
