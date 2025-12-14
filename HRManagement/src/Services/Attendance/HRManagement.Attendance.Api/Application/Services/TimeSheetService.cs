using Microsoft.EntityFrameworkCore;
using HRManagement.Attendance.Api.Domain.Entities;
using HRManagement.Attendance.Api.Application.DTOs;
using HRManagement.Attendance.Api.Infrastructure.Data;
using HRManagement.Shared.Common.Models;
using HRManagement.Shared.MessageBus;
using HRManagement.Shared.Contracts.Events;

namespace HRManagement.Attendance.Api.Application.Services;

public interface ITimeSheetService
{
    Task<ApiResponse<TimeSheetDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<TimeSheetDto>> GetByEmployeeMonthAsync(Guid employeeId, int year, int month);
    Task<ApiResponse<IEnumerable<TimeSheetDto>>> GetByEmployeeAsync(Guid employeeId);
    Task<ApiResponse<TimeSheetDto>> GenerateAsync(GenerateTimeSheetDto dto);
    Task<ApiResponse<TimeSheetDto>> SubmitAsync(Guid id);
    Task<ApiResponse<TimeSheetDto>> ApproveAsync(Guid id, ApproveTimeSheetDto dto);
    Task<ApiResponse<TimeSheetDto>> RejectAsync(Guid id);
}

public class TimeSheetService : ITimeSheetService
{
    private readonly AttendanceDbContext _context;
    private readonly IEventBus _eventBus;

    public TimeSheetService(AttendanceDbContext context, IEventBus eventBus)
    {
        _context = context;
        _eventBus = eventBus;
    }

    public async Task<ApiResponse<TimeSheetDto>> GetByIdAsync(Guid id)
    {
        var timesheet = await _context.TimeSheets.FindAsync(id);
        if (timesheet == null)
            return ApiResponse<TimeSheetDto>.FailureResponse("Табель не найден");

        return ApiResponse<TimeSheetDto>.SuccessResponse(MapToDto(timesheet));
    }

    public async Task<ApiResponse<TimeSheetDto>> GetByEmployeeMonthAsync(Guid employeeId, int year, int month)
    {
        var timesheet = await _context.TimeSheets
            .FirstOrDefaultAsync(t => t.EmployeeId == employeeId && t.Year == year && t.Month == month);

        if (timesheet == null)
            return ApiResponse<TimeSheetDto>.FailureResponse("Табель за указанный период не найден");

        return ApiResponse<TimeSheetDto>.SuccessResponse(MapToDto(timesheet));
    }

    public async Task<ApiResponse<IEnumerable<TimeSheetDto>>> GetByEmployeeAsync(Guid employeeId)
    {
        var timesheets = await _context.TimeSheets
            .Where(t => t.EmployeeId == employeeId)
            .OrderByDescending(t => t.Year)
            .ThenByDescending(t => t.Month)
            .ToListAsync();
        return ApiResponse<IEnumerable<TimeSheetDto>>.SuccessResponse(timesheets.Select(MapToDto));
    }

