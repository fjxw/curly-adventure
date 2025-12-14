namespace HRManagement.Attendance.Api.Application.DTOs;

public record WorkScheduleDto(
    Guid Id,
    Guid EmployeeId,
    DayOfWeek DayOfWeek,
    TimeSpan StartTime,
    TimeSpan EndTime,
    TimeSpan? BreakDuration,
    bool IsWorkingDay);

public record CreateWorkScheduleDto(
    Guid EmployeeId,
    DayOfWeek DayOfWeek,
    TimeSpan StartTime,
    TimeSpan EndTime,
    TimeSpan? BreakDuration,
    bool IsWorkingDay);

public record UpdateWorkScheduleDto(
    TimeSpan? StartTime,
    TimeSpan? EndTime,
    TimeSpan? BreakDuration,
    bool? IsWorkingDay);
