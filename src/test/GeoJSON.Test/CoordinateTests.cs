// Copyright © devsko 2025
// Licensed under the MIT license.

namespace GeoJSON.Test;

public static class CoordinateTests
{
    [Theory]
    [InlineData([1, 1, true])]
    [InlineData([-1f, -1f, true])]
    [InlineData([+1, +1.000_000_09, true])]
    [InlineData([-1, -1.000_000_09, true])]
    [InlineData([+1, +1.000_000_1, false])]
    [InlineData([-1, -1.000_000_1, false])]
    [InlineData(double.NaN, 100, false)]
    [InlineData(100, double.NaN, false)]
    [InlineData(double.NaN, double.NaN, true)]
    public static void CompareDouble(double left, double right, bool result)
    {
        Assert.Equal(result, Position<double>.CoordinateComparer.Instance.Equals(left, right));
    }

    [Theory]
    [InlineData([1f, 1f, true])]
    [InlineData([-1f, -1f, true])]
    // 1.000_000_09f is rounded to the same number as 1.000_000_1f
    // and both are CoordinateComparer-different from 1f.
    // There is no float that is different from 1 but CoordinateComparer-equal to 1
    //[InlineData([+1, +1.000_000_09f, true])]
    //[InlineData([-1, -1.000_000_09f, true])]
    [InlineData([+1f, +1.000_000_1f, false])]
    [InlineData([-1f, -1.000_000_1f, false])]
    [InlineData(float.NaN, 100f, false)]
    [InlineData(100f, float.NaN, false)]
    [InlineData(float.NaN, float.NaN, true)]
    public static void CompareSingle(float left, float right, bool result)
    {
        Assert.Equal(result, Position<float>.CoordinateComparer.Instance.Equals(left, right));
    }
}
