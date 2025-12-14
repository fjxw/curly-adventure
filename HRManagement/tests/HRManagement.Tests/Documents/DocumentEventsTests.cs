using HRManagement.Shared.Contracts.Events;

namespace HRManagement.Tests.Documents;

public class DocumentEventsTests
{
    [Fact]
    public void DocumentCreatedEvent_ShouldHaveCorrectProperties()
    {
        var documentId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var documentType = "HiringOrder";
        var documentNumber = "ПР-Н-20241201-ABC12345";
        var createdAt = DateTime.UtcNow;

        var documentEvent = new DocumentCreatedEvent(documentId, employeeId, documentType, documentNumber, createdAt);

        Assert.Equal(documentId, documentEvent.DocumentId);
        Assert.Equal(employeeId, documentEvent.EmployeeId);
        Assert.Equal(documentType, documentEvent.DocumentType);
        Assert.Equal(documentNumber, documentEvent.DocumentNumber);
        Assert.Equal(createdAt, documentEvent.CreatedAt);
    }

    [Fact]
    public void DocumentSignedEvent_ShouldHaveCorrectProperties()
    {
        var documentId = Guid.NewGuid();
        var signedById = Guid.NewGuid();
        var signedAt = DateTime.UtcNow;

        var signedEvent = new DocumentSignedEvent(documentId, signedById, signedAt);

        Assert.Equal(documentId, signedEvent.DocumentId);
        Assert.Equal(signedById, signedEvent.SignedById);
        Assert.Equal(signedAt, signedEvent.SignedAt);
    }

    [Theory]
    [InlineData("HiringOrder")]
    [InlineData("TerminationOrder")]
    [InlineData("VacationOrder")]
    [InlineData("EmploymentContract")]
    [InlineData("Certificate")]
    [InlineData("Application")]
    public void DocumentCreatedEvent_ShouldSupportVariousTypes(string documentType)
    {
        var documentEvent = new DocumentCreatedEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            documentType,
            $"DOC-{DateTime.Now.Ticks}",
            DateTime.UtcNow);

        Assert.Equal(documentType, documentEvent.DocumentType);
    }

    [Fact]
    public void DocumentEvents_ShouldBeRecords()
    {
        var doc1 = new DocumentCreatedEvent(Guid.NewGuid(), Guid.NewGuid(), "HiringOrder", "ПР-001", DateTime.UtcNow);
        var doc2 = doc1 with { DocumentNumber = "ПР-002" };

        Assert.NotEqual(doc1.DocumentNumber, doc2.DocumentNumber);
        Assert.Equal(doc1.DocumentId, doc2.DocumentId);
    }
}
