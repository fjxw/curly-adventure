using HRManagement.Shared.Common.Models;

namespace HRManagement.Payroll.Api.Domain.Entities;

public class SalaryCalculation : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    
    public int Month { get; set; }
    public int Year { get; set; }
    
    public decimal BaseSalary { get; set; }
    public decimal OvertimePay { get; set; }
    public decimal NightShiftPay { get; set; }
    public decimal HolidayPay { get; set; }
    public decimal Bonuses { get; set; }
    public decimal Allowances { get; set; }
    
    public decimal GrossSalary { get; set; }
    
    public decimal IncomeTax { get; set; }
    public decimal SocialTax { get; set; }
    public decimal PensionContribution { get; set; }
    public decimal OtherDeductions { get; set; }
    
    public decimal TotalDeductions { get; set; }
    public decimal NetSalary { get; set; }
    
    public SalaryStatus Status { get; set; } = SalaryStatus.Draft;
    public DateTime? PaidAt { get; set; }
}

public enum SalaryStatus
{
    Draft,
    Calculated,
    Approved,
    Paid
}
