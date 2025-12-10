namespace HRManagement.Payroll.Api.Application.DTOs;

public record CreateTimeSheetRequest(
    Guid EmployeeId,
    string EmployeeName,
    int Month,
    int Year,
    decimal WorkedHours,
    decimal OvertimeHours,
    decimal NightHours,
    decimal HolidayHours,
    decimal SickLeaveHours,
    decimal VacationHours);

public record UpdateTimeSheetRequest(
    decimal WorkedHours,
    decimal OvertimeHours,
    decimal NightHours,
    decimal HolidayHours,
    decimal SickLeaveHours,
    decimal VacationHours);

public record TimeSheetDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    int Month,
    int Year,
    decimal WorkedHours,
    decimal OvertimeHours,
    decimal NightHours,
    decimal HolidayHours,
    decimal SickLeaveHours,
    decimal VacationHours,
    bool IsApproved,
    DateTime? ApprovedAt);
