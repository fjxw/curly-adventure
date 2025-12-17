using HRManagement.Recruitment.Api.Application.DTOs;
using HRManagement.Recruitment.Api.Domain.Entities;
using HRManagement.Recruitment.Api.Infrastructure.Data;
using HRManagement.Shared.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Recruitment.Api.Application.Services;

public interface ITrainingService
{
    Task<ApiResponse<IEnumerable<TrainingDto>>> GetAllAsync(CancellationToken ct = default);
    Task<ApiResponse<IEnumerable<TrainingDto>>> GetUpcomingAsync(CancellationToken ct = default);
    Task<ApiResponse<TrainingDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<TrainingDto>> CreateAsync(CreateTrainingRequest request, CancellationToken ct = default);
    Task<ApiResponse<TrainingDto>> UpdateAsync(Guid id, UpdateTrainingRequest request, CancellationToken ct = default);
    Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<bool>> AddParticipantAsync(Guid trainingId, Guid employeeId, CancellationToken ct = default);
    Task<ApiResponse<bool>> RemoveParticipantAsync(Guid trainingId, Guid employeeId, CancellationToken ct = default);
    Task<ApiResponse<bool>> CompleteParticipantTrainingAsync(Guid trainingId, Guid employeeId, string? certificateNumber, CancellationToken ct = default);
    Task<ApiResponse<IEnumerable<TrainingDto>>> GetByEmployeeAsync(Guid employeeId, CancellationToken ct = default);
}

public class TrainingService : ITrainingService
{
    private readonly RecruitmentDbContext _context;

    public TrainingService(RecruitmentDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<IEnumerable<TrainingDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var trainings = await _context.Trainings
            .Include(t => t.Participants)
            .OrderByDescending(t => t.StartDate)
            .Select(t => MapToDto(t))
            .ToListAsync(ct);

        return ApiResponse<IEnumerable<TrainingDto>>.SuccessResponse(trainings);
    }

    public async Task<ApiResponse<IEnumerable<TrainingDto>>> GetUpcomingAsync(CancellationToken ct = default)
    {
        var trainings = await _context.Trainings
            .Include(t => t.Participants)
            .Where(t => t.StartDate > DateTime.UtcNow && t.IsActive)
            .OrderBy(t => t.StartDate)
            .Select(t => MapToDto(t))
            .ToListAsync(ct);

        return ApiResponse<IEnumerable<TrainingDto>>.SuccessResponse(trainings);
    }

    public async Task<ApiResponse<TrainingDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var training = await _context.Trainings
            .Include(t => t.Participants)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

        if (training == null)
            return ApiResponse<TrainingDto>.FailureResponse("Обучение не найдено");

