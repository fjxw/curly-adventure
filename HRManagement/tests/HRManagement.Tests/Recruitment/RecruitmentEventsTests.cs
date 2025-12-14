using FluentAssertions;
using HRManagement.Shared.Contracts.Events;

namespace HRManagement.Tests.Recruitment;

public class RecruitmentEventsTests
{
    [Fact]
    public void CandidateHiredEvent_ShouldHaveAllProperties()
    {
        // Arrange
        var candidateId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();
        var positionId = Guid.NewGuid();
        var hireDate = DateTime.UtcNow;
        var salary = 100000m;

        // Act
        var evt = new CandidateHiredEvent(
            candidateId,
            "Иван",
            "Петров",
            "ivan@company.com",
            departmentId,
            positionId,
            salary,
            hireDate,
            DateTime.UtcNow
        );

        // Assert
        evt.CandidateId.Should().Be(candidateId);
        evt.FirstName.Should().Be("Иван");
        evt.LastName.Should().Be("Петров");
        evt.Email.Should().Be("ivan@company.com");
        evt.DepartmentId.Should().Be(departmentId);
        evt.PositionId.Should().Be(positionId);
        evt.Salary.Should().Be(100000m);
        evt.HireDate.Should().Be(hireDate);
    }

    [Fact]
    public void CandidateHiredEvent_WithDifferentCandidates_ShouldNotBeEqual()
    {
        // Arrange
        var hireDate = DateTime.UtcNow;
        
        var event1 = new CandidateHiredEvent(
            Guid.NewGuid(),
            "Иван",
            "Петров",
            "ivan@company.com",
            Guid.NewGuid(),
            Guid.NewGuid(),
            80000m,
            hireDate,
            DateTime.UtcNow
        );

        var event2 = new CandidateHiredEvent(
            Guid.NewGuid(),
            "Петр",
            "Иванов",
            "peter@company.com",
            Guid.NewGuid(),
            Guid.NewGuid(),
            90000m,
            hireDate,
            DateTime.UtcNow
        );

        // Assert
        event1.CandidateId.Should().NotBe(event2.CandidateId);
        event1.FirstName.Should().NotBe(event2.FirstName);
        event1.Salary.Should().NotBe(event2.Salary);
    }

    [Fact]
    public void CandidateHiredEvent_RecordEquality_ShouldWork()
    {
        // Arrange
        var candidateId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();
        var positionId = Guid.NewGuid();
        var hireDate = new DateTime(2024, 1, 15);
        var createdAt = new DateTime(2024, 1, 15);

        var event1 = new CandidateHiredEvent(
            candidateId, "Иван", "Петров", "ivan@company.com",
            departmentId, positionId, 100000m, hireDate, createdAt
        );

        var event2 = new CandidateHiredEvent(
            candidateId, "Иван", "Петров", "ivan@company.com",
            departmentId, positionId, 100000m, hireDate, createdAt
        );

        // Assert - records with same values should be equal
        event1.Should().Be(event2);
    }
}
