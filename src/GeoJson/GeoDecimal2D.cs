﻿// Copyright © devsko 2025. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace GeoJson;

/// <summary>
/// A specialized implementation of the GeoJSON serializer, using 2D positions (longitude and latitude) with decimal precision coordinates.
/// </summary>
public sealed partial class GeoDecimal2D : Geo<Position2D<decimal>, decimal>
{
    /// <summary>
    /// The default instance of the <see cref="GeoDecimal2D"/> serializer.
    /// </summary>
    public static GeoDecimal2D Default { get; } = new();

    private GeoDecimal2D() : base()
    { }

    /// <inheritdoc/>
    public GeoDecimal2D(JsonSerializerOptions options) : base(options)
    { }

    /// <inheritdoc/>
    public GeoDecimal2D(JsonSerializerContext additional, Type? featurePropertiesType = null) : base(additional, featurePropertiesType)
    { }

    /// <inheritdoc/>
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
