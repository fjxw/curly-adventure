using Microsoft.EntityFrameworkCore;
using HRManagement.Attendance.Api.Domain.Entities;
using HRManagement.Attendance.Api.Application.DTOs;
using HRManagement.Attendance.Api.Infrastructure.Data;
using HRManagement.Shared.Common.Models;
using HRManagement.Shared.MessageBus;
using HRManagement.Shared.Contracts.Events;

namespace HRManagement.Attendance.Api.Application.Services;

public interface IAttendanceService
{
    Task<ApiResponse<AttendanceRecordDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<IEnumerable<AttendanceRecordDto>>> GetByEmployeeAsync(Guid employeeId, DateTime? startDate, DateTime? endDate);
    Task<ApiResponse<IEnumerable<AttendanceRecordDto>>> GetByDateAsync(DateTime date);
    Task<ApiResponse<AttendanceRecordDto>> CreateAsync(CreateAttendanceRecordDto dto);
    Task<ApiResponse<AttendanceRecordDto>> UpdateAsync(Guid id, UpdateAttendanceRecordDto dto);
    Task<ApiResponse<bool>> DeleteAsync(Guid id);
    Task<ApiResponse<AttendanceRecordDto>> CheckInAsync(CheckInDto dto);
    Task<ApiResponse<AttendanceRecordDto>> CheckOutAsync(CheckOutDto dto);
}

public class AttendanceService : IAttendanceService
{
    private readonly AttendanceDbContext _context;
    private readonly IEventBus _eventBus;

    public AttendanceService(AttendanceDbContext context, IEventBus eventBus)
    {
        _context = context;
        _eventBus = eventBus;
    }

    public async Task<ApiResponse<AttendanceRecordDto>> GetByIdAsync(Guid id)
    {
        var record = await _context.AttendanceRecords.FindAsync(id);
        if (record == null)
            return ApiResponse<AttendanceRecordDto>.FailureResponse("Запись посещаемости не найдена");

        return ApiResponse<AttendanceRecordDto>.SuccessResponse(MapToDto(record));
    }

