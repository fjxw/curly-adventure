using HRManagement.Employees.Api.Application.DTOs;
using HRManagement.Employees.Api.Domain.Entities;
using HRManagement.Employees.Api.Infrastructure.Data;
using HRManagement.Employees.Api.Infrastructure.Repositories;
using HRManagement.Shared.Common.Caching;
using HRManagement.Shared.Common.Models;
using HRManagement.Shared.Contracts.DTOs;
using HRManagement.Shared.Contracts.Events;
using HRManagement.Shared.MessageBus;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Employees.Api.Application.Services;

public interface IEmployeeService
{
    Task<ApiResponse<EmployeeDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResult<EmployeeDto>>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<ApiResponse<IEnumerable<EmployeeDto>>> GetByDepartmentAsync(Guid departmentId, CancellationToken cancellationToken = default);
    Task<ApiResponse<EmployeeDto>> CreateAsync(CreateEmployeeRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<EmployeeDto>> UpdateAsync(Guid id, UpdateEmployeeRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse> TerminateAsync(Guid id, TerminateEmployeeRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IPositionRepository _positionRepository;
    private readonly IEventBus _eventBus;
    private readonly ICacheService _cacheService;
    private readonly EmployeesDbContext _context;

    public EmployeeService(
        IEmployeeRepository employeeRepository,
        IDepartmentRepository departmentRepository,
        IPositionRepository positionRepository,
        IEventBus eventBus,
        ICacheService cacheService,
        EmployeesDbContext context)
    {
        _employeeRepository = employeeRepository;
        _departmentRepository = departmentRepository;
        _positionRepository = positionRepository;
        _eventBus = eventBus;
        _cacheService = cacheService;
        _context = context;
    }

    public async Task<ApiResponse<EmployeeDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"employee_{id}";
        var cached = await _cacheService.GetAsync<EmployeeDto>(cacheKey);
        if (cached != null)
            return ApiResponse<EmployeeDto>.SuccessResponse(cached);

        var employee = await _employeeRepository.GetByIdWithDetailsAsync(id, cancellationToken);
        if (employee == null)
            return ApiResponse<EmployeeDto>.FailureResponse("Сотрудник не найден");

        var dto = MapToDto(employee);
        await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(5));

        return ApiResponse<EmployeeDto>.SuccessResponse(dto);
    }

    public async Task<ApiResponse<PagedResult<EmployeeDto>>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _employeeRepository.Query()
            .Include(e => e.Department)
            .Include(e => e.Position)
            .Where(e => e.IsActive);

        var totalCount = await query.CountAsync(cancellationToken);
        
        var employees = await query
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = employees.Select(MapToDto).ToList();
        var result = PagedResult<EmployeeDto>.Create(dtos, totalCount, pageNumber, pageSize);

        return ApiResponse<PagedResult<EmployeeDto>>.SuccessResponse(result);
    }

    public async Task<ApiResponse<IEnumerable<EmployeeDto>>> GetByDepartmentAsync(Guid departmentId, CancellationToken cancellationToken = default)
    {
        var employees = await _employeeRepository.GetByDepartmentAsync(departmentId, cancellationToken);
        var dtos = employees.Select(MapToDto);
        return ApiResponse<IEnumerable<EmployeeDto>>.SuccessResponse(dtos);
    }

    public async Task<ApiResponse<EmployeeDto>> CreateAsync(CreateEmployeeRequest request, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var department = await _departmentRepository.GetByIdAsync(request.DepartmentId, cancellationToken);
            if (department == null)
                return ApiResponse<EmployeeDto>.FailureResponse("Отдел не найден");

            var position = await _positionRepository.GetByIdAsync(request.PositionId, cancellationToken);
            if (position == null)
                return ApiResponse<EmployeeDto>.FailureResponse("Должность не найдена");

            var existingEmployee = await _employeeRepository.Query()
                .FirstOrDefaultAsync(e => e.Email == request.Email, cancellationToken);
            if (existingEmployee != null)
                return ApiResponse<EmployeeDto>.FailureResponse("Сотрудник с таким email уже существует");

            var employee = new Employee
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                MiddleName = request.MiddleName,
                Email = request.Email,
                Phone = request.Phone,
                DateOfBirth = request.DateOfBirth,
                Address = request.Address,
                PassportNumber = request.PassportNumber,
                TaxId = request.TaxId,
                DepartmentId = request.DepartmentId,
                PositionId = request.PositionId,
                HireDate = request.HireDate,
                IsActive = true
            };

