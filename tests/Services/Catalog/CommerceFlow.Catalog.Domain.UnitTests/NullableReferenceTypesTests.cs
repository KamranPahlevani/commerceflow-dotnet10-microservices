namespace CommerceFlow.Catalog.Domain.UnitTests;

public sealed class NullableReferenceTypesTests
{
    [Fact]
    public void GetSafeLength_WhenValueIsNull_ShouldReturnZero()
    {
        var result = GetSafeLength(null);

        Assert.Equal(0, result);
    }

    [Fact]
    public void GetSafeLength_WhenValueHasContent_ShouldReturnLength()
    {
        var result = GetSafeLength("CommerceFlow");

        Assert.Equal(12, result);
    }

    [Fact]
    public void GetLengthWithFallback_WhenValueIsNull_ShouldReturnZero()
    {
        var result = GetLengthWithFallback(null);

        Assert.Equal(0, result);
    }

    private static int GetSafeLength(string? value)
    {
        if (value is null)
        {
            return 0;
        }

        return value.Length;
    }

    private static int GetLengthWithFallback(string? value)
    {
        return value?.Length ?? 0;
    }


}