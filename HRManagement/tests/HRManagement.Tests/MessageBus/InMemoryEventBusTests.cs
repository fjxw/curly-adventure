using FluentAssertions;
using HRManagement.Shared.MessageBus;
using HRManagement.Shared.Contracts.Events;

namespace HRManagement.Tests.MessageBus;

public class InMemoryEventBusTests
{
    [Fact]
    public async Task PublishAsync_ShouldCallSubscribedHandler()
    {
        // Arrange
        var eventBus = new InMemoryEventBus();
        EmployeeCreatedEvent? receivedEvent = null;
        
        eventBus.Subscribe<EmployeeCreatedEvent>(async (evt) =>
        {
            receivedEvent = evt;
            await Task.CompletedTask;
        });

        var testEvent = new EmployeeCreatedEvent(
            Guid.NewGuid(),    // EmployeeId
            "Иван",            // FirstName
            "Петров",          // LastName
            "ivan@company.com", // Email
            Guid.NewGuid(),    // DepartmentId
            Guid.NewGuid(),    // PositionId
            DateTime.UtcNow,   // HireDate
            DateTime.UtcNow    // CreatedAt
        );

        // Act
        await eventBus.PublishAsync(testEvent);

        // Assert
        receivedEvent.Should().NotBeNull();
        receivedEvent!.FirstName.Should().Be("Иван");
        receivedEvent.LastName.Should().Be("Петров");
        receivedEvent.Email.Should().Be("ivan@company.com");
    }

    [Fact]
    public async Task PublishAsync_WithNoSubscribers_ShouldNotThrow()
    {
        // Arrange
        var eventBus = new InMemoryEventBus();
        var testEvent = new EmployeeCreatedEvent(
            Guid.NewGuid(),
            "Test",
            "User",
            "test@test.com",
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow,
            DateTime.UtcNow
        );

        // Act & Assert
        var act = async () => await eventBus.PublishAsync(testEvent);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Subscribe_MultipleHandlers_AllShouldBeCalled()
    {
        // Arrange
        var eventBus = new InMemoryEventBus();
        var callCount = 0;

        eventBus.Subscribe<CandidateHiredEvent>(async (_) =>
        {
            Interlocked.Increment(ref callCount);
            await Task.CompletedTask;
        });

        eventBus.Subscribe<CandidateHiredEvent>(async (_) =>
        {
            Interlocked.Increment(ref callCount);
            await Task.CompletedTask;
        });

        var testEvent = new CandidateHiredEvent(
            Guid.NewGuid(),    // CandidateId
            "Алексей",         // FirstName
            "Сидоров",         // LastName
            "alex@company.com", // Email
            Guid.NewGuid(),    // PositionId
            75000m,            // Salary
            DateTime.UtcNow,   // HireDate
            DateTime.UtcNow    // CreatedAt
        );

        // Act
        await eventBus.PublishAsync(testEvent);

        // Assert
        callCount.Should().Be(2);
    }

    [Fact]
    public async Task PublishAsync_SalaryCalculatedEvent_ShouldDeliverCorrectData()
    {
        // Arrange
        var eventBus = new InMemoryEventBus();
        SalaryCalculatedEvent? receivedEvent = null;
        
        eventBus.Subscribe<SalaryCalculatedEvent>(async (evt) =>
        {
            receivedEvent = evt;
            await Task.CompletedTask;
        });

        var employeeId = Guid.NewGuid();
        var testEvent = new SalaryCalculatedEvent(
            employeeId,        // EmployeeId
            100000m,           // BaseSalary
            10000m,            // Bonuses
            13000m,            // Deductions
            97000m,            // NetSalary
            12,                // Month
            2024,              // Year
            DateTime.UtcNow    // CreatedAt
        );

        // Act
        await eventBus.PublishAsync(testEvent);

        // Assert
        receivedEvent.Should().NotBeNull();
        receivedEvent!.EmployeeId.Should().Be(employeeId);
        receivedEvent.BaseSalary.Should().Be(100000m);
        receivedEvent.NetSalary.Should().Be(97000m);
    }

    [Fact]
    public async Task PublishAsync_DifferentEventTypes_ShouldRouteCorrectly()
    {
        // Arrange
        var eventBus = new InMemoryEventBus();
        var employeeEventReceived = false;
        var salaryEventReceived = false;

        eventBus.Subscribe<EmployeeCreatedEvent>(async (_) =>
        {
            employeeEventReceived = true;
            await Task.CompletedTask;
        });

        eventBus.Subscribe<SalaryCalculatedEvent>(async (_) =>
        {
            salaryEventReceived = true;
            await Task.CompletedTask;
        });

        var employeeEvent = new EmployeeCreatedEvent(
            Guid.NewGuid(), "Test", "User", "test@test.com",
            Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, DateTime.UtcNow
        );

        // Act - only publish employee event
        await eventBus.PublishAsync(employeeEvent);

        // Assert
        employeeEventReceived.Should().BeTrue();
        salaryEventReceived.Should().BeFalse();
    }
}