        return ApiResponse<TrainingDto>.SuccessResponse(MapToDto(training));
    }

    public async Task<ApiResponse<TrainingDto>> CreateAsync(CreateTrainingRequest request, CancellationToken ct = default)
    {
        var training = new Training
        {
            Title = request.Title,
            Description = request.Description,
            Type = request.Type,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            DurationHours = request.DurationHours,
            Provider = request.Provider,
            Location = request.Location,
            Cost = request.Cost,
            MaxParticipants = request.MaxParticipants,
            IsActive = true
        };

        _context.Trainings.Add(training);
        await _context.SaveChangesAsync(ct);

        return ApiResponse<TrainingDto>.SuccessResponse(MapToDto(training), "Обучение успешно создано");
    }

    public async Task<ApiResponse<TrainingDto>> UpdateAsync(Guid id, UpdateTrainingRequest request, CancellationToken ct = default)
    {
        var training = await _context.Trainings
            .Include(t => t.Participants)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

        if (training == null)
            return ApiResponse<TrainingDto>.FailureResponse("Обучение не найдено");

        training.Title = request.Title;
        training.Description = request.Description;
        training.Type = request.Type;
        training.StartDate = request.StartDate;
        training.EndDate = request.EndDate;
        training.DurationHours = request.DurationHours;
        training.Provider = request.Provider;
        training.Location = request.Location;
        training.Cost = request.Cost;
        training.MaxParticipants = request.MaxParticipants;
        training.IsActive = request.IsActive;
        training.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        return ApiResponse<TrainingDto>.SuccessResponse(MapToDto(training), "Обучение успешно обновлено");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var training = await _context.Trainings.FindAsync(new object[] { id }, ct);
        if (training == null)
            return ApiResponse<bool>.FailureResponse("Обучение не найдено");

        _context.Trainings.Remove(training);
        await _context.SaveChangesAsync(ct);

        return ApiResponse<bool>.SuccessResponse(true, "Обучение успешно удалено");
    }

    public async Task<ApiResponse<bool>> AddParticipantAsync(Guid trainingId, Guid employeeId, CancellationToken ct = default)
    {
        var training = await _context.Trainings
            .Include(t => t.Participants)
            .FirstOrDefaultAsync(t => t.Id == trainingId, ct);

        if (training == null)
            return ApiResponse<bool>.FailureResponse("Обучение не найдено");

        if (training.MaxParticipants > 0 && training.Participants.Count >= training.MaxParticipants)
            return ApiResponse<bool>.FailureResponse("Достигнуто максимальное количество участников");

        var existingParticipant = training.Participants.FirstOrDefault(p => p.EmployeeId == employeeId);
        if (existingParticipant != null)
            return ApiResponse<bool>.FailureResponse("Сотрудник уже записан на это обучение");

        var participant = new TrainingParticipant
        {
            TrainingId = trainingId,
            EmployeeId = employeeId,
            EmployeeName = "Employee",
            Status = ParticipationStatus.Registered,
            RegisteredAt = DateTime.UtcNow
        };

        _context.TrainingParticipants.Add(participant);
        await _context.SaveChangesAsync(ct);

        return ApiResponse<bool>.SuccessResponse(true, "Участник успешно добавлен");
    }

    public async Task<ApiResponse<bool>> RemoveParticipantAsync(Guid trainingId, Guid employeeId, CancellationToken ct = default)
    {
        var participant = await _context.TrainingParticipants
            .FirstOrDefaultAsync(p => p.TrainingId == trainingId && p.EmployeeId == employeeId, ct);

        if (participant == null)
            return ApiResponse<bool>.FailureResponse("Участник не найден");

        _context.TrainingParticipants.Remove(participant);
        await _context.SaveChangesAsync(ct);

        return ApiResponse<bool>.SuccessResponse(true, "Участник успешно удалён");
    }

    public async Task<ApiResponse<bool>> CompleteParticipantTrainingAsync(Guid trainingId, Guid employeeId, string? certificateNumber, CancellationToken ct = default)
    {
        var participant = await _context.TrainingParticipants
            .FirstOrDefaultAsync(p => p.TrainingId == trainingId && p.EmployeeId == employeeId, ct);

        if (participant == null)
            return ApiResponse<bool>.FailureResponse("Участник не найден");

        participant.Status = ParticipationStatus.Completed;
        participant.CompletedAt = DateTime.UtcNow;
        participant.CertificateNumber = certificateNumber;

        await _context.SaveChangesAsync(ct);

        return ApiResponse<bool>.SuccessResponse(true, "Обучение успешно завершено");
    }

    public async Task<ApiResponse<IEnumerable<TrainingDto>>> GetByEmployeeAsync(Guid employeeId, CancellationToken ct = default)
    {
        var trainings = await _context.TrainingParticipants
            .Include(p => p.Training)
                .ThenInclude(t => t.Participants)
            .Where(p => p.EmployeeId == employeeId)
            .Select(p => MapToDto(p.Training))
            .ToListAsync(ct);

        return ApiResponse<IEnumerable<TrainingDto>>.SuccessResponse(trainings);
    }

    private static TrainingDto MapToDto(Training t) => new(
        t.Id,
        t.Title,
        t.Description,
        t.Type,
        t.StartDate,
        t.EndDate,
        t.DurationHours,
        t.Provider,
        t.Location,
        t.Cost,
        t.MaxParticipants,
        t.Participants?.Count ?? 0,
        t.IsActive,
        t.CreatedAt
    );
}
