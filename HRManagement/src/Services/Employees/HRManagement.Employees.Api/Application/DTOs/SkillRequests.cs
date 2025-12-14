namespace HRManagement.Employees.Api.Application.DTOs;

public record CreateSkillRequest(
    string Name,
    string? Description,
    string? Category);

public record AssignSkillRequest(
    Guid EmployeeId,
    Guid SkillId,
    string Level,
    int? YearsOfExperience,
    DateTime? CertifiedDate,
    string? CertificateNumber);

public record UpdateEmployeeSkillRequest(
    string? Level,
    int? YearsOfExperience,
    DateTime? CertifiedDate,
    string? CertificateNumber);

public record SkillDto(
    Guid Id,
    string Name,
    string? Description,
    string? Category);

public record EmployeeSkillDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    Guid SkillId,
    string SkillName,
    string Level,
    int? YearsOfExperience,
    DateTime? CertifiedDate,
    string? CertificateNumber);
