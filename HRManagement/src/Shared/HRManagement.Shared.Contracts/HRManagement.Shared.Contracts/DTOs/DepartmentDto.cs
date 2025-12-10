namespace HRManagement.Shared.Contracts.DTOs;

public record DepartmentDto(
    Guid Id,
    string Name,
    string Description,
    Guid? ParentDepartmentId,
    string? ParentDepartmentName,
    int EmployeeCount);
