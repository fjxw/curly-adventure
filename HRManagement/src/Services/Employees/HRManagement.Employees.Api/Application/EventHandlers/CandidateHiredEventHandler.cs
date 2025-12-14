using HRManagement.Employees.Api.Application.DTOs;
using HRManagement.Employees.Api.Application.Services;
using HRManagement.Shared.Contracts.Events;
using HRManagement.Shared.MessageBus;
using Microsoft.Extensions.Logging;

namespace HRManagement.Employees.Api.Application.EventHandlers;

/// <summary>
/// Handles CandidateHiredEvent from Recruitment service.
/// Automatically creates a new employee when a candidate is hired.
/// </summary>
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
            "Processing CandidateHiredEvent for candidate {CandidateId}: {FirstName} {LastName}",
            @event.CandidateId, @event.FirstName, @event.LastName);

        try
        {
            var request = new CreateEmployeeRequest(
                FirstName: @event.FirstName,
                LastName: @event.LastName,
                MiddleName: null,
                Email: @event.Email,
                Phone: null,
                DateOfBirth: DateTime.UtcNow.AddYears(-25), // Default, should be updated later
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
                    "Successfully created employee {EmployeeId} from candidate {CandidateId}",
                    result.Data?.Id, @event.CandidateId);
            }
            else
            {
                _logger.LogWarning(
                    "Failed to create employee from candidate {CandidateId}: {Message}",
                    @event.CandidateId, result.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating employee from candidate {CandidateId}",
                @event.CandidateId);
            throw;
        }
    }
}
