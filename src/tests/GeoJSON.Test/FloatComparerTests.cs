// Copyright © devsko 2025
// Licensed under the MIT license.

using GeoJson;

namespace cycloid.Test.GeoJson;

public static class FloatComparerTests
{
    [Theory]
    [InlineData([1, 1, true])]
    [InlineData([1, 1.000001, true])]
    [InlineData([-1, -1.000001, true])]
    [InlineData([1, 1.000002, false])]
    [InlineData([-1, -1.000002, false])]
    [InlineData(float.NaN, 100, false)]
    [InlineData(100, float.NaN, false)]
    [InlineData(float.NaN, float.NaN, true)]
    public static void Compare(float left, float right, bool result)
    {
        Assert.Equal(result, FloatComparer.Instance.Equals(left, right));
    }
}
