namespace HRManagement.Employees.Api.Application.DTOs;

public record CreateEmployeeRequest(
    string FirstName,
    string LastName,
    string? MiddleName,
    string Email,
    string? Phone,
    DateTime DateOfBirth,
    string? Address,
    string? PassportNumber,
    string? TaxId,
    Guid DepartmentId,
    Guid PositionId,
    DateTime HireDate);

public record UpdateEmployeeRequest(
    string FirstName,
    string LastName,
    string? MiddleName,
    string Email,
    string? Phone,
    DateTime DateOfBirth,
    string? Address,
    Guid DepartmentId,
    Guid PositionId);

public record TerminateEmployeeRequest(
    DateTime TerminationDate,
    string Reason);
