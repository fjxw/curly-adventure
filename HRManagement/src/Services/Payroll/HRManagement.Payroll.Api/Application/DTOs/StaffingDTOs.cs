namespace HRManagement.Payroll.Api.Application.DTOs;

public record CreateStaffingTableRequest(
    string Name,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo);

public record UpdateStaffingTableRequest(
    string Name,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo,
    bool IsActive);

public record CreateStaffingPositionRequest(
    Guid DepartmentId,
    Guid PositionId,
    string PositionName,
    string DepartmentName,
    int HeadCount,
    decimal Salary);

public record StaffingTableDto(
    Guid Id,
    string Name,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo,
    bool IsActive,
    int TotalPositions,
    int TotalHeadCount);

public record StaffingPositionDto(
    Guid Id,
    Guid StaffingTableId,
    Guid DepartmentId,
    Guid PositionId,
    string PositionName,
    string DepartmentName,
    int HeadCount,
    int OccupiedCount,
    int VacantCount,
    decimal Salary);
