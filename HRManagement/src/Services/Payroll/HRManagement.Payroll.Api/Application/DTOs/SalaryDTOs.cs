using HRManagement.Payroll.Api.Domain.Entities;

namespace HRManagement.Payroll.Api.Application.DTOs;

public record CalculateSalaryRequest(
    Guid EmployeeId,
    string EmployeeName,
    int Month,
    int Year,
    decimal BaseSalary,
    decimal Bonuses,
    decimal Allowances);

public record SalaryCalculationDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    int Month,
    int Year,
    decimal BaseSalary,
    decimal OvertimePay,
    decimal NightShiftPay,
    decimal HolidayPay,
    decimal Bonuses,
    decimal Allowances,
    decimal GrossSalary,
    decimal IncomeTax,
    decimal SocialTax,
    decimal PensionContribution,
    decimal OtherDeductions,
    decimal TotalDeductions,
    decimal NetSalary,
    SalaryStatus Status,
    DateTime? PaidAt);

public record CreateLaborNormRequest(
    string Name,
    string Description,
    Guid PositionId,
    string PositionName,
    decimal StandardHoursPerDay,
    decimal StandardHoursPerWeek,
    decimal StandardHoursPerMonth,
    decimal OvertimeMultiplier,
    decimal NightShiftMultiplier,
    decimal HolidayMultiplier);

public record LaborNormDto(
    Guid Id,
    string Name,
    string Description,
    Guid PositionId,
    string PositionName,
    decimal StandardHoursPerDay,
    decimal StandardHoursPerWeek,
    decimal StandardHoursPerMonth,
    decimal OvertimeMultiplier,
    decimal NightShiftMultiplier,
    decimal HolidayMultiplier);
