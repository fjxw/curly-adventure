namespace HRManagement.Shared.Contracts.DTOs;

public record PositionDto(
    Guid Id,
    string Name,
    string Description,
    decimal MinSalary,
    decimal MaxSalary,
    Guid DepartmentId,
    string DepartmentName);
