using FluentAssertions;
using ICMarkets.Application.Common;

namespace ICMarkets.UnitTests.Application;

public class PaginationTests
{
    [Theory]
    [InlineData(0, 10, 1, 10)]      // page below 1 -> default page
    [InlineData(-5, 10, 1, 10)]
    [InlineData(3, 0, 3, 25)]       // size below 1 -> default size
    [InlineData(2, 9999, 2, 200)]   // size above max -> clamped to MaxPageSize
    [InlineData(4, 50, 4, 50)]      // already valid -> unchanged
    public void Normalize_clamps_page_and_size(int page, int size, int expectedPage, int expectedSize)
    {
        var (normalizedPage, normalizedSize) = Pagination.Normalize(page, size);

        normalizedPage.Should().Be(expectedPage);
        normalizedSize.Should().Be(expectedSize);
    }
}
