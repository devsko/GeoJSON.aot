// Copyright © devsko 2025
// Licensed under the MIT license.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace GeoJSON.Test;

public static partial class PositionTests
{
    [Theory]
    [InlineData(180.1, 0)]
    [InlineData(-180.1, 0)]
    [InlineData(0, 90.1)]
    [InlineData(0, -90.1)]
    [InlineData(double.NaN, 0)]
    [InlineData(0, double.NaN)]
    public static void Position2DBounds(double longitude, double latitude)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Position2D<double>(longitude, latitude));
    }

    [Theory]
    [InlineData(180.1, 0)]
    [InlineData(-180.1, 0)]
    [InlineData(0, 90.1)]
    [InlineData(0, -90.1)]
    [InlineData(double.NaN, 0)]
    [InlineData(0, double.NaN)]
    public static void Position3DBounds(double longitude, double latitude)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Position3D<double>(longitude, latitude));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Position3D<double>(longitude, latitude, 0));
    }

    [Fact]
    public static void Position2DRoundtrip()
    {
        Position2D<double> position = new(1, 2);
        string json = JsonSerializer.Serialize(position, GeoDouble2D.Default.Options.GetTypeInfo(typeof(Position2D<double>)));

        Assert.Equal("[1,2]", json);

        Position2D<double>? deserialized = (Position2D<double>?)JsonSerializer.Deserialize(json, GeoDouble2D.Default.Options.GetTypeInfo(typeof(Position2D<double>)));

        Assert.Equal(position, deserialized);
    }

    [Fact]
    public static void Position3DRoundtripWithAltitude()
    {
        Position3D<double> position = new(1, 2, 3);
        string json = JsonSerializer.Serialize(position, GeoDouble3D.Default.Options.GetTypeInfo(typeof(Position3D<double>)));

        Assert.Equal("[1,2,3]", json);

        Position3D<double>? deserialized = (Position3D<double>?)JsonSerializer.Deserialize(json, GeoDouble3D.Default.Options.GetTypeInfo(typeof(Position3D<double>)));

        Assert.Equal(position, deserialized);
    }

    [Fact]
    public static void Position3DRoundtripWithoutAltitude()
    {
        Position3D<double> position = new(1, 2);

        Assert.Null(position.Altitude);

        string json = JsonSerializer.Serialize(position, GeoDouble3D.Default.Options.GetTypeInfo(typeof(Position3D<double>)));

        Assert.Equal("[1,2]", json);

        Position3D<double>? deserialized = (Position3D<double>?)JsonSerializer.Deserialize(json, GeoDouble3D.Default.Options.GetTypeInfo(typeof(Position3D<double>)));

        Assert.Equal(position, deserialized);
    }

    [Fact]
    public static void Position2DHasExactly2Coordinates()
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize("[1]", GeoDouble2D.Default.Options.GetTypeInfo(typeof(Position2D<double>))));
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize("[1,1,1]", GeoDouble2D.Default.Options.GetTypeInfo(typeof(Position2D<double>))));
    }

    [Fact]
    public static void Position3DHas2Or3Coordinates()
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize("[1]", GeoDouble3D.Default.Options.GetTypeInfo(typeof(Position3D<double>))));
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize("[1,1,1,1]", GeoDouble3D.Default.Options.GetTypeInfo(typeof(Position3D<double>))));
    }

    [Fact]
    public static void PositionSingleCoordinates()
    {
        Position2D<float> position = new(1f, 2f);
        string json = JsonSerializer.Serialize(position, GeoSingle2D.Default.Options.GetTypeInfo(typeof(Position2D<float>)));
        Assert.Equal("[1,2]", json);
        Position2D<float>? deserialized = (Position2D<float>?)JsonSerializer.Deserialize(json, GeoSingle2D.Default.Options.GetTypeInfo(typeof(Position2D<float>)));
        Assert.Equal(position, deserialized);
    }

    [Fact]
    public static void PositionHalfCoordinates()
    {
        Position2D<Half> position = new((Half)1, (Half)2);
        string json = JsonSerializer.Serialize(position, GeoHalf2D.Default.Options.GetTypeInfo(typeof(Position2D<Half>)));
        Assert.Equal("[1,2]", json);
        Position2D<Half>? deserialized = (Position2D<Half>?)JsonSerializer.Deserialize(json, GeoHalf2D.Default.Options.GetTypeInfo(typeof(Position2D<Half>)));
        Assert.Equal(position, deserialized);
    }

    [Fact]
    public static void PositionDecimalCoordinates()
    {
        Position2D<decimal> position = new(1m, 2m);
        string json = JsonSerializer.Serialize(position, GeoDecimal2D.Default.Options.GetTypeInfo(typeof(Position2D<decimal>)));
        Assert.Equal("[1,2]", json);
        Position2D<decimal>? deserialized = (Position2D<decimal>?)JsonSerializer.Deserialize(json, GeoDecimal2D.Default.Options.GetTypeInfo(typeof(Position2D<decimal>)));
        Assert.Equal(position, deserialized);
    }
}
