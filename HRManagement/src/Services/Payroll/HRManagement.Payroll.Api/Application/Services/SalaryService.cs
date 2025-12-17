using HRManagement.Payroll.Api.Application.DTOs;
using HRManagement.Payroll.Api.Domain.Entities;
using HRManagement.Payroll.Api.Infrastructure.Data;
using HRManagement.Shared.Common.Caching;
using HRManagement.Shared.Common.Models;
using HRManagement.Shared.Contracts.Events;
using HRManagement.Shared.MessageBus;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Payroll.Api.Application.Services;

public interface ISalaryService
{
    Task<ApiResponse<SalaryCalculationDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<IEnumerable<SalaryCalculationDto>>> GetByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default);
    Task<ApiResponse<IEnumerable<SalaryCalculationDto>>> GetByPeriodAsync(int month, int year, CancellationToken cancellationToken = default);
    Task<ApiResponse<SalaryCalculationDto>> CalculateAsync(CalculateSalaryRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse> ApproveAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse> MarkAsPaidAsync(Guid id, CancellationToken cancellationToken = default);
}

public class SalaryService : ISalaryService
{
    private readonly PayrollDbContext _context;
    private readonly IEventBus _eventBus;
    private readonly ICacheService _cacheService;

    private const decimal IncomeTaxRate = 0.13m;
    private const decimal SocialTaxRate = 0.30m;
    private const decimal PensionRate = 0.06m;

    public SalaryService(PayrollDbContext context, IEventBus eventBus, ICacheService cacheService)
    {
        _context = context;
        _eventBus = eventBus;
        _cacheService = cacheService;
    }

    public async Task<ApiResponse<SalaryCalculationDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var calculation = await _context.SalaryCalculations
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (calculation == null)
            return ApiResponse<SalaryCalculationDto>.FailureResponse("Расчёт не найден");

