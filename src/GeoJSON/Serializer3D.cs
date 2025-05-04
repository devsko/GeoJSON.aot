// Copyright © devsko 2025
// Licensed under the MIT license.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace GeoJson;

public sealed partial class Serializer3D : Serializer<Position3D>
{
    public static Serializer3D Default { get; } = new();

    private Serializer3D() : base()
    { }

    public Serializer3D(JsonSerializerOptions options) : base(options)
    { }

    public Serializer3D(JsonSerializerContext additional, Type? featurePropertiesType = null) : base(additional, featurePropertiesType)
    { }

    protected override JsonSerializerContext BaseContext => JsonContext3D.Default;

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
    internal sealed partial class JsonContext3D : JsonSerializerContext
    { }
}
