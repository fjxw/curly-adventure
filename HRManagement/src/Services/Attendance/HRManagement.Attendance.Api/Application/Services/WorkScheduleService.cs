using Microsoft.EntityFrameworkCore;
using HRManagement.Attendance.Api.Domain.Entities;
using HRManagement.Attendance.Api.Application.DTOs;
using HRManagement.Attendance.Api.Infrastructure.Data;
using HRManagement.Shared.Common.Models;

namespace HRManagement.Attendance.Api.Application.Services;

public interface IWorkScheduleService
{
    Task<ApiResponse<WorkScheduleDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<IEnumerable<WorkScheduleDto>>> GetByEmployeeAsync(Guid employeeId);
    Task<ApiResponse<WorkScheduleDto>> CreateAsync(CreateWorkScheduleDto dto);
    Task<ApiResponse<WorkScheduleDto>> UpdateAsync(Guid id, UpdateWorkScheduleDto dto);
    Task<ApiResponse<bool>> DeleteAsync(Guid id);
    Task<ApiResponse<bool>> CreateDefaultScheduleAsync(Guid employeeId);
}

public class WorkScheduleService : IWorkScheduleService
{
    private readonly AttendanceDbContext _context;

    public WorkScheduleService(AttendanceDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<WorkScheduleDto>> GetByIdAsync(Guid id)
    {
        var schedule = await _context.WorkSchedules.FindAsync(id);
        if (schedule == null)
            return ApiResponse<WorkScheduleDto>.FailureResponse("График работы не найден");

        return ApiResponse<WorkScheduleDto>.SuccessResponse(MapToDto(schedule));
    }

    public async Task<ApiResponse<IEnumerable<WorkScheduleDto>>> GetByEmployeeAsync(Guid employeeId)
    {
        var schedules = await _context.WorkSchedules
            .Where(s => s.EmployeeId == employeeId)
            .OrderBy(s => s.DayOfWeek)
            .ToListAsync();
        return ApiResponse<IEnumerable<WorkScheduleDto>>.SuccessResponse(schedules.Select(MapToDto));
    }

    public async Task<ApiResponse<WorkScheduleDto>> CreateAsync(CreateWorkScheduleDto dto)
    {
        var existing = await _context.WorkSchedules
            .FirstOrDefaultAsync(s => s.EmployeeId == dto.EmployeeId && s.DayOfWeek == dto.DayOfWeek);

        if (existing != null)
            return ApiResponse<WorkScheduleDto>.FailureResponse("График на этот день уже существует");

        var schedule = new WorkSchedule
        {
            Id = Guid.NewGuid(),
            EmployeeId = dto.EmployeeId,
            DayOfWeek = dto.DayOfWeek,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            BreakDuration = dto.BreakDuration,
            IsWorkingDay = dto.IsWorkingDay,
            CreatedAt = DateTime.UtcNow
        };

        _context.WorkSchedules.Add(schedule);
        await _context.SaveChangesAsync();

        return ApiResponse<WorkScheduleDto>.SuccessResponse(MapToDto(schedule), "График работы создан");
    }

    public async Task<ApiResponse<WorkScheduleDto>> UpdateAsync(Guid id, UpdateWorkScheduleDto dto)
    {
        var schedule = await _context.WorkSchedules.FindAsync(id);
        if (schedule == null)
            return ApiResponse<WorkScheduleDto>.FailureResponse("График работы не найден");

        if (dto.StartTime.HasValue)
            schedule.StartTime = dto.StartTime.Value;
        if (dto.EndTime.HasValue)
            schedule.EndTime = dto.EndTime.Value;
        if (dto.BreakDuration.HasValue)
            schedule.BreakDuration = dto.BreakDuration.Value;
        if (dto.IsWorkingDay.HasValue)
            schedule.IsWorkingDay = dto.IsWorkingDay.Value;

        schedule.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ApiResponse<WorkScheduleDto>.SuccessResponse(MapToDto(schedule), "График работы обновлен");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id)
    {
        var schedule = await _context.WorkSchedules.FindAsync(id);
        if (schedule == null)
            return ApiResponse<bool>.FailureResponse("График работы не найден");

        _context.WorkSchedules.Remove(schedule);
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResponse(true, "График работы удален");
    }

    public async Task<ApiResponse<bool>> CreateDefaultScheduleAsync(Guid employeeId)
    {
        var existing = await _context.WorkSchedules.AnyAsync(s => s.EmployeeId == employeeId);
        if (existing)
            return ApiResponse<bool>.FailureResponse("График для сотрудника уже существует");

        var workDays = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };
        var weekend = new[] { DayOfWeek.Saturday, DayOfWeek.Sunday };

        foreach (var day in workDays)
        {
            _context.WorkSchedules.Add(new WorkSchedule
            {
                Id = Guid.NewGuid(),
                EmployeeId = employeeId,
                DayOfWeek = day,
                StartTime = new TimeSpan(9, 0, 0),
                EndTime = new TimeSpan(18, 0, 0),
                BreakDuration = new TimeSpan(1, 0, 0),
                IsWorkingDay = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        foreach (var day in weekend)
        {
            _context.WorkSchedules.Add(new WorkSchedule
            {
                Id = Guid.NewGuid(),
                EmployeeId = employeeId,
                DayOfWeek = day,
                StartTime = TimeSpan.Zero,
                EndTime = TimeSpan.Zero,
                IsWorkingDay = false,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResponse(true, "Стандартный график работы создан (Пн-Пт 9:00-18:00)");
    }

    private static WorkScheduleDto MapToDto(WorkSchedule schedule) => new(
        schedule.Id,
        schedule.EmployeeId,
        schedule.DayOfWeek,
        schedule.StartTime,
        schedule.EndTime,
        schedule.BreakDuration,
        schedule.IsWorkingDay);
}
