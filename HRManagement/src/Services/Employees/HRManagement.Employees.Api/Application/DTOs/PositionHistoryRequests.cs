namespace HRManagement.Employees.Api.Application.DTOs;

public record PositionHistoryDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    Guid PositionId,
    string PositionName,
    Guid DepartmentId,
    string DepartmentName,
    DateTime StartDate,
    DateTime? EndDate,
    decimal Salary,
    string? ChangeReason,
    bool IsCurrent);

public record CreatePositionChangeRequest(
    Guid EmployeeId,
    Guid NewPositionId,
    Guid NewDepartmentId,
    decimal NewSalary,
    DateTime EffectiveDate,
    string? ChangeReason);
