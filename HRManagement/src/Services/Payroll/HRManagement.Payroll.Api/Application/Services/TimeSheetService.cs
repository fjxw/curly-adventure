using HRManagement.Payroll.Api.Application.DTOs;
using HRManagement.Payroll.Api.Domain.Entities;
using HRManagement.Payroll.Api.Infrastructure.Data;
using HRManagement.Shared.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Payroll.Api.Application.Services;

public interface ITimeSheetService
{
    Task<ApiResponse<TimeSheetDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<IEnumerable<TimeSheetDto>>> GetByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default);
    Task<ApiResponse<IEnumerable<TimeSheetDto>>> GetByPeriodAsync(int month, int year, CancellationToken cancellationToken = default);
    Task<ApiResponse<TimeSheetDto>> CreateAsync(CreateTimeSheetRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<TimeSheetDto>> UpdateAsync(Guid id, UpdateTimeSheetRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse> ApproveAsync(Guid id, Guid approvedById, CancellationToken cancellationToken = default);
}

public class TimeSheetService : ITimeSheetService
{
    private readonly PayrollDbContext _context;

    public TimeSheetService(PayrollDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<TimeSheetDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var timeSheet = await _context.TimeSheets
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (timeSheet == null)
            return ApiResponse<TimeSheetDto>.FailureResponse("Табель не найден");

        return ApiResponse<TimeSheetDto>.SuccessResponse(MapToDto(timeSheet));
    }

    public async Task<ApiResponse<IEnumerable<TimeSheetDto>>> GetByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        var timeSheets = await _context.TimeSheets
            .Where(t => t.EmployeeId == employeeId)
            .OrderByDescending(t => t.Year)
            .ThenByDescending(t => t.Month)
            .ToListAsync(cancellationToken);

        return ApiResponse<IEnumerable<TimeSheetDto>>.SuccessResponse(timeSheets.Select(MapToDto));
    }

    public async Task<ApiResponse<IEnumerable<TimeSheetDto>>> GetByPeriodAsync(int month, int year, CancellationToken cancellationToken = default)
    {
        var timeSheets = await _context.TimeSheets
            .Where(t => t.Month == month && t.Year == year)
            .OrderBy(t => t.EmployeeName)
            .ToListAsync(cancellationToken);

        return ApiResponse<IEnumerable<TimeSheetDto>>.SuccessResponse(timeSheets.Select(MapToDto));
    }

    public async Task<ApiResponse<TimeSheetDto>> CreateAsync(CreateTimeSheetRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await _context.TimeSheets
            .FirstOrDefaultAsync(t => t.EmployeeId == request.EmployeeId 
                && t.Month == request.Month 
                && t.Year == request.Year, cancellationToken);

        if (existing != null)
            return ApiResponse<TimeSheetDto>.FailureResponse("Табель за этот период уже существует");

        var timeSheet = new TimeSheet
        {
            EmployeeId = request.EmployeeId,
            EmployeeName = request.EmployeeName,
            Month = request.Month,
            Year = request.Year,
            WorkedHours = request.WorkedHours,
            OvertimeHours = request.OvertimeHours,
            NightHours = request.NightHours,
            HolidayHours = request.HolidayHours,
            SickLeaveHours = request.SickLeaveHours,
            VacationHours = request.VacationHours
        };

        _context.TimeSheets.Add(timeSheet);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<TimeSheetDto>.SuccessResponse(MapToDto(timeSheet), "Табель создан");
    }

    public async Task<ApiResponse<TimeSheetDto>> UpdateAsync(Guid id, UpdateTimeSheetRequest request, CancellationToken cancellationToken = default)
    {
        var timeSheet = await _context.TimeSheets.FindAsync(new object[] { id }, cancellationToken);
        if (timeSheet == null)
            return ApiResponse<TimeSheetDto>.FailureResponse("Табель не найден");

        if (timeSheet.IsApproved)
            return ApiResponse<TimeSheetDto>.FailureResponse("Нельзя изменить утверждённый табель");

        timeSheet.WorkedHours = request.WorkedHours;
        timeSheet.OvertimeHours = request.OvertimeHours;
        timeSheet.NightHours = request.NightHours;
        timeSheet.HolidayHours = request.HolidayHours;
        timeSheet.SickLeaveHours = request.SickLeaveHours;
        timeSheet.VacationHours = request.VacationHours;

        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<TimeSheetDto>.SuccessResponse(MapToDto(timeSheet), "Табель обновлён");
    }

    public async Task<ApiResponse> ApproveAsync(Guid id, Guid approvedById, CancellationToken cancellationToken = default)
    {
        var timeSheet = await _context.TimeSheets.FindAsync(new object[] { id }, cancellationToken);
        if (timeSheet == null)
            return ApiResponse.FailureResponse("Табель не найден");

        timeSheet.IsApproved = true;
        timeSheet.ApprovedAt = DateTime.UtcNow;
        timeSheet.ApprovedById = approvedById;

        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse.SuccessResponse("Табель утверждён");
    }

    private static TimeSheetDto MapToDto(TimeSheet ts)
    {
        return new TimeSheetDto(
            ts.Id,
            ts.EmployeeId,
            ts.EmployeeName,
            ts.Month,
            ts.Year,
            ts.WorkedHours,
            ts.OvertimeHours,
            ts.NightHours,
            ts.HolidayHours,
            ts.SickLeaveHours,
            ts.VacationHours,
            ts.IsApproved,
            ts.ApprovedAt);
    }
}
