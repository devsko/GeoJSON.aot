// Copyright © devsko 2025. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace GeoJson;

/// <summary>
/// A specialized implementation of the GeoJSON serializer, using 3D positions (longitude, latitude and optional altitude) with double floating point precision coordinates.
/// </summary>
public sealed partial class GeoDouble3D : Geo<Position3D<double>, double>
{
    /// <summary>
    /// The default instance of the <see cref="GeoDouble3D"/> serializer.
    /// </summary>
    public static GeoDouble3D Default { get; } = new();

    private GeoDouble3D() : base()
    { }

    /// <inheritdoc/>
    public GeoDouble3D(JsonSerializerOptions options) : base(options)
    { }

    /// <inheritdoc/>
    public GeoDouble3D(JsonSerializerContext additional, Type? featurePropertiesType = null) : base(additional, featurePropertiesType)
    { }

    /// <inheritdoc/>
    protected override JsonSerializerContext BaseContext => Double3D.Default;

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
    internal sealed partial class Double3D : JsonSerializerContext
    { }
}
