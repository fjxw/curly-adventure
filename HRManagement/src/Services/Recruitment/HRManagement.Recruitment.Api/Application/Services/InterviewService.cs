using HRManagement.Recruitment.Api.Application.DTOs;
using HRManagement.Recruitment.Api.Domain.Entities;
using HRManagement.Recruitment.Api.Infrastructure.Data;
using HRManagement.Shared.Common.Caching;
using HRManagement.Shared.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Recruitment.Api.Application.Services;

public interface IInterviewService
{
    Task<ApiResponse<IEnumerable<InterviewDto>>> GetAllAsync(CancellationToken ct = default);
    Task<ApiResponse<IEnumerable<InterviewDto>>> GetByCandidateAsync(Guid candidateId, CancellationToken ct = default);
    Task<ApiResponse<InterviewDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<InterviewDto>> CreateAsync(CreateInterviewRequest request, CancellationToken ct = default);
    Task<ApiResponse<InterviewDto>> UpdateAsync(Guid id, UpdateInterviewRequest request, CancellationToken ct = default);
    Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken ct = default);
}

public class InterviewService : IInterviewService
{
    private readonly RecruitmentDbContext _context;
    private readonly ICacheService _cacheService;

    public InterviewService(RecruitmentDbContext context, ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<ApiResponse<IEnumerable<InterviewDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var interviews = await _context.Interviews
            .Include(i => i.Candidate)
            .OrderByDescending(i => i.ScheduledDate)
            .Select(i => MapToDto(i))
            .ToListAsync(ct);

        return ApiResponse<IEnumerable<InterviewDto>>.SuccessResponse(interviews);
    }

    public async Task<ApiResponse<IEnumerable<InterviewDto>>> GetByCandidateAsync(Guid candidateId, CancellationToken ct = default)
    {
        var interviews = await _context.Interviews
            .Include(i => i.Candidate)
            .Where(i => i.CandidateId == candidateId)
            .OrderByDescending(i => i.ScheduledDate)
            .Select(i => MapToDto(i))
            .ToListAsync(ct);

        return ApiResponse<IEnumerable<InterviewDto>>.SuccessResponse(interviews);
    }

    public async Task<ApiResponse<InterviewDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var cacheKey = $"interview_{id}";
        var cached = await _cacheService.GetAsync<InterviewDto>(cacheKey);
        if (cached != null)
            return ApiResponse<InterviewDto>.SuccessResponse(cached);

        var interview = await _context.Interviews
            .Include(i => i.Candidate)
            .FirstOrDefaultAsync(i => i.Id == id, ct);

        if (interview == null)
            return ApiResponse<InterviewDto>.FailureResponse("Собеседование не найдено");

        var dto = MapToDto(interview);
        await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(10));

        return ApiResponse<InterviewDto>.SuccessResponse(dto);
    }

    public async Task<ApiResponse<InterviewDto>> CreateAsync(CreateInterviewRequest request, CancellationToken ct = default)
    {
        var candidate = await _context.Candidates.FindAsync(new object[] { request.CandidateId }, ct);
        if (candidate == null)
            return ApiResponse<InterviewDto>.FailureResponse("Кандидат не найден");

        var interview = new Interview
        {
            CandidateId = request.CandidateId,
            ScheduledDate = request.ScheduledDate,
            InterviewerName = request.InterviewerName,
            Notes = request.Notes,
            Status = InterviewStatus.Scheduled
        };

        _context.Interviews.Add(interview);
        await _context.SaveChangesAsync(ct);

        // Update candidate status
        candidate.Status = CandidateStatus.Interview;
        await _context.SaveChangesAsync(ct);

        interview.Candidate = candidate;
        var dto = MapToDto(interview);

        return ApiResponse<InterviewDto>.SuccessResponse(dto, "Собеседование создано успешно");
    }

    public async Task<ApiResponse<InterviewDto>> UpdateAsync(Guid id, UpdateInterviewRequest request, CancellationToken ct = default)
    {
        var interview = await _context.Interviews
            .Include(i => i.Candidate)
            .FirstOrDefaultAsync(i => i.Id == id, ct);

        if (interview == null)
            return ApiResponse<InterviewDto>.FailureResponse("Собеседование не найдено");

        if (request.ScheduledDate.HasValue)
            interview.ScheduledDate = request.ScheduledDate.Value;

        if (!string.IsNullOrEmpty(request.InterviewerName))
            interview.InterviewerName = request.InterviewerName;

        if (request.Notes != null)
            interview.Notes = request.Notes;

        if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<InterviewStatus>(request.Status, out var status))
            interview.Status = status;

        interview.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        await _cacheService.RemoveAsync($"interview_{id}");

        var dto = MapToDto(interview);
        return ApiResponse<InterviewDto>.SuccessResponse(dto, "Собеседование обновлено успешно");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var interview = await _context.Interviews.FindAsync(new object[] { id }, ct);
        if (interview == null)
            return ApiResponse<bool>.FailureResponse("Собеседование не найдено");

        _context.Interviews.Remove(interview);
        await _context.SaveChangesAsync(ct);

        await _cacheService.RemoveAsync($"interview_{id}");

        return ApiResponse<bool>.SuccessResponse(true, "Собеседование удалено успешно");
    }

    private static InterviewDto MapToDto(Interview interview) => new(
        interview.Id,
        interview.CandidateId,
        $"{interview.Candidate?.FirstName} {interview.Candidate?.LastName}",
        interview.ScheduledDate,
        interview.InterviewerName,
        interview.Notes,
        interview.Status.ToString(),
        interview.CreatedAt
    );
}
