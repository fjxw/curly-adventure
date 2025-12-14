namespace HRManagement.Employees.Api.Application.DTOs;

public record CreateLeaveRequest(
    Guid EmployeeId,
    string Type,
    DateTime StartDate,
    DateTime EndDate,
    string? Reason);

public record UpdateLeaveRequest(
    DateTime? StartDate,
    DateTime? EndDate,
    string? Reason);

public record ApproveLeaveRequest(
    bool Approved,
    string? Comment);

public record LeaveRequestDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    string Type,
    DateTime StartDate,
    DateTime EndDate,
    int TotalDays,
    string Status,
    string? Reason,
    Guid? ApprovedById,
    DateTime? ApprovedAt,
    string? ApproverComment,
    DateTime CreatedAt);
