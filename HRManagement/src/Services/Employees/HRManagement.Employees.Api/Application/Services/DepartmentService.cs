using HRManagement.Employees.Api.Application.DTOs;
using HRManagement.Employees.Api.Domain.Entities;
using HRManagement.Employees.Api.Infrastructure.Repositories;
using HRManagement.Shared.Common.Caching;
using HRManagement.Shared.Common.Models;
using HRManagement.Shared.Contracts.DTOs;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Employees.Api.Application.Services;

public interface IDepartmentService
{
    Task<ApiResponse<DepartmentDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<IEnumerable<DepartmentDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<IEnumerable<DepartmentDto>>> GetTopLevelAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<DepartmentDto>> CreateAsync(CreateDepartmentRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<DepartmentDto>> UpdateAsync(Guid id, UpdateDepartmentRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ICacheService _cacheService;

    public DepartmentService(IDepartmentRepository departmentRepository, ICacheService cacheService)
    {
        _departmentRepository = departmentRepository;
        _cacheService = cacheService;
    }

    public async Task<ApiResponse<DepartmentDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"department_{id}";
        var cached = await _cacheService.GetAsync<DepartmentDto>(cacheKey);
        if (cached != null)
            return ApiResponse<DepartmentDto>.SuccessResponse(cached);

        var department = await _departmentRepository.GetByIdWithEmployeesAsync(id, cancellationToken);
        if (department == null)
            return ApiResponse<DepartmentDto>.FailureResponse("Отдел не найден");

        var dto = MapToDto(department);
        await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(10));

        return ApiResponse<DepartmentDto>.SuccessResponse(dto);
    }

    public async Task<ApiResponse<IEnumerable<DepartmentDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var departments = await _departmentRepository.Query()
            .Include(d => d.ParentDepartment)
            .Include(d => d.Employees)
            .ToListAsync(cancellationToken);

        var dtos = departments.Select(MapToDto);
        return ApiResponse<IEnumerable<DepartmentDto>>.SuccessResponse(dtos);
    }

    public async Task<ApiResponse<IEnumerable<DepartmentDto>>> GetTopLevelAsync(CancellationToken cancellationToken = default)
    {
        var departments = await _departmentRepository.GetTopLevelDepartmentsAsync(cancellationToken);
        var dtos = departments.Select(MapToDto);
        return ApiResponse<IEnumerable<DepartmentDto>>.SuccessResponse(dtos);
    }

    public async Task<ApiResponse<DepartmentDto>> CreateAsync(CreateDepartmentRequest request, CancellationToken cancellationToken = default)
    {
        if (request.ParentDepartmentId.HasValue)
        {
            var parent = await _departmentRepository.GetByIdAsync(request.ParentDepartmentId.Value, cancellationToken);
            if (parent == null)
                return ApiResponse<DepartmentDto>.FailureResponse("Родительский отдел не найден");
        }

        var department = new Department
        {
            Name = request.Name,
            Description = request.Description,
            ParentDepartmentId = request.ParentDepartmentId
        };

        await _departmentRepository.AddAsync(department, cancellationToken);

        return ApiResponse<DepartmentDto>.SuccessResponse(MapToDto(department), "Отдел успешно создан");
    }

    public async Task<ApiResponse<DepartmentDto>> UpdateAsync(Guid id, UpdateDepartmentRequest request, CancellationToken cancellationToken = default)
    {
        var department = await _departmentRepository.GetByIdAsync(id, cancellationToken);
        if (department == null)
            return ApiResponse<DepartmentDto>.FailureResponse("Отдел не найден");

        if (request.ParentDepartmentId.HasValue && request.ParentDepartmentId.Value == id)
            return ApiResponse<DepartmentDto>.FailureResponse("Отдел не может быть родителем самого себя");

        department.Name = request.Name;
        department.Description = request.Description;
        department.ParentDepartmentId = request.ParentDepartmentId;

        await _departmentRepository.UpdateAsync(department, cancellationToken);
        await _cacheService.RemoveAsync($"department_{id}");

        return ApiResponse<DepartmentDto>.SuccessResponse(MapToDto(department), "Отдел обновлен");
    }

    public async Task<ApiResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var department = await _departmentRepository.GetByIdWithEmployeesAsync(id, cancellationToken);
        if (department == null)
            return ApiResponse.FailureResponse("Отдел не найден");

        if (department.Employees.Any())
            return ApiResponse.FailureResponse("Невозможно удалить отдел с сотрудниками");

        if (department.ChildDepartments.Any())
            return ApiResponse.FailureResponse("Невозможно удалить отдел с дочерними отделами");

        await _departmentRepository.DeleteAsync(department, cancellationToken);
        await _cacheService.RemoveAsync($"department_{id}");

        return ApiResponse.SuccessResponse("Отдел удален");
    }

    private static DepartmentDto MapToDto(Department department)
    {
        return new DepartmentDto(
            department.Id,
            department.Name,
            department.Description ?? string.Empty,
            department.ParentDepartmentId,
            department.ParentDepartment?.Name,
            department.Employees?.Count(e => e.IsActive) ?? 0);
    }
}
