using Microsoft.AspNetCore.Identity;

namespace HRManagement.Employees.Api.Infrastructure.Data;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Guid? EmployeeId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
