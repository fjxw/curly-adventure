using HRManagement.Employees.Api.Application.DTOs;
using HRManagement.Employees.Api.Domain.Entities;
using HRManagement.Employees.Api.Infrastructure.Data;
using HRManagement.Shared.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Employees.Api.Application.Services;

public interface ILeaveService
{
    Task<ApiResponse<LeaveRequestDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<IEnumerable<LeaveRequestDto>>> GetByEmployeeAsync(Guid employeeId, CancellationToken ct = default);
    Task<ApiResponse<IEnumerable<LeaveRequestDto>>> GetPendingAsync(CancellationToken ct = default);
    Task<ApiResponse<LeaveRequestDto>> CreateAsync(CreateLeaveRequest request, CancellationToken ct = default);
    Task<ApiResponse<LeaveRequestDto>> UpdateAsync(Guid id, UpdateLeaveRequest request, CancellationToken ct = default);
    Task<ApiResponse<LeaveRequestDto>> ApproveAsync(Guid id, Guid approverId, ApproveLeaveRequest request, CancellationToken ct = default);
    Task<ApiResponse> CancelAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<int>> GetRemainingDaysAsync(Guid employeeId, int year, CancellationToken ct = default);
}

public class LeaveService : ILeaveService
{
    private readonly EmployeesDbContext _context;
    private const int AnnualLeaveDays = 28;

    public LeaveService(EmployeesDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<LeaveRequestDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var leave = await _context.LeaveRequests
            .Include(l => l.Employee)
            .FirstOrDefaultAsync(l => l.Id == id, ct);

        if (leave == null)
            return ApiResponse<LeaveRequestDto>.FailureResponse("Заявка на отпуск не найдена");

        return ApiResponse<LeaveRequestDto>.SuccessResponse(MapToDto(leave));
    }

    public async Task<ApiResponse<IEnumerable<LeaveRequestDto>>> GetByEmployeeAsync(Guid employeeId, CancellationToken ct = default)
    {
        var leaves = await _context.LeaveRequests
            .Include(l => l.Employee)
            .Where(l => l.EmployeeId == employeeId)
            .OrderByDescending(l => l.StartDate)
            .ToListAsync(ct);

        return ApiResponse<IEnumerable<LeaveRequestDto>>.SuccessResponse(leaves.Select(MapToDto));
    }

    public async Task<ApiResponse<IEnumerable<LeaveRequestDto>>> GetPendingAsync(CancellationToken ct = default)
    {
        var leaves = await _context.LeaveRequests
            .Include(l => l.Employee)
            .Where(l => l.Status == LeaveStatus.Pending)
            .OrderBy(l => l.StartDate)
            .ToListAsync(ct);

        return ApiResponse<IEnumerable<LeaveRequestDto>>.SuccessResponse(leaves.Select(MapToDto));
    }

    public async Task<ApiResponse<LeaveRequestDto>> CreateAsync(CreateLeaveRequest request, CancellationToken ct = default)
    {
        var employee = await _context.Employees.FindAsync(new object[] { request.EmployeeId }, ct);
        if (employee == null)
            return ApiResponse<LeaveRequestDto>.FailureResponse("Сотрудник не найден");

        if (!Enum.TryParse<LeaveType>(request.Type, true, out var leaveType))
            return ApiResponse<LeaveRequestDto>.FailureResponse("Неверный тип отпуска");

        if (request.StartDate > request.EndDate)
            return ApiResponse<LeaveRequestDto>.FailureResponse("Дата начала не может быть позже даты окончания");

        if (request.StartDate < DateTime.UtcNow.Date)
            return ApiResponse<LeaveRequestDto>.FailureResponse("Нельзя создать заявку на прошедшую дату");

        var overlapping = await _context.LeaveRequests
            .Where(l => l.EmployeeId == request.EmployeeId 
                && l.Status != LeaveStatus.Rejected 
                && l.Status != LeaveStatus.Cancelled
                && l.StartDate <= request.EndDate 
                && l.EndDate >= request.StartDate)
            .AnyAsync(ct);

        if (overlapping)
            return ApiResponse<LeaveRequestDto>.FailureResponse("На указанный период уже есть заявка на отпуск");

        var leave = new LeaveRequest
        {
            EmployeeId = request.EmployeeId,
            Type = leaveType,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Reason = request.Reason,
            Status = LeaveStatus.Pending
        };

        _context.LeaveRequests.Add(leave);
        await _context.SaveChangesAsync(ct);

        leave.Employee = employee;
        return ApiResponse<LeaveRequestDto>.SuccessResponse(MapToDto(leave), "Заявка на отпуск создана");
    }

