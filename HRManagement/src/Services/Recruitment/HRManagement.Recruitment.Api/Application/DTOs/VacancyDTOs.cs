using HRManagement.Recruitment.Api.Domain.Entities;

namespace HRManagement.Recruitment.Api.Application.DTOs;

public record CreateVacancyRequest(
    string Title,
    string Description,
    string Requirements,
    Guid DepartmentId,
    string DepartmentName,
    Guid PositionId,
    string PositionName,
    decimal SalaryFrom,
    decimal SalaryTo);

public record UpdateVacancyRequest(
    string Title,
    string Description,
    string Requirements,
    decimal SalaryFrom,
    decimal SalaryTo,
    VacancyStatus Status);

public record VacancyDto(
    Guid Id,
    string Title,
    string Description,
    string Requirements,
    Guid DepartmentId,
    string DepartmentName,
    Guid PositionId,
    string PositionName,
    decimal SalaryFrom,
    decimal SalaryTo,
    VacancyStatus Status,
    int CandidatesCount,
    DateTime CreatedAt);
