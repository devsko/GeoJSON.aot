// Copyright © devsko 2025
// Licensed under the MIT license.

using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GeoJSON;

public partial class Geo<TPosition, TCoordinate>
{
    public class Feature<TProperties>(Geometry? geometry) : GeoJsonObject
    {
        [JsonPropertyName("geometry")]
        [JsonRequired]
        public Geometry? Geometry { get; init; } = geometry;

        [JsonPropertyName("properties")]
        [JsonRequired]
        public TProperties? Properties { get; init; }

        [JsonPropertyName("id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonConverter(typeof(FeatureIdConverter))]
        public object? Id { get; init; }
    }

    public class Feature : Feature<IDictionary<string, object?>>
    {
        public Feature(Geometry? geometry) : base(geometry)
        {
            Properties = new SlimDictionary();
        }
    }

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

    public class FeatureCollection : FeatureCollection<IDictionary<string, object?>>
    {
        [JsonConstructor]
        public FeatureCollection(ImmutableArray<GeoJsonObject> featureObjects)
            : base(featureObjects)
        { }

        public FeatureCollection(ImmutableArray<Feature<IDictionary<string, object?>>> features)
            : base(features)
        { }

        public FeatureCollection(IEnumerable<Feature<IDictionary<string, object?>>> features)
            : base(features)
        { }
    }
}

public class FeatureIdConverter : JsonConverter<object>
{
    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            return reader.GetString();
        }
        if (reader.TokenType == JsonTokenType.Number)
        {
            if (reader.TryGetInt32(out int intValue))
            {
                return intValue;
            }
            if (reader.TryGetInt64(out long longValue))
            {
                return longValue;
            }
            if (reader.TryGetDouble(out double doubleValue))
            {
                return doubleValue;
            }
        }

        throw new NotSupportedException();
    }

    public override void Write(Utf8JsonWriter writer, object? value, JsonSerializerOptions options)
    {
        if (value is string stringValue)
        {
            writer.WriteStringValue(stringValue);
        }
        else if (value is int intValue)
        {
            writer.WriteNumberValue(intValue);
        }
        else if (value is long longValue)
        {
            writer.WriteNumberValue(longValue);
        }
        else if (value is double doubleValue)
        {
            writer.WriteNumberValue(doubleValue);
        }
        else
        {
            throw new NotSupportedException();
        }
    }
}
