using HRManagement.Employees.Api.Application.DTOs;
using HRManagement.Employees.Api.Domain.Entities;
using HRManagement.Employees.Api.Infrastructure.Data;
using HRManagement.Shared.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Employees.Api.Application.Services;

public interface ISkillService
{
    Task<ApiResponse<IEnumerable<SkillDto>>> GetAllSkillsAsync(CancellationToken ct = default);
    Task<ApiResponse<SkillDto>> CreateSkillAsync(CreateSkillRequest request, CancellationToken ct = default);
    Task<ApiResponse<IEnumerable<EmployeeSkillDto>>> GetEmployeeSkillsAsync(Guid employeeId, CancellationToken ct = default);
    Task<ApiResponse<EmployeeSkillDto>> AssignSkillAsync(AssignSkillRequest request, CancellationToken ct = default);
    Task<ApiResponse<EmployeeSkillDto>> UpdateEmployeeSkillAsync(Guid id, UpdateEmployeeSkillRequest request, CancellationToken ct = default);
    Task<ApiResponse> RemoveEmployeeSkillAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<IEnumerable<EmployeeSkillDto>>> GetEmployeesBySkillAsync(Guid skillId, CancellationToken ct = default);
}

public class SkillService : ISkillService
{
    private readonly EmployeesDbContext _context;

    public SkillService(EmployeesDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<IEnumerable<SkillDto>>> GetAllSkillsAsync(CancellationToken ct = default)
    {
        var skills = await _context.Skills
            .OrderBy(s => s.Category)
            .ThenBy(s => s.Name)
            .ToListAsync(ct);

        return ApiResponse<IEnumerable<SkillDto>>.SuccessResponse(skills.Select(s => new SkillDto(s.Id, s.Name, s.Description, s.Category)));
    }

    public async Task<ApiResponse<SkillDto>> CreateSkillAsync(CreateSkillRequest request, CancellationToken ct = default)
    {
        var existing = await _context.Skills.FirstOrDefaultAsync(s => s.Name == request.Name, ct);
        if (existing != null)
            return ApiResponse<SkillDto>.FailureResponse("Навык с таким названием уже существует");

        var skill = new Skill
        {
            Name = request.Name,
            Description = request.Description,
            Category = request.Category
        };

        _context.Skills.Add(skill);
        await _context.SaveChangesAsync(ct);

        return ApiResponse<SkillDto>.SuccessResponse(new SkillDto(skill.Id, skill.Name, skill.Description, skill.Category), "Навык создан");
    }

    public async Task<ApiResponse<IEnumerable<EmployeeSkillDto>>> GetEmployeeSkillsAsync(Guid employeeId, CancellationToken ct = default)
    {
        var skills = await _context.EmployeeSkills
            .Include(es => es.Employee)
            .Include(es => es.Skill)
            .Where(es => es.EmployeeId == employeeId)
            .ToListAsync(ct);

        return ApiResponse<IEnumerable<EmployeeSkillDto>>.SuccessResponse(skills.Select(MapToDto));
    }

    public async Task<ApiResponse<EmployeeSkillDto>> AssignSkillAsync(AssignSkillRequest request, CancellationToken ct = default)
    {
        var employee = await _context.Employees.FindAsync(new object[] { request.EmployeeId }, ct);
        if (employee == null)
            return ApiResponse<EmployeeSkillDto>.FailureResponse("Сотрудник не найден");

        var skill = await _context.Skills.FindAsync(new object[] { request.SkillId }, ct);
        if (skill == null)
            return ApiResponse<EmployeeSkillDto>.FailureResponse("Навык не найден");

        if (!Enum.TryParse<SkillLevel>(request.Level, true, out var level))
            return ApiResponse<EmployeeSkillDto>.FailureResponse("Неверный уровень навыка");

        var existing = await _context.EmployeeSkills
            .FirstOrDefaultAsync(es => es.EmployeeId == request.EmployeeId && es.SkillId == request.SkillId, ct);
        if (existing != null)
            return ApiResponse<EmployeeSkillDto>.FailureResponse("Навык уже назначен сотруднику");

        var employeeSkill = new EmployeeSkill
        {
            EmployeeId = request.EmployeeId,
            SkillId = request.SkillId,
            Level = level,
            YearsOfExperience = request.YearsOfExperience,
            CertifiedDate = request.CertifiedDate,
            CertificateNumber = request.CertificateNumber
        };

        _context.EmployeeSkills.Add(employeeSkill);
        await _context.SaveChangesAsync(ct);

        employeeSkill.Employee = employee;
        employeeSkill.Skill = skill;

        return ApiResponse<EmployeeSkillDto>.SuccessResponse(MapToDto(employeeSkill), "Навык назначен сотруднику");
    }

    public async Task<ApiResponse<EmployeeSkillDto>> UpdateEmployeeSkillAsync(Guid id, UpdateEmployeeSkillRequest request, CancellationToken ct = default)
    {
        var employeeSkill = await _context.EmployeeSkills
            .Include(es => es.Employee)
            .Include(es => es.Skill)
            .FirstOrDefaultAsync(es => es.Id == id, ct);

        if (employeeSkill == null)
            return ApiResponse<EmployeeSkillDto>.FailureResponse("Навык сотрудника не найден");

        if (!string.IsNullOrEmpty(request.Level) && Enum.TryParse<SkillLevel>(request.Level, true, out var level))
            employeeSkill.Level = level;

        if (request.YearsOfExperience.HasValue)
            employeeSkill.YearsOfExperience = request.YearsOfExperience;

        if (request.CertifiedDate.HasValue)
            employeeSkill.CertifiedDate = request.CertifiedDate;

        if (request.CertificateNumber != null)
            employeeSkill.CertificateNumber = request.CertificateNumber;

        employeeSkill.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        return ApiResponse<EmployeeSkillDto>.SuccessResponse(MapToDto(employeeSkill), "Навык обновлён");
    }

    public async Task<ApiResponse> RemoveEmployeeSkillAsync(Guid id, CancellationToken ct = default)
    {
        var employeeSkill = await _context.EmployeeSkills.FindAsync(new object[] { id }, ct);
        if (employeeSkill == null)
            return ApiResponse.FailureResponse("Навык сотрудника не найден");

        _context.EmployeeSkills.Remove(employeeSkill);
        await _context.SaveChangesAsync(ct);

        return ApiResponse.SuccessResponse("Навык удалён");
    }

    public async Task<ApiResponse<IEnumerable<EmployeeSkillDto>>> GetEmployeesBySkillAsync(Guid skillId, CancellationToken ct = default)
    {
        var skills = await _context.EmployeeSkills
            .Include(es => es.Employee)
            .Include(es => es.Skill)
            .Where(es => es.SkillId == skillId)
            .ToListAsync(ct);

        return ApiResponse<IEnumerable<EmployeeSkillDto>>.SuccessResponse(skills.Select(MapToDto));
    }

    private static EmployeeSkillDto MapToDto(EmployeeSkill es) => new(
        es.Id,
        es.EmployeeId,
        $"{es.Employee?.LastName} {es.Employee?.FirstName}",
        es.SkillId,
        es.Skill?.Name ?? string.Empty,
        es.Level.ToString(),
        es.YearsOfExperience,
        es.CertifiedDate,
        es.CertificateNumber);
}
