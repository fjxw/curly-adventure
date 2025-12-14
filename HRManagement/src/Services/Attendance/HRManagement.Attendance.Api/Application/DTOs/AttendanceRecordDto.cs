namespace HRManagement.Attendance.Api.Application.DTOs;

public record AttendanceRecordDto(
    Guid Id,
    Guid EmployeeId,
    DateTime Date,
    TimeSpan? CheckInTime,
    TimeSpan? CheckOutTime,
    string Status,
    string? Notes,
    decimal? WorkedHours,
    decimal? OvertimeHours);

public record CreateAttendanceRecordDto(
    Guid EmployeeId,
    DateTime Date,
    TimeSpan? CheckInTime,
    TimeSpan? CheckOutTime,
    string Status,
    string? Notes);

public record UpdateAttendanceRecordDto(
    TimeSpan? CheckInTime,
    TimeSpan? CheckOutTime,
    string? Status,
    string? Notes);

public record CheckInDto(Guid EmployeeId, string? Notes);
public record CheckOutDto(Guid EmployeeId, string? Notes);
