// Copyright © devsko 2025
// Licensed under the MIT license.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace GeoJson;

public partial class Serializer<TPosition>
{
    // The derived types cannot be declared by JsonDerivedTypeAttribute because of CS0416.
    // They are registered at runtime. See GeoJsonSerializer<TPosition>.CreateOptions()
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

    public class Feature<TProperties>(Geometry geometry) : GeoJsonObject
    {
        [JsonPropertyName("geometry")]
        [JsonRequired]
        public Geometry Geometry { get; init; } = geometry ?? throw new ArgumentNullException(nameof(Geometry));

        [JsonPropertyName("properties")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public TProperties? Properties { get; init; }
    }

    public class Feature(Geometry geometry) : Feature<Dictionary<string, object>>(geometry)
    { }

    public class FeatureCollection<TProperties> : GeoJsonObject
    {
        [JsonPropertyName("features")]
        [JsonRequired]
        public ImmutableArray<GeoJsonObject> FeatureObjects
        {
            get;
            init
            {
                value.EnsureNotDefault(nameof(Features));

                int i = 0;
                foreach (GeoJsonObject feature in value)
                {
                    ArgumentNullException.ThrowIfNull(feature, $"{nameof(Features)}[{i}]");

                    if (feature is not Feature<TProperties>)
                    {
                        throw new ArgumentException("A FeatureCollection can only contain Features.", $"{nameof(Features)}[{i}]");
                    }
                    i++;
                }
                field = value;
            }
        }

        [JsonConstructor]
        public FeatureCollection(ImmutableArray<GeoJsonObject> featureObjects)
        {
            FeatureObjects = featureObjects;
        }

        public FeatureCollection(ImmutableArray<Feature<TProperties>> features)
        {
            FeatureObjects = features.CastArray<GeoJsonObject>();
        }

        public FeatureCollection(IEnumerable<Feature<TProperties>> features)
        {
            FeatureObjects = [.. features];
        }

        [JsonIgnore]
        public IEnumerable<Feature<TProperties>> Features => FeatureObjects.Cast<Feature<TProperties>>();
    }

    public class FeatureCollection : FeatureCollection<Dictionary<string, object>>
    {
        [JsonConstructor]
        public FeatureCollection(ImmutableArray<GeoJsonObject> featureObjects)
            : base(featureObjects)
        { }

        public FeatureCollection(ImmutableArray<Feature<Dictionary<string, object>>> features)
            : base(features)
        { }

        public FeatureCollection(IEnumerable<Feature<Dictionary<string, object>>> features)
            : base(features)
        { }
    }
}

internal static class ImmutableArrayExtensions
{
    public static ImmutableArray<T> EnsureNotDefault<T>(this ImmutableArray<T> array, [CallerArgumentExpression(nameof(array))] string? paramName = null)
    {
        if (array.IsDefault)
        {
            throw new ArgumentNullException(paramName);
        }

        return array;
    }
}
