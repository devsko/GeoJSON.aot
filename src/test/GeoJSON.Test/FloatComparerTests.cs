// Copyright © devsko 2025
// Licensed under the MIT license.

namespace GeoJSON.Test;

public static class DoubleComparerTests
{
    [Theory]
    [InlineData([1, 1, true])]
    [InlineData([+1, +1.000_000_000_999, true])]
    [InlineData([-1, -1.000_000_000_999, true])]
    [InlineData([+1, +1.000_000_001, false])]
    [InlineData([-1, -1.000_000_001, false])]
    [InlineData(double.NaN, 100, false)]
    [InlineData(100, double.NaN, false)]
    [InlineData(double.NaN, double.NaN, true)]
    public static void Compare(double left, double right, bool result)
    {
        Assert.Equal(result, DoubleComparer.Instance.Equals(left, right));
    }
}
