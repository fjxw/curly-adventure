using HRManagement.Employees.Api.Application.DTOs;
using HRManagement.Employees.Api.Application.Services;
using HRManagement.Shared.Contracts.Events;
using HRManagement.Shared.MessageBus;
using Microsoft.Extensions.Logging;

namespace HRManagement.Employees.Api.Application.EventHandlers;

public class CandidateHiredEventHandler : IEventHandler<CandidateHiredEvent>
{
    private readonly IEmployeeService _employeeService;
    private readonly ILogger<CandidateHiredEventHandler> _logger;

    public CandidateHiredEventHandler(
        IEmployeeService employeeService,
        ILogger<CandidateHiredEventHandler> logger)
    {
        _employeeService = employeeService;
        _logger = logger;
    }

    public async Task HandleAsync(CandidateHiredEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Обработка события CandidateHiredEvent для кандидата {CandidateId}: {FirstName} {LastName}",
            @event.CandidateId, @event.FirstName, @event.LastName);

        try
        {
            var request = new CreateEmployeeRequest(
                FirstName: @event.FirstName,
                LastName: @event.LastName,
                MiddleName: null,
                Email: @event.Email,
                Phone: null,
                DateOfBirth: DateTime.UtcNow.AddYears(-25),
                Address: null,
                PassportNumber: null,
                TaxId: null,
                DepartmentId: @event.DepartmentId,
                PositionId: @event.PositionId,
                HireDate: @event.HireDate);

            var result = await _employeeService.CreateAsync(request, cancellationToken);

            if (result.Success)
            {
                _logger.LogInformation(
                    "Успешно создан сотрудник {EmployeeId} из кандидата {CandidateId}",
                    result.Data?.Id, @event.CandidateId);
            }
            else
            {
                _logger.LogWarning(
                    "Не удалось создать сотрудника из кандидата {CandidateId}: {Message}",
                    @event.CandidateId, result.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Ошибка создания сотрудника из кандидата {CandidateId}",
                @event.CandidateId);
            throw;
        }
    }
}
