namespace HRManagement.Shared.Contracts.DTOs;

public record EmployeeDto(
    Guid Id,
    string FirstName,
    string LastName,
    string MiddleName,
    string Email,
    string Phone,
    DateTime DateOfBirth,
    string Address,
    Guid DepartmentId,
    string DepartmentName,
    Guid PositionId,
    string PositionName,
    DateTime HireDate,
    DateTime? TerminationDate,
    bool IsActive);