        return ApiResponse<SalaryCalculationDto>.SuccessResponse(MapToDto(calculation));
    }

    public async Task<ApiResponse<IEnumerable<SalaryCalculationDto>>> GetByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        var calculations = await _context.SalaryCalculations
            .Where(s => s.EmployeeId == employeeId)
            .OrderByDescending(s => s.Year)
            .ThenByDescending(s => s.Month)
            .ToListAsync(cancellationToken);

        return ApiResponse<IEnumerable<SalaryCalculationDto>>.SuccessResponse(calculations.Select(MapToDto));
    }

    public async Task<ApiResponse<IEnumerable<SalaryCalculationDto>>> GetByPeriodAsync(int month, int year, CancellationToken cancellationToken = default)
    {
        var calculations = await _context.SalaryCalculations
            .Where(s => s.Month == month && s.Year == year)
            .OrderBy(s => s.EmployeeName)
            .ToListAsync(cancellationToken);

        return ApiResponse<IEnumerable<SalaryCalculationDto>>.SuccessResponse(calculations.Select(MapToDto));
    }

    public async Task<ApiResponse<SalaryCalculationDto>> CalculateAsync(CalculateSalaryRequest request, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var existing = await _context.SalaryCalculations
                .FirstOrDefaultAsync(s => s.EmployeeId == request.EmployeeId 
                    && s.Month == request.Month 
                    && s.Year == request.Year, cancellationToken);

            if (existing != null && existing.Status == SalaryStatus.Paid)
                return ApiResponse<SalaryCalculationDto>.FailureResponse("Зарплата за этот период уже выплачена");

            var timeSheet = await _context.TimeSheets
                .FirstOrDefaultAsync(t => t.EmployeeId == request.EmployeeId 
                    && t.Month == request.Month 
                    && t.Year == request.Year, cancellationToken);

            var laborNorm = await _context.LaborNorms
                .FirstOrDefaultAsync(cancellationToken);

            decimal overtimePay = 0;
            decimal nightShiftPay = 0;
            decimal holidayPay = 0;
            decimal hourlyRate = request.BaseSalary / (laborNorm?.StandardHoursPerMonth ?? 160);

            if (timeSheet != null)
            {
                overtimePay = timeSheet.OvertimeHours * hourlyRate * (laborNorm?.OvertimeMultiplier ?? 1.5m);
                nightShiftPay = timeSheet.NightHours * hourlyRate * (laborNorm?.NightShiftMultiplier ?? 1.2m);
                holidayPay = timeSheet.HolidayHours * hourlyRate * (laborNorm?.HolidayMultiplier ?? 2.0m);
            }

            decimal grossSalary = request.BaseSalary + overtimePay + nightShiftPay + holidayPay + request.Bonuses + request.Allowances;

            decimal incomeTax = grossSalary * IncomeTaxRate;
            decimal socialTax = grossSalary * SocialTaxRate;
            decimal pensionContribution = grossSalary * PensionRate;
            decimal totalDeductions = incomeTax + pensionContribution;
            decimal netSalary = grossSalary - totalDeductions;

            SalaryCalculation calculation;

            if (existing != null)
            {
                calculation = existing;
                calculation.BaseSalary = request.BaseSalary;
                calculation.OvertimePay = overtimePay;
                calculation.NightShiftPay = nightShiftPay;
                calculation.HolidayPay = holidayPay;
                calculation.Bonuses = request.Bonuses;
                calculation.Allowances = request.Allowances;
                calculation.GrossSalary = grossSalary;
                calculation.IncomeTax = incomeTax;
                calculation.SocialTax = socialTax;
                calculation.PensionContribution = pensionContribution;
                calculation.TotalDeductions = totalDeductions;
                calculation.NetSalary = netSalary;
                calculation.Status = SalaryStatus.Calculated;
            }
            else
            {
                calculation = new SalaryCalculation
                {
                    EmployeeId = request.EmployeeId,
                    EmployeeName = request.EmployeeName,
                    Month = request.Month,
                    Year = request.Year,
                    BaseSalary = request.BaseSalary,
                    OvertimePay = overtimePay,
                    NightShiftPay = nightShiftPay,
                    HolidayPay = holidayPay,
                    Bonuses = request.Bonuses,
                    Allowances = request.Allowances,
                    GrossSalary = grossSalary,
                    IncomeTax = incomeTax,
                    SocialTax = socialTax,
                    PensionContribution = pensionContribution,
                    TotalDeductions = totalDeductions,
                    NetSalary = netSalary,
                    Status = SalaryStatus.Calculated
                };

                _context.SalaryCalculations.Add(calculation);
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return ApiResponse<SalaryCalculationDto>.SuccessResponse(MapToDto(calculation), "Зарплата рассчитана");
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<ApiResponse> ApproveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var calculation = await _context.SalaryCalculations.FindAsync(new object[] { id }, cancellationToken);
        if (calculation == null)
            return ApiResponse.FailureResponse("Расчёт не найден");

        if (calculation.Status == SalaryStatus.Paid)
            return ApiResponse.FailureResponse("Зарплата уже выплачена");

        calculation.Status = SalaryStatus.Approved;
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse.SuccessResponse("Расчёт утверждён");
    }

    public async Task<ApiResponse> MarkAsPaidAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var calculation = await _context.SalaryCalculations.FindAsync(new object[] { id }, cancellationToken);
        if (calculation == null)
            return ApiResponse.FailureResponse("Расчёт не найден");

        if (calculation.Status != SalaryStatus.Approved)
            return ApiResponse.FailureResponse("Расчёт должен быть утверждён перед выплатой");

        calculation.Status = SalaryStatus.Paid;
        calculation.PaidAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        await _eventBus.PublishAsync(new SalaryCalculatedEvent(
            calculation.EmployeeId,
            calculation.BaseSalary,
            calculation.Bonuses,
            calculation.TotalDeductions,
            calculation.NetSalary,
            calculation.Month,
            calculation.Year,
            DateTime.UtcNow), cancellationToken);

        return ApiResponse.SuccessResponse("Зарплата выплачена");
    }

    private static SalaryCalculationDto MapToDto(SalaryCalculation calc)
    {
        return new SalaryCalculationDto(
            calc.Id,
            calc.EmployeeId,
            calc.EmployeeName,
            calc.Month,
            calc.Year,
            calc.BaseSalary,
            calc.OvertimePay,
            calc.NightShiftPay,
            calc.HolidayPay,
            calc.Bonuses,
            calc.Allowances,
            calc.GrossSalary,
            calc.IncomeTax,
            calc.SocialTax,
            calc.PensionContribution,
            calc.OtherDeductions,
            calc.TotalDeductions,
            calc.NetSalary,
            calc.Status,
            calc.PaidAt);
    }
}
