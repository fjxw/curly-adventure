using HRManagement.Shared.Common.Models;

namespace HRManagement.Employees.Api.Domain.Entities;

public class Employee : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? Address { get; set; }
    public string? PassportNumber { get; set; }
    public string? TaxId { get; set; }
    
    public Guid DepartmentId { get; set; }
    public Department Department { get; set; } = null!;
    
    public Guid PositionId { get; set; }
    public Position Position { get; set; } = null!;
    
    public DateTime HireDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    public bool IsActive { get; set; } = true;
    
    public ICollection<EmployeeDocument> Documents { get; set; } = new List<EmployeeDocument>();
}
