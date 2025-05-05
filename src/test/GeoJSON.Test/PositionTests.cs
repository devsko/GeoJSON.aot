// Copyright © devsko 2025
// Licensed under the MIT license.

using System.Text.Json;
using System.Text.Json.Serialization;

using static GeoJSON.Geo<double>;

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
        Assert.Throws<ArgumentOutOfRangeException>(() => new Position2D(longitude, latitude));
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
        Assert.Throws<ArgumentOutOfRangeException>(() => new Position3D(longitude, latitude));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Position3D(longitude, latitude, 0));
    }

    [Fact]
    public static void Position2DRoundtrip()
    {
        Position2D position = new(1, 2);
        string json = JsonSerializer.Serialize(position, GeoDouble2D.Default.Options.GetTypeInfo(typeof(Position2D)));

        Assert.Equal("[1,2]", json);

        Position2D? deserialized = (Position2D?)JsonSerializer.Deserialize(json, GeoDouble2D.Default.Options.GetTypeInfo(typeof(Position2D)));

        Assert.Equal(position, deserialized);
    }

    [Fact]
    public static void Position3DRoundtripWithAltitude()
    {
        Position3D position = new(1, 2, 3);
        string json = JsonSerializer.Serialize(position, GeoDouble3D.Default.Options.GetTypeInfo(typeof(Position3D)));

        Assert.Equal("[1,2,3]", json);

        Position3D? deserialized = (Position3D?)JsonSerializer.Deserialize(json, GeoDouble3D.Default.Options.GetTypeInfo(typeof(Position3D)));

        Assert.Equal(position, deserialized);
    }

    [Fact]
    public static void Position3DRoundtripWithoutAltitude()
    {
        Position3D position = new(1, 2);

        Assert.Null(position.Altitude);

        string json = JsonSerializer.Serialize(position, GeoDouble3D.Default.Options.GetTypeInfo(typeof(Position3D)));

        Assert.Equal("[1,2]", json);

        Position3D? deserialized = (Position3D?)JsonSerializer.Deserialize(json, GeoDouble3D.Default.Options.GetTypeInfo(typeof(Position3D)));

        Assert.Equal(position, deserialized);
    }

    [Fact]
    public static void Position2DHasExactly2Coordinates()
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize("[1]", GeoDouble2D.Default.Options.GetTypeInfo(typeof(Position2D))));
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize("[1,1,1]", GeoDouble2D.Default.Options.GetTypeInfo(typeof(Position2D))));
    }

    [Fact]
    public static void Position3DHas2Or3Coordinates()
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize("[1]", GeoDouble3D.Default.Options.GetTypeInfo(typeof(Position3D))));
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize("[1,1,1,1]", GeoDouble3D.Default.Options.GetTypeInfo(typeof(Position3D))));
    }

    [Fact]
    public static void PositionSingleCoordinates()
    {
        Geo<float>.Position2D position = new(1f, 2f);
        string json = JsonSerializer.Serialize(position, GeoSingle2D.Default.Options.GetTypeInfo(typeof(Geo<float>.Position2D)));
        Assert.Equal("[1,2]", json);
        Geo<float>.Position2D? deserialized = (Geo<float>.Position2D?)JsonSerializer.Deserialize(json, GeoSingle2D.Default.Options.GetTypeInfo(typeof(Geo<float>.Position2D)));
        Assert.Equal(position, deserialized);
    }

    [Fact]
    public static void PositionHalfCoordinates()
    {
        Geo<Half>.Position2D position = new((Half)1, (Half)2);
        string json = JsonSerializer.Serialize(position, GeoHalf2D.Default.Options.GetTypeInfo(typeof(Geo<Half>.Position2D)));
        Assert.Equal("[1,2]", json);
        Geo<Half>.Position2D? deserialized = (Geo<Half>.Position2D?)JsonSerializer.Deserialize(json, GeoHalf2D.Default.Options.GetTypeInfo(typeof(Geo<Half>.Position2D)));
        Assert.Equal(position, deserialized);
    }

    [Fact]
    public static void PositionDecimalCoordinates()
    {
        Geo<decimal>.Position2D position = new(1m, 2m);
        string json = JsonSerializer.Serialize(position, GeoDecimal2D.Default.Options.GetTypeInfo(typeof(Geo<decimal>.Position2D)));
        Assert.Equal("[1,2]", json);
        Geo<decimal>.Position2D? deserialized = (Geo<decimal>.Position2D?)JsonSerializer.Deserialize(json, GeoDecimal2D.Default.Options.GetTypeInfo(typeof(Geo<decimal>.Position2D)));
        Assert.Equal(position, deserialized);
    }

    public sealed partial class GeoSingle2D : Geo<Geo<float>.Position2D, float>
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

    public sealed partial class GeoHalf2D : Geo<Geo<Half>.Position2D, Half>
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

    public sealed partial class GeoDecimal2D : Geo<Geo<decimal>.Position2D, decimal>
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
