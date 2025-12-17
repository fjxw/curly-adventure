using FluentAssertions;
using HRManagement.Shared.Common.Caching;
using Microsoft.Extensions.Caching.Memory;

namespace HRManagement.Tests.Common;

public class MemoryCacheServiceTests
{
    private readonly ICacheService _cacheService;

    public MemoryCacheServiceTests()
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        _cacheService = new MemoryCacheService(memoryCache);
    }

    [Fact]
    public async Task SetAsync_AndGetAsync_ShouldStoreAndRetrieveValue()
    {
        var key = "test_key_1";
        var value = new TestObject { Id = 1, Name = "Test" };

        await _cacheService.SetAsync(key, value, TimeSpan.FromMinutes(5));
        var result = await _cacheService.GetAsync<TestObject>(key);

        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Test");
    }

    [Fact]
    public async Task GetAsync_NonExistentKey_ShouldReturnNull()
    {
        var result = await _cacheService.GetAsync<TestObject>("non_existent_key");

        result.Should().BeNull();
    }

    [Fact]
    public async Task RemoveAsync_ShouldDeleteCachedValue()
    {
        var key = "test_key_2";
        var value = new TestObject { Id = 2, Name = "ToBeDeleted" };
        await _cacheService.SetAsync(key, value, TimeSpan.FromMinutes(5));

        await _cacheService.RemoveAsync(key);
        var result = await _cacheService.GetAsync<TestObject>(key);

        result.Should().BeNull();
    }

    [Fact]
    public async Task SetAsync_WithShortExpiration_ShouldExpire()
    {
        var key = "test_key_expiring";
        var value = new TestObject { Id = 3, Name = "Expiring" };

        await _cacheService.SetAsync(key, value, TimeSpan.FromMilliseconds(50));
        await Task.Delay(100);
        var result = await _cacheService.GetAsync<TestObject>(key);

        result.Should().BeNull();
    }

    [Fact]
    public async Task SetAsync_OverwriteExistingKey_ShouldUpdateValue()
    {
        var key = "test_key_overwrite";
        var originalValue = new TestObject { Id = 1, Name = "Original" };
        var newValue = new TestObject { Id = 2, Name = "Updated" };

        await _cacheService.SetAsync(key, originalValue, TimeSpan.FromMinutes(5));
        await _cacheService.SetAsync(key, newValue, TimeSpan.FromMinutes(5));
        var result = await _cacheService.GetAsync<TestObject>(key);

        result.Should().NotBeNull();
        result!.Id.Should().Be(2);
        result.Name.Should().Be("Updated");
    }

    [Fact]
    public async Task SetAsync_WithPrimitiveTypes_ShouldWork()
    {
        var stringKey = "string_key";
        var intKey = "int_key";

        await _cacheService.SetAsync(stringKey, "Hello World", TimeSpan.FromMinutes(5));
        await _cacheService.SetAsync(intKey, 42, TimeSpan.FromMinutes(5));

        var stringResult = await _cacheService.GetAsync<string>(stringKey);
        var intResult = await _cacheService.GetAsync<int>(intKey);

        stringResult.Should().Be("Hello World");
        intResult.Should().Be(42);
    }

    private class TestObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
