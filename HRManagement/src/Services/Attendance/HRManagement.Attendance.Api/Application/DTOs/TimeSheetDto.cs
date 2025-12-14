namespace HRManagement.Attendance.Api.Application.DTOs;

public record TimeSheetDto(
    Guid Id,
    Guid EmployeeId,
    int Year,
    int Month,
    decimal TotalWorkedHours,
    decimal TotalOvertimeHours,
    int WorkingDays,
    int AbsentDays,
    int LateDays,
    int SickLeaveDays,
    int VacationDays,
    string Status,
    DateTime CreatedAt,
    DateTime? ApprovedAt,
    Guid? ApprovedById);

public record GenerateTimeSheetDto(Guid EmployeeId, int Year, int Month);
public record ApproveTimeSheetDto(Guid ApprovedById);
