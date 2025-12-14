namespace HRManagement.Shared.Contracts.Events;

public record AttendanceMarkedEvent(
    Guid AttendanceId,
    Guid EmployeeId,
    DateTime Date,
    string Status);

public record TimeSheetApprovedEvent(
    Guid TimeSheetId,
    Guid EmployeeId,
    int Year,
    int Month,
    decimal TotalWorkedHours,
    decimal TotalOvertimeHours);
