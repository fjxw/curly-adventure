using HRManagement.Employees.Api.Application.DTOs;
using HRManagement.Employees.Api.Domain.Entities;
using HRManagement.Employees.Api.Infrastructure.Data;
using HRManagement.Shared.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Employees.Api.Application.Services;

public interface IPositionHistoryService
{
    Task<ApiResponse<IEnumerable<PositionHistoryDto>>> GetByEmployeeAsync(Guid employeeId, CancellationToken ct = default);
    Task<ApiResponse<PositionHistoryDto>> GetCurrentPositionAsync(Guid employeeId, CancellationToken ct = default);
    Task<ApiResponse<PositionHistoryDto>> ChangePositionAsync(CreatePositionChangeRequest request, CancellationToken ct = default);
}

public class PositionHistoryService : IPositionHistoryService
{
    private readonly EmployeesDbContext _context;

    public PositionHistoryService(EmployeesDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<IEnumerable<PositionHistoryDto>>> GetByEmployeeAsync(Guid employeeId, CancellationToken ct = default)
    {
        var history = await _context.PositionHistories
            .Include(ph => ph.Employee)
            .Include(ph => ph.Position)
            .Include(ph => ph.Department)
            .Where(ph => ph.EmployeeId == employeeId)
            .OrderByDescending(ph => ph.StartDate)
            .ToListAsync(ct);

        return ApiResponse<IEnumerable<PositionHistoryDto>>.SuccessResponse(history.Select(MapToDto));
    }

    public async Task<ApiResponse<PositionHistoryDto>> GetCurrentPositionAsync(Guid employeeId, CancellationToken ct = default)
    {
        var current = await _context.PositionHistories
            .Include(ph => ph.Employee)
            .Include(ph => ph.Position)
            .Include(ph => ph.Department)
            .FirstOrDefaultAsync(ph => ph.EmployeeId == employeeId && ph.EndDate == null, ct);

        if (current == null)
            return ApiResponse<PositionHistoryDto>.FailureResponse("Текущая должность не найдена");

        return ApiResponse<PositionHistoryDto>.SuccessResponse(MapToDto(current));
    }

    public async Task<ApiResponse<PositionHistoryDto>> ChangePositionAsync(CreatePositionChangeRequest request, CancellationToken ct = default)
    {
        var employee = await _context.Employees.FindAsync(new object[] { request.EmployeeId }, ct);
        if (employee == null)
            return ApiResponse<PositionHistoryDto>.FailureResponse("Сотрудник не найден");

        var newPosition = await _context.Positions.FindAsync(new object[] { request.NewPositionId }, ct);
        if (newPosition == null)
            return ApiResponse<PositionHistoryDto>.FailureResponse("Должность не найдена");

        var newDepartment = await _context.Departments.FindAsync(new object[] { request.NewDepartmentId }, ct);
        if (newDepartment == null)
            return ApiResponse<PositionHistoryDto>.FailureResponse("Отдел не найден");

        var currentPosition = await _context.PositionHistories
            .FirstOrDefaultAsync(ph => ph.EmployeeId == request.EmployeeId && ph.EndDate == null, ct);

        if (currentPosition != null)
        {
            currentPosition.EndDate = request.EffectiveDate.AddDays(-1);
            currentPosition.UpdatedAt = DateTime.UtcNow;
        }

        var newHistory = new PositionHistory
        {
            EmployeeId = request.EmployeeId,
            PositionId = request.NewPositionId,
            DepartmentId = request.NewDepartmentId,
            StartDate = request.EffectiveDate,
            Salary = request.NewSalary,
            ChangeReason = request.ChangeReason
        };

        employee.PositionId = request.NewPositionId;
        employee.DepartmentId = request.NewDepartmentId;
        employee.UpdatedAt = DateTime.UtcNow;

        _context.PositionHistories.Add(newHistory);
        await _context.SaveChangesAsync(ct);

        newHistory.Employee = employee;
        newHistory.Position = newPosition;
        newHistory.Department = newDepartment;

        return ApiResponse<PositionHistoryDto>.SuccessResponse(MapToDto(newHistory), "Должность изменена");
    }

    private static PositionHistoryDto MapToDto(PositionHistory ph) => new(
        ph.Id,
        ph.EmployeeId,
        $"{ph.Employee?.LastName} {ph.Employee?.FirstName}",
        ph.PositionId,
        ph.Position?.Name ?? string.Empty,
        ph.DepartmentId,
        ph.Department?.Name ?? string.Empty,
        ph.StartDate,
        ph.EndDate,
        ph.Salary,
        ph.ChangeReason,
        ph.IsCurrent);
}
