namespace HRManagement.Employees.Api.Application.DTOs;

public record CreateDepartmentRequest(
    string Name,
    string? Description,
    Guid? ParentDepartmentId);

public record UpdateDepartmentRequest(
    string Name,
    string? Description,
    Guid? ParentDepartmentId);
