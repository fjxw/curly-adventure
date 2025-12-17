using FluentAssertions;
using HRManagement.Shared.MessageBus;
using HRManagement.Shared.Contracts.Events;

namespace HRManagement.Tests.MessageBus;

public class InMemoryEventBusTests
{
    [Fact]
    public async Task PublishAsync_ShouldCallSubscribedHandler()
    {
        var eventBus = new InMemoryEventBus();
        EmployeeCreatedEvent? receivedEvent = null;
        
        eventBus.Subscribe<EmployeeCreatedEvent>(async (evt) =>
        {
            receivedEvent = evt;
            await Task.CompletedTask;
        });

        var testEvent = new EmployeeCreatedEvent(
            Guid.NewGuid(),
            "Иван",
            "Петров",
            "ivan@company.com",
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow,
            DateTime.UtcNow
        );

        await eventBus.PublishAsync(testEvent);

        receivedEvent.Should().NotBeNull();
        receivedEvent!.FirstName.Should().Be("Иван");
        receivedEvent.LastName.Should().Be("Петров");
        receivedEvent.Email.Should().Be("ivan@company.com");
    }

    [Fact]
    public async Task PublishAsync_WithNoSubscribers_ShouldNotThrow()
    {
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

        var act = async () => await eventBus.PublishAsync(testEvent);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Subscribe_MultipleHandlers_AllShouldBeCalled()
    {
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
            Guid.NewGuid(),
            "Алексей",
            "Сидоров",
            "alex@company.com",
            Guid.NewGuid(),
            Guid.NewGuid(),
            75000m,
            DateTime.UtcNow,
            DateTime.UtcNow
        );

        await eventBus.PublishAsync(testEvent);

        callCount.Should().Be(2);
    }

    [Fact]
    public async Task PublishAsync_SalaryCalculatedEvent_ShouldDeliverCorrectData()
    {
        var eventBus = new InMemoryEventBus();
        SalaryCalculatedEvent? receivedEvent = null;
        
        eventBus.Subscribe<SalaryCalculatedEvent>(async (evt) =>
        {
            receivedEvent = evt;
            await Task.CompletedTask;
        });

        var employeeId = Guid.NewGuid();
        var testEvent = new SalaryCalculatedEvent(
            employeeId,
            100000m,
            10000m,
            13000m,
            97000m,
            12,
            2024,
            DateTime.UtcNow
        );

        await eventBus.PublishAsync(testEvent);

        receivedEvent.Should().NotBeNull();
        receivedEvent!.EmployeeId.Should().Be(employeeId);
        receivedEvent.BaseSalary.Should().Be(100000m);
        receivedEvent.NetSalary.Should().Be(97000m);
    }

    [Fact]
    public async Task PublishAsync_DifferentEventTypes_ShouldRouteCorrectly()
    {
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

        await eventBus.PublishAsync(employeeEvent);

        employeeEventReceived.Should().BeTrue();
        salaryEventReceived.Should().BeFalse();
    }
}
