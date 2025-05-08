// Copyright © devsko 2025. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace GeoJson;

public sealed partial class GeoHalf3D : Geo<Position3D<Half>, Half>
{
    public static GeoHalf3D Default { get; } = new();

    private GeoHalf3D() : base()
    { }

    public GeoHalf3D(JsonSerializerOptions options) : base(options)
    { }

    public GeoHalf3D(JsonSerializerContext additional, Type? featurePropertiesType = null) : base(additional, featurePropertiesType)
    { }

    protected override JsonSerializerContext BaseContext => Half3D.Default;

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
    internal sealed partial class Half3D : JsonSerializerContext
    { }
}
