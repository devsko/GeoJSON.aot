// Copyright © devsko 2025
// Licensed under the MIT license.

using System.Text.Json;
using System.Text.Json.Serialization;

using static GeoJSON.Position<double>;

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
        Assert.Throws<ArgumentOutOfRangeException>(() => new TwoD(longitude, latitude));
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
        Assert.Throws<ArgumentOutOfRangeException>(() => new ThreeD(longitude, latitude));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ThreeD(longitude, latitude, 0));
    }

    [Fact]
    public static void Position2DRoundtrip()
    {
        TwoD position = new(1, 2);
        string json = JsonSerializer.Serialize(position, GeoDouble2D.Default.Options.GetTypeInfo(typeof(TwoD)));

        Assert.Equal("[1,2]", json);

        TwoD? deserialized = (TwoD?)JsonSerializer.Deserialize(json, GeoDouble2D.Default.Options.GetTypeInfo(typeof(TwoD)));

        Assert.Equal(position, deserialized);
    }

    [Fact]
    public static void Position3DRoundtripWithAltitude()
    {
        ThreeD position = new(1, 2, 3);
        string json = JsonSerializer.Serialize(position, GeoDouble3D.Default.Options.GetTypeInfo(typeof(ThreeD)));

        Assert.Equal("[1,2,3]", json);

        ThreeD? deserialized = (ThreeD?)JsonSerializer.Deserialize(json, GeoDouble3D.Default.Options.GetTypeInfo(typeof(ThreeD)));

        Assert.Equal(position, deserialized);
    }

    [Fact]
    public static void Position3DRoundtripWithoutAltitude()
    {
        ThreeD position = new(1, 2);

        Assert.Null(position.Altitude);

        string json = JsonSerializer.Serialize(position, GeoDouble3D.Default.Options.GetTypeInfo(typeof(ThreeD)));

        Assert.Equal("[1,2]", json);

        ThreeD? deserialized = (ThreeD?)JsonSerializer.Deserialize(json, GeoDouble3D.Default.Options.GetTypeInfo(typeof(ThreeD)));

        Assert.Equal(position, deserialized);
    }

    [Fact]
    public static void Position2DHasExactly2Coordinates()
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize("[1]", GeoDouble2D.Default.Options.GetTypeInfo(typeof(TwoD))));
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize("[1,1,1]", GeoDouble2D.Default.Options.GetTypeInfo(typeof(TwoD))));
    }

    [Fact]
    public static void Position3DHas2Or3Coordinates()
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize("[1]", GeoDouble3D.Default.Options.GetTypeInfo(typeof(ThreeD))));
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize("[1,1,1,1]", GeoDouble3D.Default.Options.GetTypeInfo(typeof(ThreeD))));
    }

    [Fact]
    public static void PositionSingleCoordinates()
    {
        Position<float>.TwoD position = new(1f, 2f);
        string json = JsonSerializer.Serialize(position, GeoSingle2D.Default.Options.GetTypeInfo(typeof(Position<float>.TwoD)));
        Assert.Equal("[1,2]", json);
        Position<float>.TwoD? deserialized = (Position<float>.TwoD?)JsonSerializer.Deserialize(json, GeoSingle2D.Default.Options.GetTypeInfo(typeof(Position<float>.TwoD)));
        Assert.Equal(position, deserialized);
    }

    [Fact]
    public static void PositionHalfCoordinates()
    {
        Position<Half>.TwoD position = new((Half)1, (Half)2);
        string json = JsonSerializer.Serialize(position, GeoHalf2D.Default.Options.GetTypeInfo(typeof(Position<Half>.TwoD)));
        Assert.Equal("[1,2]", json);
        Position<Half>.TwoD? deserialized = (Position<Half>.TwoD?)JsonSerializer.Deserialize(json, GeoHalf2D.Default.Options.GetTypeInfo(typeof(Position<Half>.TwoD)));
        Assert.Equal(position, deserialized);
    }

    [Fact]
    public static void PositionDecimalCoordinates()
    {
        Position<decimal>.TwoD position = new(1m, 2m);
        string json = JsonSerializer.Serialize(position, GeoDecimal2D.Default.Options.GetTypeInfo(typeof(Position<decimal>.TwoD)));
        Assert.Equal("[1,2]", json);
        Position<decimal>.TwoD? deserialized = (Position<decimal>.TwoD?)JsonSerializer.Deserialize(json, GeoDecimal2D.Default.Options.GetTypeInfo(typeof(Position<decimal>.TwoD)));
        Assert.Equal(position, deserialized);
    }

    public sealed partial class GeoSingle2D : Geo<Position<float>.TwoD, float>
    {
        public static GeoSingle2D Default { get; } = new();

        protected override JsonSerializerContext BaseContext => Single2D.Default;

        [JsonSerializable(typeof(GeoJsonObject))]
        [JsonSerializable(typeof(Feature))]
        [JsonSerializable(typeof(FeatureCollection))]
        [JsonSerializable(typeof(Geometry))]
        [JsonSerializable(typeof(Point))]
        [JsonSerializable(typeof(MultiPoint))]
        [JsonSerializable(typeof(LineString))]
        [JsonSerializable(typeof(MultiLineString))]
        [JsonSerializable(typeof(Polygon))]
        [JsonSerializable(typeof(MultiPolygon))]
        [JsonSerializable(typeof(GeometryCollection))]
        [JsonSerializable(typeof(NoCrs))]
        [JsonSerializable(typeof(NamedCrs))]
        [JsonSerializable(typeof(LinkedCrs))]
        internal sealed partial class Single2D : JsonSerializerContext
        { }
    }

    public sealed partial class GeoHalf2D : Geo<Position<Half>.TwoD, Half>
    {
        public static GeoHalf2D Default { get; } = new();

        protected override JsonSerializerContext BaseContext => Half2D.Default;

        [JsonSerializable(typeof(GeoJsonObject))]
        [JsonSerializable(typeof(Feature))]
        [JsonSerializable(typeof(FeatureCollection))]
        [JsonSerializable(typeof(Geometry))]
        [JsonSerializable(typeof(Point))]
        [JsonSerializable(typeof(MultiPoint))]
        [JsonSerializable(typeof(LineString))]
        [JsonSerializable(typeof(MultiLineString))]
        [JsonSerializable(typeof(Polygon))]
        [JsonSerializable(typeof(MultiPolygon))]
        [JsonSerializable(typeof(GeometryCollection))]
        [JsonSerializable(typeof(NoCrs))]
        [JsonSerializable(typeof(NamedCrs))]
        [JsonSerializable(typeof(LinkedCrs))]
        internal sealed partial class Half2D : JsonSerializerContext
        { }
    }

    public sealed partial class GeoDecimal2D : Geo<Position<decimal>.TwoD, decimal>
    {
        public static GeoDecimal2D Default { get; } = new();

        protected override JsonSerializerContext BaseContext => Decimal2D.Default;

        [JsonSerializable(typeof(GeoJsonObject))]
        [JsonSerializable(typeof(Feature))]
        [JsonSerializable(typeof(FeatureCollection))]
        [JsonSerializable(typeof(Geometry))]
        [JsonSerializable(typeof(Point))]
        [JsonSerializable(typeof(MultiPoint))]
        [JsonSerializable(typeof(LineString))]
        [JsonSerializable(typeof(MultiLineString))]
        [JsonSerializable(typeof(Polygon))]
        [JsonSerializable(typeof(MultiPolygon))]
        [JsonSerializable(typeof(GeometryCollection))]
        [JsonSerializable(typeof(NoCrs))]
        [JsonSerializable(typeof(NamedCrs))]
        [JsonSerializable(typeof(LinkedCrs))]
        internal sealed partial class Decimal2D : JsonSerializerContext
        { }
    }
}
