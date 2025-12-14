namespace HRManagement.Shared.Contracts.Events;

public record CandidateHiredEvent(
    Guid CandidateId,
    string FirstName,
    string LastName,
    string Email,
    Guid DepartmentId,
    Guid PositionId,
    decimal Salary,
    DateTime HireDate,
    DateTime CreatedAt);
