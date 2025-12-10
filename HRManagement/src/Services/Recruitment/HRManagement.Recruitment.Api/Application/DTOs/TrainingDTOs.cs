using HRManagement.Recruitment.Api.Domain.Entities;

namespace HRManagement.Recruitment.Api.Application.DTOs;

public record CreateTrainingRequest(
    string Title,
    string Description,
    TrainingType Type,
    DateTime StartDate,
    DateTime? EndDate,
    int DurationHours,
    string? Provider,
    string? Location,
    decimal Cost,
    int MaxParticipants);

public record UpdateTrainingRequest(
    string Title,
    string Description,
    TrainingType Type,
    DateTime StartDate,
    DateTime? EndDate,
    int DurationHours,
    string? Provider,
    string? Location,
    decimal Cost,
    int MaxParticipants,
    bool IsActive);

public record TrainingDto(
    Guid Id,
    string Title,
    string Description,
    TrainingType Type,
    DateTime StartDate,
    DateTime? EndDate,
    int DurationHours,
    string? Provider,
    string? Location,
    decimal Cost,
    int MaxParticipants,
    int CurrentParticipants,
    bool IsActive,
    DateTime CreatedAt);

public record RegisterParticipantRequest(
    Guid EmployeeId,
    string EmployeeName);

public record CompleteParticipantRequest(
    int? Score,
    string? Feedback,
    string? CertificateNumber);

public record TrainingParticipantDto(
    Guid Id,
    Guid TrainingId,
    string TrainingTitle,
    Guid EmployeeId,
    string EmployeeName,
    ParticipationStatus Status,
    DateTime RegisteredAt,
    DateTime? CompletedAt,
    string? CertificateNumber,
    int? Score,
    string? Feedback);
