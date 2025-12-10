using FluentAssertions;
using HRManagement.Shared.Contracts.Events;

namespace HRManagement.Tests.Payroll;

public class PayrollEventsTests
{
    [Fact]
    public void SalaryCalculatedEvent_ShouldHaveAllProperties()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var baseSalary = 100000m;
        var bonuses = 10000m;
        var deductions = 14300m; // ~13% tax
        var netSalary = baseSalary + bonuses - deductions;

        // Act
        var evt = new SalaryCalculatedEvent(
            employeeId,
            baseSalary,
            bonuses,
            deductions,
            netSalary,
            12,
            2024,
            DateTime.UtcNow
        );

        // Assert
        evt.EmployeeId.Should().Be(employeeId);
        evt.BaseSalary.Should().Be(100000m);
        evt.Bonuses.Should().Be(10000m);
        evt.Deductions.Should().Be(14300m);
        evt.NetSalary.Should().Be(95700m);
        evt.Month.Should().Be(12);
        evt.Year.Should().Be(2024);
    }

    [Fact]
    public void SalaryCalculatedEvent_WithZeroBonuses_ShouldCalculateCorrectly()
    {
        // Arrange
        var baseSalary = 80000m;
        var deductions = 10400m;

        // Act
        var evt = new SalaryCalculatedEvent(
            Guid.NewGuid(),
            baseSalary,
            0m,             // No bonuses
            deductions,
            baseSalary - deductions,
            1,
            2025,
            DateTime.UtcNow
        );

        // Assert
        evt.Bonuses.Should().Be(0);
        evt.NetSalary.Should().Be(69600m);
    }

    [Fact]
    public void SalaryCalculatedEvent_DifferentMonths_ShouldHaveDifferentValues()
    {
        // Arrange
        var employeeId = Guid.NewGuid();

        var januaryPayroll = new SalaryCalculatedEvent(
            employeeId,
            80000m,
            5000m,
            11050m,
            73950m,
            1,
            2025,
            DateTime.UtcNow
        );

        var februaryPayroll = new SalaryCalculatedEvent(
            employeeId,
            85000m,
            7000m,
            11960m,
            80040m,
            2,
            2025,
            DateTime.UtcNow
        );

        // Assert
        januaryPayroll.Month.Should().Be(1);
        februaryPayroll.Month.Should().Be(2);
        januaryPayroll.BaseSalary.Should().BeLessThan(februaryPayroll.BaseSalary);
        januaryPayroll.EmployeeId.Should().Be(februaryPayroll.EmployeeId);
    }

    [Fact]
    public void SalaryCalculatedEvent_RecordEquality_ShouldWork()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var createdAt = new DateTime(2024, 12, 25);

        var event1 = new SalaryCalculatedEvent(
            employeeId, 100000m, 10000m, 14300m, 95700m, 12, 2024, createdAt
        );

        var event2 = new SalaryCalculatedEvent(
            employeeId, 100000m, 10000m, 14300m, 95700m, 12, 2024, createdAt
        );

        // Assert
        event1.Should().Be(event2);
    }
}
