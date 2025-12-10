using HRManagement.Recruitment.Api.Application.DTOs;
using HRManagement.Recruitment.Api.Domain.Entities;
using HRManagement.Recruitment.Api.Infrastructure.Data;
using HRManagement.Shared.Common.Models;
using HRManagement.Shared.Contracts.Events;
using HRManagement.Shared.MessageBus;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Recruitment.Api.Application.Services;

public interface ICandidateService
{
    Task<ApiResponse<IEnumerable<CandidateDto>>> GetAllAsync(CancellationToken ct = default);
    Task<ApiResponse<CandidateDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<IEnumerable<CandidateDto>>> GetByVacancyAsync(Guid vacancyId, CancellationToken ct = default);
    Task<ApiResponse<CandidateDto>> CreateAsync(CreateCandidateRequest request, CancellationToken ct = default);
    Task<ApiResponse<CandidateDto>> UpdateAsync(Guid id, UpdateCandidateRequest request, CancellationToken ct = default);
    Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<bool>> HireAsync(Guid id, Guid departmentId, Guid positionId, CancellationToken ct = default);
    Task<ApiResponse<bool>> RejectAsync(Guid id, CancellationToken ct = default);
}

public class CandidateService : ICandidateService
{
    private readonly RecruitmentDbContext _context;
    private readonly IEventBus _eventBus;

    public CandidateService(RecruitmentDbContext context, IEventBus eventBus)
    {
        _context = context;
        _eventBus = eventBus;
    }

    public async Task<ApiResponse<IEnumerable<CandidateDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var candidates = await _context.Candidates
            .Include(c => c.Vacancy)
            .Include(c => c.Interviews)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => MapToDto(c))
            .ToListAsync(ct);

        return ApiResponse<IEnumerable<CandidateDto>>.SuccessResponse(candidates);
    }

    public async Task<ApiResponse<CandidateDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var candidate = await _context.Candidates
            .Include(c => c.Vacancy)
            .Include(c => c.Interviews)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (candidate == null)
            return ApiResponse<CandidateDto>.FailureResponse("Кандидат не найден");

        return ApiResponse<CandidateDto>.SuccessResponse(MapToDto(candidate));
    }

    public async Task<ApiResponse<IEnumerable<CandidateDto>>> GetByVacancyAsync(Guid vacancyId, CancellationToken ct = default)
    {
        var candidates = await _context.Candidates
            .Include(c => c.Vacancy)
            .Include(c => c.Interviews)
            .Where(c => c.VacancyId == vacancyId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => MapToDto(c))
            .ToListAsync(ct);

        return ApiResponse<IEnumerable<CandidateDto>>.SuccessResponse(candidates);
    }

    public async Task<ApiResponse<CandidateDto>> CreateAsync(CreateCandidateRequest request, CancellationToken ct = default)
    {
        var vacancy = await _context.Vacancies.FindAsync(new object[] { request.VacancyId }, ct);
        if (vacancy == null)
            return ApiResponse<CandidateDto>.FailureResponse("Вакансия не найдена");

        if (vacancy.Status != VacancyStatus.Open)
            return ApiResponse<CandidateDto>.FailureResponse("Вакансия закрыта для подачи заявок");

        var candidate = new Candidate
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            ResumeUrl = request.ResumeUrl,
            CoverLetter = request.CoverLetter,
            YearsOfExperience = request.YearsOfExperience,
            Education = request.Education,
            Skills = request.Skills,
            VacancyId = request.VacancyId,
            Status = CandidateStatus.New
        };

        _context.Candidates.Add(candidate);
        await _context.SaveChangesAsync(ct);

        candidate.Vacancy = vacancy;

        return ApiResponse<CandidateDto>.SuccessResponse(MapToDto(candidate), "Кандидат успешно добавлен");
    }

    public async Task<ApiResponse<CandidateDto>> UpdateAsync(Guid id, UpdateCandidateRequest request, CancellationToken ct = default)
    {
        var candidate = await _context.Candidates
            .Include(c => c.Vacancy)
            .Include(c => c.Interviews)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (candidate == null)
            return ApiResponse<CandidateDto>.FailureResponse("Кандидат не найден");

        if (!string.IsNullOrEmpty(request.FirstName))
            candidate.FirstName = request.FirstName;
        
        if (!string.IsNullOrEmpty(request.LastName))
            candidate.LastName = request.LastName;
        
        if (!string.IsNullOrEmpty(request.Email))
            candidate.Email = request.Email;
        
        if (request.Phone != null)
            candidate.Phone = request.Phone;
        
        if (request.ResumeUrl != null)
            candidate.ResumeUrl = request.ResumeUrl;
        
        if (request.CoverLetter != null)
            candidate.CoverLetter = request.CoverLetter;
        
        if (request.YearsOfExperience.HasValue)
            candidate.YearsOfExperience = request.YearsOfExperience.Value;
        
        if (request.Education != null)
            candidate.Education = request.Education;
        
        if (request.Skills != null)
            candidate.Skills = request.Skills;

        candidate.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        return ApiResponse<CandidateDto>.SuccessResponse(MapToDto(candidate), "Кандидат успешно обновлён");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var candidate = await _context.Candidates.FindAsync(new object[] { id }, ct);
        if (candidate == null)
            return ApiResponse<bool>.FailureResponse("Кандидат не найден");

        _context.Candidates.Remove(candidate);
        await _context.SaveChangesAsync(ct);

        return ApiResponse<bool>.SuccessResponse(true, "Кандидат успешно удалён");
    }

    public async Task<ApiResponse<bool>> HireAsync(Guid id, Guid departmentId, Guid positionId, CancellationToken ct = default)
    {
        var candidate = await _context.Candidates
            .Include(c => c.Vacancy)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (candidate == null)
            return ApiResponse<bool>.FailureResponse("Кандидат не найден");

        if (candidate.Status == CandidateStatus.Hired)
            return ApiResponse<bool>.FailureResponse("Кандидат уже принят на работу");

        candidate.Status = CandidateStatus.Hired;
        
        // Close vacancy
        if (candidate.Vacancy != null)
        {
            candidate.Vacancy.Status = VacancyStatus.Filled;
            candidate.Vacancy.ClosedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(ct);

        // Publish event to create employee
        await _eventBus.PublishAsync(new CandidateHiredEvent(
            candidate.Id,
            candidate.FirstName,
            candidate.LastName,
            candidate.Email,
            positionId,
            0,
            DateTime.UtcNow,
            DateTime.UtcNow), ct);

        return ApiResponse<bool>.SuccessResponse(true, "Кандидат принят на работу");
    }

    public async Task<ApiResponse<bool>> RejectAsync(Guid id, CancellationToken ct = default)
    {
        var candidate = await _context.Candidates.FindAsync(new object[] { id }, ct);
        if (candidate == null)
            return ApiResponse<bool>.FailureResponse("Кандидат не найден");

        candidate.Status = CandidateStatus.Rejected;
        candidate.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        return ApiResponse<bool>.SuccessResponse(true, "Кандидат отклонён");
    }

    private static CandidateDto MapToDto(Candidate c) => new(
        c.Id,
        c.FirstName,
        c.LastName,
        c.Email,
        c.Phone,
        c.ResumeUrl,
        c.YearsOfExperience,
        c.Education,
        c.Skills,
        c.VacancyId,
        c.Vacancy?.Title ?? string.Empty,
        c.Status.ToString(),
        c.Notes,
        c.Interviews?.Count ?? 0,
        c.CreatedAt
    );
}
