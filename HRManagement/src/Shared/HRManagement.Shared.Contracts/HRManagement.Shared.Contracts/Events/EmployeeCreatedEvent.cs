namespace HRManagement.Shared.Contracts.Events;

public record EmployeeCreatedEvent(
    Guid EmployeeId,
    string FirstName,
    string LastName,
    string Email,
    Guid DepartmentId,
    Guid PositionId,
    DateTime HireDate,
    DateTime CreatedAt);
