using FluentAssertions;
using HRManagement.Shared.Common.Models;

namespace HRManagement.Tests.Common;

public class ApiResponseTests
{
    [Fact]
    public void SuccessResponse_WithData_ShouldSetCorrectProperties()
    {
        // Arrange
        var testData = new { Id = 1, Name = "Test" };

        // Act
        var response = ApiResponse<object>.SuccessResponse(testData, "Операция выполнена");

        // Assert
        response.Success.Should().BeTrue();
        response.Data.Should().Be(testData);
        response.Message.Should().Be("Операция выполнена");
        response.Errors.Should().BeEmpty();
    }

    [Fact]
    public void SuccessResponse_WithoutMessage_ShouldHaveNullMessage()
    {
        // Arrange
        var testData = "Test data";

        // Act
        var response = ApiResponse<string>.SuccessResponse(testData);

        // Assert
        response.Success.Should().BeTrue();
        response.Data.Should().Be(testData);
        response.Message.Should().BeNull();
    }

    [Fact]
    public void FailureResponse_WithMessage_ShouldSetCorrectProperties()
    {
        // Act
        var response = ApiResponse<string>.FailureResponse("Ошибка валидации");

        // Assert
        response.Success.Should().BeFalse();
        response.Data.Should().BeNull();
        response.Message.Should().Be("Ошибка валидации");
        response.Errors.Should().BeEmpty();
    }

    [Fact]
    public void FailureResponse_WithErrors_ShouldContainAllErrors()
    {
        // Arrange
        var errors = new List<string> { "Ошибка 1", "Ошибка 2", "Ошибка 3" };

        // Act
        var response = ApiResponse<object>.FailureResponse("Ошибки валидации", errors);

        // Assert
        response.Success.Should().BeFalse();
        response.Errors.Should().HaveCount(3);
        response.Errors.Should().Contain("Ошибка 1");
        response.Errors.Should().Contain("Ошибка 2");
        response.Errors.Should().Contain("Ошибка 3");
    }

    [Fact]
    public void NonGeneric_SuccessResponse_ShouldWork()
    {
        // Act
        var response = ApiResponse.SuccessResponse("Успешно удалено");

        // Assert
        response.Success.Should().BeTrue();
        response.Message.Should().Be("Успешно удалено");
    }

    [Fact]
    public void NonGeneric_FailureResponse_ShouldWork()
    {
        // Act
        var response = ApiResponse.FailureResponse("Не удалось удалить");

        // Assert
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Не удалось удалить");
    }
}