    public async Task<ApiResponse<TimeSheetDto>> GenerateAsync(GenerateTimeSheetDto dto)
    {
        var existing = await _context.TimeSheets
            .FirstOrDefaultAsync(t => t.EmployeeId == dto.EmployeeId && t.Year == dto.Year && t.Month == dto.Month);

        if (existing != null)
            return ApiResponse<TimeSheetDto>.FailureResponse("Табель за этот месяц уже существует");

        var startDate = new DateTime(dto.Year, dto.Month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var records = await _context.AttendanceRecords
            .Where(a => a.EmployeeId == dto.EmployeeId && a.Date >= startDate && a.Date <= endDate)
            .ToListAsync();

        var timesheet = new TimeSheet
        {
            Id = Guid.NewGuid(),
            EmployeeId = dto.EmployeeId,
            Year = dto.Year,
            Month = dto.Month,
            TotalWorkedHours = records.Sum(r => r.WorkedHours ?? 0),
            TotalOvertimeHours = records.Sum(r => r.OvertimeHours ?? 0),
            WorkingDays = records.Count(r => r.Status == AttendanceStatus.Present || r.Status == AttendanceStatus.Late || r.Status == AttendanceStatus.Remote),
            AbsentDays = records.Count(r => r.Status == AttendanceStatus.Absent),
            LateDays = records.Count(r => r.Status == AttendanceStatus.Late),
            SickLeaveDays = records.Count(r => r.Status == AttendanceStatus.SickLeave),
            VacationDays = records.Count(r => r.Status == AttendanceStatus.OnLeave),
            Status = TimeSheetStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        _context.TimeSheets.Add(timesheet);
        await _context.SaveChangesAsync();

        return ApiResponse<TimeSheetDto>.SuccessResponse(MapToDto(timesheet), "Табель сформирован");
    }

    public async Task<ApiResponse<TimeSheetDto>> SubmitAsync(Guid id)
    {
        var timesheet = await _context.TimeSheets.FindAsync(id);
        if (timesheet == null)
            return ApiResponse<TimeSheetDto>.FailureResponse("Табель не найден");

        if (timesheet.Status != TimeSheetStatus.Draft)
            return ApiResponse<TimeSheetDto>.FailureResponse("Можно подать только черновик");

        timesheet.Status = TimeSheetStatus.Submitted;
        await _context.SaveChangesAsync();

        return ApiResponse<TimeSheetDto>.SuccessResponse(MapToDto(timesheet), "Табель подан на утверждение");
    }

    public async Task<ApiResponse<TimeSheetDto>> ApproveAsync(Guid id, ApproveTimeSheetDto dto)
    {
        var timesheet = await _context.TimeSheets.FindAsync(id);
        if (timesheet == null)
            return ApiResponse<TimeSheetDto>.FailureResponse("Табель не найден");

        if (timesheet.Status != TimeSheetStatus.Submitted)
            return ApiResponse<TimeSheetDto>.FailureResponse("Можно утвердить только поданный табель");

        timesheet.Status = TimeSheetStatus.Approved;
        timesheet.ApprovedAt = DateTime.UtcNow;
        timesheet.ApprovedById = dto.ApprovedById;
        await _context.SaveChangesAsync();

        await _eventBus.PublishAsync(new TimeSheetApprovedEvent(
            timesheet.Id,
            timesheet.EmployeeId,
            timesheet.Year,
            timesheet.Month,
            timesheet.TotalWorkedHours,
            timesheet.TotalOvertimeHours));

        return ApiResponse<TimeSheetDto>.SuccessResponse(MapToDto(timesheet), "Табель утвержден");
    }

    public async Task<ApiResponse<TimeSheetDto>> RejectAsync(Guid id)
    {
        var timesheet = await _context.TimeSheets.FindAsync(id);
        if (timesheet == null)
            return ApiResponse<TimeSheetDto>.FailureResponse("Табель не найден");

        if (timesheet.Status != TimeSheetStatus.Submitted)
            return ApiResponse<TimeSheetDto>.FailureResponse("Можно отклонить только поданный табель");

        timesheet.Status = TimeSheetStatus.Rejected;
        await _context.SaveChangesAsync();

        return ApiResponse<TimeSheetDto>.SuccessResponse(MapToDto(timesheet), "Табель отклонен");
    }

    private static TimeSheetDto MapToDto(TimeSheet ts) => new(
        ts.Id,
        ts.EmployeeId,
        ts.Year,
        ts.Month,
        ts.TotalWorkedHours,
        ts.TotalOvertimeHours,
        ts.WorkingDays,
        ts.AbsentDays,
        ts.LateDays,
        ts.SickLeaveDays,
        ts.VacationDays,
        ts.Status.ToString(),
        ts.CreatedAt,
        ts.ApprovedAt,
        ts.ApprovedById);
}
