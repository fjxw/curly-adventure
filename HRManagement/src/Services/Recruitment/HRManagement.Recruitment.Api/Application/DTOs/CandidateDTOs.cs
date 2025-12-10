using HRManagement.Recruitment.Api.Domain.Entities;

namespace HRManagement.Recruitment.Api.Application.DTOs;

public record CreateCandidateRequest(
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string? ResumeUrl,
    string? CoverLetter,
    int YearsOfExperience,
    string? Education,
    string? Skills,
    Guid VacancyId);

public record UpdateCandidateRequest(
    string? FirstName,
    string? LastName,
    string? Email,
    string? Phone,
    string? ResumeUrl,
    string? CoverLetter,
    int? YearsOfExperience,
    string? Education,
    string? Skills);

public record UpdateCandidateStatusRequest(
    CandidateStatus Status,
    string? Notes);

public record CandidateDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string? ResumeUrl,
    int YearsOfExperience,
    string? Education,
    string? Skills,
    Guid VacancyId,
    string VacancyTitle,
    string Status,
    string? Notes,
    int InterviewsCount,
    DateTime CreatedAt);

public record CreateInterviewRequest(
    Guid CandidateId,
    DateTime ScheduledDate,
    string InterviewerName,
    string? Notes);

public record UpdateInterviewRequest(
    DateTime? ScheduledDate,
    string? InterviewerName,
    string? Notes,
    string? Status);

public record CompleteInterviewRequest(
    string Feedback,
    int Rating);

public record InterviewDto(
    Guid Id,
    Guid CandidateId,
    string CandidateName,
    DateTime ScheduledDate,
    string InterviewerName,
    string? Notes,
    string Status,
    DateTime CreatedAt);
