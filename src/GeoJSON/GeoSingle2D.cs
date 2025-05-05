// Copyright © devsko 2025
// Licensed under the MIT license.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace GeoJSON;

public sealed partial class GeoSingle2D : Geo<Position2D<float>, float>
{
    public static GeoSingle2D Default { get; } = new();

    private GeoSingle2D() : base()
    { }

    public GeoSingle2D(JsonSerializerOptions options) : base(options)
    { }

    public GeoSingle2D(JsonSerializerContext additional, Type? featurePropertiesType = null) : base(additional, featurePropertiesType)
    { }

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
