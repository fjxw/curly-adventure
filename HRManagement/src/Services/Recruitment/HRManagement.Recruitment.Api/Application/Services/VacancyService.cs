using HRManagement.Recruitment.Api.Application.DTOs;
using HRManagement.Recruitment.Api.Domain.Entities;
using HRManagement.Recruitment.Api.Infrastructure.Data;
using HRManagement.Shared.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Recruitment.Api.Application.Services;

public interface IVacancyService
{
    Task<ApiResponse<VacancyDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<IEnumerable<VacancyDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<IEnumerable<VacancyDto>>> GetOpenAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<VacancyDto>> CreateAsync(CreateVacancyRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<VacancyDto>> UpdateAsync(Guid id, UpdateVacancyRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public class VacancyService : IVacancyService
{
    private readonly RecruitmentDbContext _context;

    public VacancyService(RecruitmentDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<VacancyDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var vacancy = await _context.Vacancies
            .Include(v => v.Candidates)
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

        if (vacancy == null)
            return ApiResponse<VacancyDto>.FailureResponse("Вакансия не найдена");

        return ApiResponse<VacancyDto>.SuccessResponse(MapToDto(vacancy));
    }

    public async Task<ApiResponse<IEnumerable<VacancyDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var vacancies = await _context.Vacancies
            .Include(v => v.Candidates)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync(cancellationToken);

        return ApiResponse<IEnumerable<VacancyDto>>.SuccessResponse(vacancies.Select(MapToDto));
    }

    public async Task<ApiResponse<IEnumerable<VacancyDto>>> GetOpenAsync(CancellationToken cancellationToken = default)
    {
        var vacancies = await _context.Vacancies
            .Include(v => v.Candidates)
            .Where(v => v.Status == VacancyStatus.Open)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync(cancellationToken);

        return ApiResponse<IEnumerable<VacancyDto>>.SuccessResponse(vacancies.Select(MapToDto));
    }

    public async Task<ApiResponse<VacancyDto>> CreateAsync(CreateVacancyRequest request, CancellationToken cancellationToken = default)
    {
        var vacancy = new Vacancy
        {
            Title = request.Title,
            Description = request.Description,
            Requirements = request.Requirements,
            DepartmentId = request.DepartmentId,
            DepartmentName = request.DepartmentName,
            PositionId = request.PositionId,
            PositionName = request.PositionName,
            SalaryFrom = request.SalaryFrom,
            SalaryTo = request.SalaryTo,
            Status = VacancyStatus.Open
        };

        _context.Vacancies.Add(vacancy);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<VacancyDto>.SuccessResponse(MapToDto(vacancy), "Вакансия создана");
    }

    public async Task<ApiResponse<VacancyDto>> UpdateAsync(Guid id, UpdateVacancyRequest request, CancellationToken cancellationToken = default)
    {
        var vacancy = await _context.Vacancies
            .Include(v => v.Candidates)
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

        if (vacancy == null)
            return ApiResponse<VacancyDto>.FailureResponse("Вакансия не найдена");

        vacancy.Title = request.Title;
        vacancy.Description = request.Description;
        vacancy.Requirements = request.Requirements;
        vacancy.SalaryFrom = request.SalaryFrom;
        vacancy.SalaryTo = request.SalaryTo;
        vacancy.Status = request.Status;

        if (request.Status == VacancyStatus.Closed || request.Status == VacancyStatus.Filled)
            vacancy.ClosedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<VacancyDto>.SuccessResponse(MapToDto(vacancy), "Вакансия обновлена");
    }

    public async Task<ApiResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var vacancy = await _context.Vacancies.FindAsync(new object[] { id }, cancellationToken);
        if (vacancy == null)
            return ApiResponse.FailureResponse("Вакансия не найдена");

        vacancy.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse.SuccessResponse("Вакансия удалена");
    }

    private static VacancyDto MapToDto(Vacancy v)
    {
        return new VacancyDto(
            v.Id,
            v.Title,
            v.Description,
            v.Requirements,
            v.DepartmentId,
            v.DepartmentName,
            v.PositionId,
            v.PositionName,
            v.SalaryFrom,
            v.SalaryTo,
            v.Status,
            v.Candidates?.Count ?? 0,
            v.CreatedAt);
    }
}
