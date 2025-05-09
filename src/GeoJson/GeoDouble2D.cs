// Copyright © devsko 2025. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace GeoJson;

/// <summary>
/// A specialized implementation of the GeoJSON serializer, using 2D positions (longitude and latitude) with decimal double floating point coordinates.
/// </summary>
public sealed partial class GeoDouble2D : Geo<Position2D<double>, double>
{
    /// <summary>
    /// The default instance of the <see cref="GeoDouble2D"/> serializer.
    /// </summary>
    public static GeoDouble2D Default { get; } = new();

    private GeoDouble2D() : base()
    { }

    /// <inheritdoc/>
    public GeoDouble2D(JsonSerializerOptions options) : base(options)
    { }

    /// <inheritdoc/>
    public GeoDouble2D(JsonSerializerContext additional, Type? featurePropertiesType = null) : base(additional, featurePropertiesType)
    { }

    /// <inheritdoc/>
    protected override JsonSerializerContext BaseContext => Double2D.Default;

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
    internal sealed partial class Double2D : JsonSerializerContext
    { }
}
