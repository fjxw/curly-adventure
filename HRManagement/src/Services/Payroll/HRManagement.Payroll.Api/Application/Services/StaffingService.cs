using HRManagement.Payroll.Api.Application.DTOs;
using HRManagement.Payroll.Api.Domain.Entities;
using HRManagement.Payroll.Api.Infrastructure.Data;
using HRManagement.Shared.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Payroll.Api.Application.Services;

public interface IStaffingService
{
    Task<ApiResponse<StaffingTableDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<IEnumerable<StaffingTableDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<StaffingTableDto>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<StaffingTableDto>> CreateAsync(CreateStaffingTableRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<StaffingTableDto>> UpdateAsync(Guid id, UpdateStaffingTableRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse> AddPositionAsync(Guid staffingTableId, CreateStaffingPositionRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<IEnumerable<StaffingPositionDto>>> GetPositionsAsync(Guid staffingTableId, CancellationToken cancellationToken = default);
}

public class StaffingService : IStaffingService
{
    private readonly PayrollDbContext _context;

    public StaffingService(PayrollDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<StaffingTableDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var table = await _context.StaffingTables
            .Include(s => s.Positions)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (table == null)
            return ApiResponse<StaffingTableDto>.FailureResponse("Штатное расписание не найдено");

        return ApiResponse<StaffingTableDto>.SuccessResponse(MapToDto(table));
    }

    public async Task<ApiResponse<IEnumerable<StaffingTableDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var tables = await _context.StaffingTables
            .Include(s => s.Positions)
            .OrderByDescending(s => s.EffectiveFrom)
            .ToListAsync(cancellationToken);

        return ApiResponse<IEnumerable<StaffingTableDto>>.SuccessResponse(tables.Select(MapToDto));
    }

    public async Task<ApiResponse<StaffingTableDto>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var table = await _context.StaffingTables
            .Include(s => s.Positions)
            .FirstOrDefaultAsync(s => s.IsActive, cancellationToken);

        if (table == null)
            return ApiResponse<StaffingTableDto>.FailureResponse("Активное штатное расписание не найдено");

        return ApiResponse<StaffingTableDto>.SuccessResponse(MapToDto(table));
    }

    public async Task<ApiResponse<StaffingTableDto>> CreateAsync(CreateStaffingTableRequest request, CancellationToken cancellationToken = default)
    {
        var table = new StaffingTable
        {
            Name = request.Name,
            EffectiveFrom = request.EffectiveFrom,
            EffectiveTo = request.EffectiveTo,
            IsActive = true
        };

     
        var activeTables = await _context.StaffingTables.Where(s => s.IsActive).ToListAsync(cancellationToken);
        foreach (var activeTable in activeTables)
        {
            activeTable.IsActive = false;
        }

        _context.StaffingTables.Add(table);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<StaffingTableDto>.SuccessResponse(MapToDto(table), "Штатное расписание создано");
    }

    public async Task<ApiResponse<StaffingTableDto>> UpdateAsync(Guid id, UpdateStaffingTableRequest request, CancellationToken cancellationToken = default)
    {
        var table = await _context.StaffingTables
            .Include(s => s.Positions)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (table == null)
            return ApiResponse<StaffingTableDto>.FailureResponse("Штатное расписание не найдено");

        table.Name = request.Name;
        table.EffectiveFrom = request.EffectiveFrom;
        table.EffectiveTo = request.EffectiveTo;

        if (request.IsActive && !table.IsActive)
        {
            var activeTables = await _context.StaffingTables.Where(s => s.IsActive && s.Id != id).ToListAsync(cancellationToken);
            foreach (var activeTable in activeTables)
            {
                activeTable.IsActive = false;
            }
        }
        table.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<StaffingTableDto>.SuccessResponse(MapToDto(table), "Штатное расписание обновлено");
    }

    public async Task<ApiResponse> AddPositionAsync(Guid staffingTableId, CreateStaffingPositionRequest request, CancellationToken cancellationToken = default)
    {
        var table = await _context.StaffingTables.FindAsync(new object[] { staffingTableId }, cancellationToken);
        if (table == null)
            return ApiResponse.FailureResponse("Штатное расписание не найдено");

        var position = new StaffingPosition
        {
            StaffingTableId = staffingTableId,
            DepartmentId = request.DepartmentId,
            PositionId = request.PositionId,
            PositionName = request.PositionName,
            DepartmentName = request.DepartmentName,
            HeadCount = request.HeadCount,
            Salary = request.Salary
        };

        _context.StaffingPositions.Add(position);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse.SuccessResponse("Штатная единица добавлена");
    }

    public async Task<ApiResponse<IEnumerable<StaffingPositionDto>>> GetPositionsAsync(Guid staffingTableId, CancellationToken cancellationToken = default)
    {
        var positions = await _context.StaffingPositions
            .Where(p => p.StaffingTableId == staffingTableId)
            .OrderBy(p => p.DepartmentName)
            .ThenBy(p => p.PositionName)
            .ToListAsync(cancellationToken);

        var dtos = positions.Select(p => new StaffingPositionDto(
            p.Id,
            p.StaffingTableId,
            p.DepartmentId,
            p.PositionId,
            p.PositionName,
            p.DepartmentName,
            p.HeadCount,
            p.OccupiedCount,
            p.HeadCount - p.OccupiedCount,
            p.Salary));

        return ApiResponse<IEnumerable<StaffingPositionDto>>.SuccessResponse(dtos);
    }

    private static StaffingTableDto MapToDto(StaffingTable table)
    {
        return new StaffingTableDto(
            table.Id,
            table.Name,
            table.EffectiveFrom,
            table.EffectiveTo,
            table.IsActive,
            table.Positions?.Count ?? 0,
            table.Positions?.Sum(p => p.HeadCount) ?? 0);
    }
}
