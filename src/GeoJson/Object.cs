// Copyright © devsko 2025. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace GeoJson;

public partial class Geo<TPosition, TCoordinate>
{
    /// <summary>
    /// Represents the base class for all GeoJSON objects.
    /// </summary>
    /// <remarks>This class serves as the foundation for all GeoJSON object types, such as Point, LineString,
    /// and Polygon. Derived types are not registered with <see cref="JsonDerivedTypeAttribute"/> but at runtime because of <c>CS0416</c>.
    /// </remarks>
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    public abstract class GeoJsonObject
    {
        /// <summary>
        /// Gets the bounding box that defines the spatial extent of the object.
        /// </summary>
        [JsonPropertyName("bbox")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public BBox? BBox { get; init; }

        /// <summary>
        /// Gets the Coordinate Reference System (CRS) associated with the geographic data.
        /// </summary>
        [JsonPropertyName("crs")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Crs? Crs { get; init; }
    }

    /// <summary>
    /// Represents the base class for all GeoJSON objects representing geometric shapes.
    /// </summary>
    /// <remarks>This class serves as the foundation for all geometric shape types, such as Point, LineString,
    /// and Polygon. Derived types are not registered with <see cref="JsonDerivedTypeAttribute"/> but at runtime because of <c>CS0416</c>.
    /// </remarks>
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    public abstract class Geometry : GeoJsonObject
    { }
}
