using HRManagement.Shared.Common.Models;

namespace HRManagement.Employees.Api.Domain.Entities;

public class Position : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal MinSalary { get; set; }
    public decimal MaxSalary { get; set; }
    
    public Guid DepartmentId { get; set; }
    public Department Department { get; set; } = null!;
    
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    public ICollection<JobResponsibility> Responsibilities { get; set; } = new List<JobResponsibility>();
}
