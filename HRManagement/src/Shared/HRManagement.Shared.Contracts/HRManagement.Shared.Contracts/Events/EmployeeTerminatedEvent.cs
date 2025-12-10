namespace HRManagement.Shared.Contracts.Events;

public record EmployeeTerminatedEvent(
    Guid EmployeeId,
    DateTime TerminationDate,
    string Reason,
    DateTime CreatedAt);