            await _employeeRepository.AddAsync(employee, cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            // Publish event
            await _eventBus.PublishAsync(new EmployeeCreatedEvent(
                employee.Id,
                employee.FirstName,
                employee.LastName,
                employee.Email,
                employee.DepartmentId,
                employee.PositionId,
                employee.HireDate,
                DateTime.UtcNow), cancellationToken);

            employee.Department = department;
            employee.Position = position;

            return ApiResponse<EmployeeDto>.SuccessResponse(MapToDto(employee), "Сотрудник успешно создан");
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<ApiResponse<EmployeeDto>> UpdateAsync(Guid id, UpdateEmployeeRequest request, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var employee = await _employeeRepository.GetByIdWithDetailsAsync(id, cancellationToken);
            if (employee == null)
                return ApiResponse<EmployeeDto>.FailureResponse("Сотрудник не найден");

            var department = await _departmentRepository.GetByIdAsync(request.DepartmentId, cancellationToken);
            if (department == null)
                return ApiResponse<EmployeeDto>.FailureResponse("Отдел не найден");

            var position = await _positionRepository.GetByIdAsync(request.PositionId, cancellationToken);
            if (position == null)
                return ApiResponse<EmployeeDto>.FailureResponse("Должность не найдена");

            employee.FirstName = request.FirstName;
            employee.LastName = request.LastName;
            employee.MiddleName = request.MiddleName;
            employee.Email = request.Email;
            employee.Phone = request.Phone;
            employee.DateOfBirth = request.DateOfBirth;
            employee.Address = request.Address;
            employee.DepartmentId = request.DepartmentId;
            employee.PositionId = request.PositionId;

            await _employeeRepository.UpdateAsync(employee, cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            await _cacheService.RemoveAsync($"employee_{id}");

            employee.Department = department;
            employee.Position = position;

            return ApiResponse<EmployeeDto>.SuccessResponse(MapToDto(employee), "Данные сотрудника обновлены");
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<ApiResponse> TerminateAsync(Guid id, TerminateEmployeeRequest request, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByIdAsync(id, cancellationToken);
        if (employee == null)
            return ApiResponse.FailureResponse("Сотрудник не найден");

        employee.IsActive = false;
        employee.TerminationDate = request.TerminationDate;

        await _employeeRepository.UpdateAsync(employee, cancellationToken);

        await _eventBus.PublishAsync(new EmployeeTerminatedEvent(
            employee.Id,
            request.TerminationDate,
            request.Reason,
            DateTime.UtcNow), cancellationToken);

        await _cacheService.RemoveAsync($"employee_{id}");

        return ApiResponse.SuccessResponse("Сотрудник уволен");
    }

    public async Task<ApiResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByIdAsync(id, cancellationToken);
        if (employee == null)
            return ApiResponse.FailureResponse("Сотрудник не найден");

        await _employeeRepository.DeleteAsync(employee, cancellationToken);
        await _cacheService.RemoveAsync($"employee_{id}");

        return ApiResponse.SuccessResponse("Сотрудник удален");
    }

    private static EmployeeDto MapToDto(Employee employee)
    {
        return new EmployeeDto(
            employee.Id,
            employee.FirstName,
            employee.LastName,
            employee.MiddleName ?? string.Empty,
            employee.Email,
            employee.Phone ?? string.Empty,
            employee.DateOfBirth,
            employee.Address ?? string.Empty,
            employee.DepartmentId,
            employee.Department?.Name ?? string.Empty,
            employee.PositionId,
            employee.Position?.Name ?? string.Empty,
            employee.HireDate,
            employee.TerminationDate,
            employee.IsActive);
    }
}
