// Copyright © devsko 2025. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace GeoJson;

public sealed partial class GeoDecimal3D : Geo<Position3D<decimal>, decimal>
{
    public static GeoDecimal3D Default { get; } = new();

    private GeoDecimal3D() : base()
    { }

    public GeoDecimal3D(JsonSerializerOptions options) : base(options)
    { }

    public GeoDecimal3D(JsonSerializerContext additional, Type? featurePropertiesType = null) : base(additional, featurePropertiesType)
    { }

    protected override JsonSerializerContext BaseContext => Decimal3D.Default;

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
    internal sealed partial class Decimal3D : JsonSerializerContext
    { }
}
