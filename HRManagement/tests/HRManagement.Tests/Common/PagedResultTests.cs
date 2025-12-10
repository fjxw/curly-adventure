using FluentAssertions;
using HRManagement.Shared.Common.Models;

namespace HRManagement.Tests.Common;

public class PagedResultTests
{
    [Fact]
    public void PagedResult_ShouldCalculateTotalPagesCorrectly()
    {
        // Arrange
        var items = new List<string> { "item1", "item2", "item3" };

        // Act
        var result = PagedResult<string>.Create(items, 25, 1, 10);

        // Assert
        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(25);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalPages.Should().Be(3); // 25 / 10 = 2.5 -> ceiling = 3
    }

    [Fact]
    public void PagedResult_HasNextPage_ShouldBeTrue_WhenNotLastPage()
    {
        // Act
        var result = PagedResult<int>.Create(new List<int> { 1, 2, 3, 4, 5 }, 15, 1, 5);

        // Assert
        result.HasNextPage.Should().BeTrue();
        result.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void PagedResult_HasNextPage_ShouldBeFalse_WhenLastPage()
    {
        // Act
        var result = PagedResult<int>.Create(new List<int> { 11, 12, 13 }, 13, 3, 5);

        // Assert
        result.HasNextPage.Should().BeFalse();
        result.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public void PagedResult_MiddlePage_ShouldHaveBothNavigations()
    {
        // Act
        var result = PagedResult<string>.Create(new List<string> { "a", "b", "c", "d", "e" }, 15, 2, 5);

        // Assert
        result.HasNextPage.Should().BeTrue();
        result.HasPreviousPage.Should().BeTrue();
        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public void PagedResult_EmptyItems_ShouldHaveZeroTotalPages()
    {
        // Act
        var result = PagedResult<object>.Create(new List<object>(), 0, 1, 10);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
        result.HasNextPage.Should().BeFalse();
        result.HasPreviousPage.Should().BeFalse();
    }
}