    public async Task<ApiResponse<IEnumerable<AttendanceRecordDto>>> GetByEmployeeAsync(Guid employeeId, DateTime? startDate, DateTime? endDate)
    {
        var query = _context.AttendanceRecords.Where(a => a.EmployeeId == employeeId);

        if (startDate.HasValue)
            query = query.Where(a => a.Date >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(a => a.Date <= endDate.Value);

        var records = await query.OrderByDescending(a => a.Date).ToListAsync();
        return ApiResponse<IEnumerable<AttendanceRecordDto>>.SuccessResponse(records.Select(MapToDto));
    }

    public async Task<ApiResponse<IEnumerable<AttendanceRecordDto>>> GetByDateAsync(DateTime date)
    {
        var records = await _context.AttendanceRecords
            .Where(a => a.Date.Date == date.Date)
            .OrderBy(a => a.CheckInTime)
            .ToListAsync();
        return ApiResponse<IEnumerable<AttendanceRecordDto>>.SuccessResponse(records.Select(MapToDto));
    }

    public async Task<ApiResponse<AttendanceRecordDto>> CreateAsync(CreateAttendanceRecordDto dto)
    {
        var existing = await _context.AttendanceRecords
            .FirstOrDefaultAsync(a => a.EmployeeId == dto.EmployeeId && a.Date.Date == dto.Date.Date);

        if (existing != null)
            return ApiResponse<AttendanceRecordDto>.FailureResponse("Запись посещаемости на эту дату уже существует");

        if (!Enum.TryParse<AttendanceStatus>(dto.Status, out var status))
            return ApiResponse<AttendanceRecordDto>.FailureResponse("Неверный статус посещаемости");

        var record = new AttendanceRecord
        {
            Id = Guid.NewGuid(),
            EmployeeId = dto.EmployeeId,
            Date = dto.Date.Date,
            CheckInTime = dto.CheckInTime,
            CheckOutTime = dto.CheckOutTime,
            Status = status,
            Notes = dto.Notes,
            WorkedHours = CalculateWorkedHours(dto.CheckInTime, dto.CheckOutTime),
            CreatedAt = DateTime.UtcNow
        };

        _context.AttendanceRecords.Add(record);
        await _context.SaveChangesAsync();

        await _eventBus.PublishAsync(new AttendanceMarkedEvent(
            record.Id,
            record.EmployeeId,
            record.Date,
            record.Status.ToString()));

        return ApiResponse<AttendanceRecordDto>.SuccessResponse(MapToDto(record), "Запись посещаемости создана");
    }

    public async Task<ApiResponse<AttendanceRecordDto>> UpdateAsync(Guid id, UpdateAttendanceRecordDto dto)
    {
        var record = await _context.AttendanceRecords.FindAsync(id);
        if (record == null)
            return ApiResponse<AttendanceRecordDto>.FailureResponse("Запись посещаемости не найдена");

        if (dto.CheckInTime.HasValue)
            record.CheckInTime = dto.CheckInTime.Value;
        if (dto.CheckOutTime.HasValue)
            record.CheckOutTime = dto.CheckOutTime.Value;
        if (!string.IsNullOrEmpty(dto.Status) && Enum.TryParse<AttendanceStatus>(dto.Status, out var status))
            record.Status = status;
        if (dto.Notes != null)
            record.Notes = dto.Notes;

        record.WorkedHours = CalculateWorkedHours(record.CheckInTime, record.CheckOutTime);
        record.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return ApiResponse<AttendanceRecordDto>.SuccessResponse(MapToDto(record), "Запись посещаемости обновлена");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id)
    {
        var record = await _context.AttendanceRecords.FindAsync(id);
        if (record == null)
            return ApiResponse<bool>.FailureResponse("Запись посещаемости не найдена");

        _context.AttendanceRecords.Remove(record);
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResponse(true, "Запись посещаемости удалена");
    }

    public async Task<ApiResponse<AttendanceRecordDto>> CheckInAsync(CheckInDto dto)
    {
        var today = DateTime.UtcNow.Date;
        var existing = await _context.AttendanceRecords
            .FirstOrDefaultAsync(a => a.EmployeeId == dto.EmployeeId && a.Date.Date == today);

        if (existing != null && existing.CheckInTime.HasValue)
            return ApiResponse<AttendanceRecordDto>.FailureResponse("Сотрудник уже отметился на приход сегодня");

        var schedule = await _context.WorkSchedules
            .FirstOrDefaultAsync(s => s.EmployeeId == dto.EmployeeId && s.DayOfWeek == today.DayOfWeek);

        var now = DateTime.UtcNow.TimeOfDay;
        var status = AttendanceStatus.Present;

        if (schedule != null && schedule.IsWorkingDay && now > schedule.StartTime.Add(TimeSpan.FromMinutes(15)))
            status = AttendanceStatus.Late;

        AttendanceRecord record;
        if (existing != null)
        {
            existing.CheckInTime = now;
            existing.Status = status;
            existing.Notes = dto.Notes;
            existing.UpdatedAt = DateTime.UtcNow;
            record = existing;
        }
        else
        {
            record = new AttendanceRecord
            {
                Id = Guid.NewGuid(),
                EmployeeId = dto.EmployeeId,
                Date = today,
                CheckInTime = now,
                Status = status,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow
            };
            _context.AttendanceRecords.Add(record);
        }

        await _context.SaveChangesAsync();

        await _eventBus.PublishAsync(new AttendanceMarkedEvent(
            record.Id,
            record.EmployeeId,
            record.Date,
            record.Status.ToString()));

        return ApiResponse<AttendanceRecordDto>.SuccessResponse(MapToDto(record), "Отметка прихода зарегистрирована");
    }

    public async Task<ApiResponse<AttendanceRecordDto>> CheckOutAsync(CheckOutDto dto)
    {
        var today = DateTime.UtcNow.Date;
        var record = await _context.AttendanceRecords
            .FirstOrDefaultAsync(a => a.EmployeeId == dto.EmployeeId && a.Date.Date == today);

        if (record == null)
            return ApiResponse<AttendanceRecordDto>.FailureResponse("Отметка прихода не найдена. Сначала отметьтесь на приход");

        if (record.CheckOutTime.HasValue)
            return ApiResponse<AttendanceRecordDto>.FailureResponse("Сотрудник уже отметился на уход сегодня");

        var now = DateTime.UtcNow.TimeOfDay;
        record.CheckOutTime = now;
        record.WorkedHours = CalculateWorkedHours(record.CheckInTime, record.CheckOutTime);

        var schedule = await _context.WorkSchedules
            .FirstOrDefaultAsync(s => s.EmployeeId == dto.EmployeeId && s.DayOfWeek == today.DayOfWeek);

        if (schedule != null && record.WorkedHours.HasValue)
        {
            var expectedHours = (schedule.EndTime - schedule.StartTime).TotalHours;
            if (schedule.BreakDuration.HasValue)
                expectedHours -= schedule.BreakDuration.Value.TotalHours;

            if ((double)record.WorkedHours.Value > expectedHours)
                record.OvertimeHours = record.WorkedHours.Value - (decimal)expectedHours;
        }

        if (!string.IsNullOrEmpty(dto.Notes))
            record.Notes = (record.Notes ?? "") + " | " + dto.Notes;

        record.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ApiResponse<AttendanceRecordDto>.SuccessResponse(MapToDto(record), "Отметка ухода зарегистрирована");
    }

    private static decimal? CalculateWorkedHours(TimeSpan? checkIn, TimeSpan? checkOut)
    {
        if (!checkIn.HasValue || !checkOut.HasValue)
            return null;
        return (decimal)(checkOut.Value - checkIn.Value).TotalHours;
    }

    private static AttendanceRecordDto MapToDto(AttendanceRecord record) => new(
        record.Id,
        record.EmployeeId,
        record.Date,
        record.CheckInTime,
        record.CheckOutTime,
        record.Status.ToString(),
        record.Notes,
        record.WorkedHours,
        record.OvertimeHours);
}