    public async Task<ApiResponse<LeaveRequestDto>> UpdateAsync(Guid id, UpdateLeaveRequest request, CancellationToken ct = default)
    {
        var leave = await _context.LeaveRequests
            .Include(l => l.Employee)
            .FirstOrDefaultAsync(l => l.Id == id, ct);

        if (leave == null)
            return ApiResponse<LeaveRequestDto>.FailureResponse("Заявка на отпуск не найдена");

        if (leave.Status != LeaveStatus.Pending)
            return ApiResponse<LeaveRequestDto>.FailureResponse("Можно изменять только заявки в статусе ожидания");

        if (request.StartDate.HasValue)
            leave.StartDate = request.StartDate.Value;
        
        if (request.EndDate.HasValue)
            leave.EndDate = request.EndDate.Value;
        
        if (request.Reason != null)
            leave.Reason = request.Reason;

        leave.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        return ApiResponse<LeaveRequestDto>.SuccessResponse(MapToDto(leave), "Заявка на отпуск обновлена");
    }

    public async Task<ApiResponse<LeaveRequestDto>> ApproveAsync(Guid id, Guid approverId, ApproveLeaveRequest request, CancellationToken ct = default)
    {
        var leave = await _context.LeaveRequests
            .Include(l => l.Employee)
            .FirstOrDefaultAsync(l => l.Id == id, ct);

        if (leave == null)
            return ApiResponse<LeaveRequestDto>.FailureResponse("Заявка на отпуск не найдена");

        if (leave.Status != LeaveStatus.Pending)
            return ApiResponse<LeaveRequestDto>.FailureResponse("Заявка уже рассмотрена");

        leave.Status = request.Approved ? LeaveStatus.Approved : LeaveStatus.Rejected;
        leave.ApprovedById = approverId;
        leave.ApprovedAt = DateTime.UtcNow;
        leave.ApproverComment = request.Comment;
        leave.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        var statusMessage = request.Approved ? "Заявка одобрена" : "Заявка отклонена";
        return ApiResponse<LeaveRequestDto>.SuccessResponse(MapToDto(leave), statusMessage);
    }

    public async Task<ApiResponse> CancelAsync(Guid id, CancellationToken ct = default)
    {
        var leave = await _context.LeaveRequests.FindAsync(new object[] { id }, ct);
        if (leave == null)
            return ApiResponse.FailureResponse("Заявка на отпуск не найдена");

        if (leave.Status != LeaveStatus.Pending && leave.Status != LeaveStatus.Approved)
            return ApiResponse.FailureResponse("Невозможно отменить заявку в текущем статусе");

        leave.Status = LeaveStatus.Cancelled;
        leave.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        return ApiResponse.SuccessResponse("Заявка отменена");
    }

    public async Task<ApiResponse<int>> GetRemainingDaysAsync(Guid employeeId, int year, CancellationToken ct = default)
    {
        var usedDays = await _context.LeaveRequests
            .Where(l => l.EmployeeId == employeeId 
                && l.Type == LeaveType.Annual 
                && l.Status == LeaveStatus.Approved
                && l.StartDate.Year == year)
            .SumAsync(l => (l.EndDate - l.StartDate).Days + 1, ct);

        var remaining = AnnualLeaveDays - usedDays;
        return ApiResponse<int>.SuccessResponse(remaining);
    }

    private static LeaveRequestDto MapToDto(LeaveRequest leave) => new(
        leave.Id,
        leave.EmployeeId,
        $"{leave.Employee?.LastName} {leave.Employee?.FirstName}",
        leave.Type.ToString(),
        leave.StartDate,
        leave.EndDate,
        leave.TotalDays,
        leave.Status.ToString(),
        leave.Reason,
        leave.ApprovedById,
        leave.ApprovedAt,
        leave.ApproverComment,
        leave.CreatedAt);
}
