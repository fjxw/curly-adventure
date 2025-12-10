namespace HRManagement.Shared.Contracts.Events;

public record SalaryCalculatedEvent(
    Guid EmployeeId,
    decimal BaseSalary,
    decimal Bonuses,
    decimal Deductions,
    decimal NetSalary,
    int Month,
    int Year,
    DateTime CreatedAt);
