// Copyright © devsko 2025. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace GeoJSON;

public partial class Geo<TPosition, TCoordinate>
{
    // The derived types cannot be declared here with JsonDerivedTypeAttribute because of CS0416.
    // They are registered at runtime. See Geo<TPosition, TCoordinate>.CreateOptions()
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    public abstract class GeoJsonObject
    {
        [JsonPropertyName("bbox")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public BBox? BBox { get; set; }

        [JsonPropertyName("crs")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Crs? Crs { get; set; }
    }

    // The derived types cannot be declared here with JsonDerivedTypeAttribute because of CS0416.
    // They are registered at runtime. See Geo<TPosition, TCoordinate>.CreateOptions()
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    public abstract class Geometry : GeoJsonObject
    { }
}
