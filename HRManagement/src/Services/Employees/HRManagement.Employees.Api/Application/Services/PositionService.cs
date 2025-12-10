using HRManagement.Employees.Api.Application.DTOs;
using HRManagement.Employees.Api.Domain.Entities;
using HRManagement.Employees.Api.Infrastructure.Repositories;
using HRManagement.Shared.Common.Caching;
using HRManagement.Shared.Common.Models;
using HRManagement.Shared.Contracts.DTOs;

namespace HRManagement.Employees.Api.Application.Services;

public interface IPositionService
{
    Task<ApiResponse<PositionDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<IEnumerable<PositionDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<IEnumerable<PositionDto>>> GetByDepartmentAsync(Guid departmentId, CancellationToken cancellationToken = default);
    Task<ApiResponse<PositionDto>> CreateAsync(CreatePositionRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<PositionDto>> UpdateAsync(Guid id, UpdatePositionRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse> AddResponsibilityAsync(Guid positionId, CreateJobResponsibilityRequest request, CancellationToken cancellationToken = default);
}

public class PositionService : IPositionService
{
    private readonly IPositionRepository _positionRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IRepository<JobResponsibility> _responsibilityRepository;
    private readonly ICacheService _cacheService;

    public PositionService(
        IPositionRepository positionRepository,
        IDepartmentRepository departmentRepository,
        IRepository<JobResponsibility> responsibilityRepository,
        ICacheService cacheService)
    {
        _positionRepository = positionRepository;
        _departmentRepository = departmentRepository;
        _responsibilityRepository = responsibilityRepository;
        _cacheService = cacheService;
    }

    public async Task<ApiResponse<PositionDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"position_{id}";
        var cached = await _cacheService.GetAsync<PositionDto>(cacheKey);
        if (cached != null)
            return ApiResponse<PositionDto>.SuccessResponse(cached);

        var position = await _positionRepository.GetByIdWithDetailsAsync(id, cancellationToken);
        if (position == null)
            return ApiResponse<PositionDto>.FailureResponse("Должность не найдена");

        var dto = MapToDto(position);
        await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(10));

        return ApiResponse<PositionDto>.SuccessResponse(dto);
    }

    public async Task<ApiResponse<IEnumerable<PositionDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var positions = await _positionRepository.GetAllAsync(cancellationToken);
        var dtos = positions.Select(MapToDto);
        return ApiResponse<IEnumerable<PositionDto>>.SuccessResponse(dtos);
    }

    public async Task<ApiResponse<IEnumerable<PositionDto>>> GetByDepartmentAsync(Guid departmentId, CancellationToken cancellationToken = default)
    {
        var positions = await _positionRepository.GetByDepartmentAsync(departmentId, cancellationToken);
        var dtos = positions.Select(MapToDto);
        return ApiResponse<IEnumerable<PositionDto>>.SuccessResponse(dtos);
    }

    public async Task<ApiResponse<PositionDto>> CreateAsync(CreatePositionRequest request, CancellationToken cancellationToken = default)
    {
        var department = await _departmentRepository.GetByIdAsync(request.DepartmentId, cancellationToken);
        if (department == null)
            return ApiResponse<PositionDto>.FailureResponse("Отдел не найден");

        if (request.MinSalary > request.MaxSalary)
            return ApiResponse<PositionDto>.FailureResponse("Минимальная зарплата не может быть больше максимальной");

        var position = new Position
        {
            Name = request.Name,
            Description = request.Description,
            MinSalary = request.MinSalary,
            MaxSalary = request.MaxSalary,
            DepartmentId = request.DepartmentId
        };

        await _positionRepository.AddAsync(position, cancellationToken);
        position.Department = department;

        return ApiResponse<PositionDto>.SuccessResponse(MapToDto(position), "Должность успешно создана");
    }

    public async Task<ApiResponse<PositionDto>> UpdateAsync(Guid id, UpdatePositionRequest request, CancellationToken cancellationToken = default)
    {
        var position = await _positionRepository.GetByIdWithDetailsAsync(id, cancellationToken);
        if (position == null)
            return ApiResponse<PositionDto>.FailureResponse("Должность не найдена");

        var department = await _departmentRepository.GetByIdAsync(request.DepartmentId, cancellationToken);
        if (department == null)
            return ApiResponse<PositionDto>.FailureResponse("Отдел не найден");

        if (request.MinSalary > request.MaxSalary)
            return ApiResponse<PositionDto>.FailureResponse("Минимальная зарплата не может быть больше максимальной");

        position.Name = request.Name;
        position.Description = request.Description;
        position.MinSalary = request.MinSalary;
        position.MaxSalary = request.MaxSalary;
        position.DepartmentId = request.DepartmentId;

        await _positionRepository.UpdateAsync(position, cancellationToken);
        await _cacheService.RemoveAsync($"position_{id}");

        position.Department = department;

        return ApiResponse<PositionDto>.SuccessResponse(MapToDto(position), "Должность обновлена");
    }

    public async Task<ApiResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var position = await _positionRepository.GetByIdWithDetailsAsync(id, cancellationToken);
        if (position == null)
            return ApiResponse.FailureResponse("Должность не найдена");

        if (position.Employees.Any())
            return ApiResponse.FailureResponse("Невозможно удалить должность с сотрудниками");

        await _positionRepository.DeleteAsync(position, cancellationToken);
        await _cacheService.RemoveAsync($"position_{id}");

        return ApiResponse.SuccessResponse("Должность удалена");
    }

    public async Task<ApiResponse> AddResponsibilityAsync(Guid positionId, CreateJobResponsibilityRequest request, CancellationToken cancellationToken = default)
    {
        var position = await _positionRepository.GetByIdAsync(positionId, cancellationToken);
        if (position == null)
            return ApiResponse.FailureResponse("Должность не найдена");

        var responsibility = new JobResponsibility
        {
            Description = request.Description,
            Priority = request.Priority,
            PositionId = positionId
        };

        await _responsibilityRepository.AddAsync(responsibility, cancellationToken);

        return ApiResponse.SuccessResponse("Должностная обязанность добавлена");
    }

    private static PositionDto MapToDto(Position position)
    {
        return new PositionDto(
            position.Id,
            position.Name,
            position.Description ?? string.Empty,
            position.MinSalary,
            position.MaxSalary,
            position.DepartmentId,
            position.Department?.Name ?? string.Empty);
    }
}
