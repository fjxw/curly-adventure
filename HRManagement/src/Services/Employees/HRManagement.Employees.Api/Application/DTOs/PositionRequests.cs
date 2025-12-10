namespace HRManagement.Employees.Api.Application.DTOs;

public record CreatePositionRequest(
    string Name,
    string? Description,
    decimal MinSalary,
    decimal MaxSalary,
    Guid DepartmentId);

public record UpdatePositionRequest(
    string Name,
    string? Description,
    decimal MinSalary,
    decimal MaxSalary,
    Guid DepartmentId);

public record CreateJobResponsibilityRequest(
    string Description,
    int Priority);
