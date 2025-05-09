// Copyright © devsko 2025. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GeoJson;

public partial class Geo<TPosition, TCoordinate>
{
    /// <summary>
    /// Represents a GeoJSON Feature object, which is a spatially bounded entity with associated properties.
    /// </summary>
    /// <remarks>A GeoJSON Feature is defined by its geometry, properties, and an optional identifier.</remarks>
    /// <typeparam name="TProperties">The type of the properties associated with the feature. This can be any type that represents the feature's
    /// metadata or attributes.</typeparam>
    /// <param name="geometry">The geometry that specifies the spatial identity of the feature.</param>
    public class Feature<TProperties>(Geometry? geometry) : GeoJsonObject
    {
        /// <summary>
        /// Gets the geometry object associated with the object.
        /// </summary>
        [JsonPropertyName("geometry")]
        [JsonRequired]
        public Geometry? Geometry { get; init; } = geometry;

        /// <summary>
        /// Gets the properties associated with the current object.
        /// </summary>
        [JsonPropertyName("properties")]
        [JsonRequired]
        public TProperties? Properties { get; init; }

        /// <summary>
        /// Gets the unique identifier for the feature.
        /// </summary>
        [JsonPropertyName("id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonConverter(typeof(FeatureIdConverter))]
        public object? Id { get; init; }
    }

    /// <summary>
    /// Represents a GeoJSON Feature object with properties of type <see cref="IDictionary{String, Object}"/>.
    /// </summary>
    public class Feature : Feature<IDictionary<string, object?>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Feature"/> class with the specified geometry.
        /// </summary>
        /// <param name="geometry">The geometric representation of the feature. Can be <see langword="null"/> if the feature does not have a
        /// geometry.</param>
        public Feature(Geometry? geometry) : base(geometry)
        {
            Properties = new DictionarySlim();
        }
    }

    /// <summary>
    /// Represents an abstract base class for collections of geographic features in GeoJSON format.
    /// </summary>
    public abstract class FeatureCollectionBase : GeoJsonObject
    {
        internal abstract ImmutableArray<GeoJsonObject> FeatureObjects { get; init; }
    }

    /// <summary>
    /// Represents a collection of GeoJSON Features with associated properties of a specified type.
    /// </summary>
    /// <remarks>This class provides functionality for managing a collection of GeoJSON features, where each
    /// feature is expected to have properties of the specified type <typeparamref name="TProperties"/>. The collection
    /// ensures that only valid GeoJSON features are included, and provides access to the features in a strongly-typed
    /// manner.</remarks>
    /// <typeparam name="TProperties">The type of the properties associated with each feature in the collection.</typeparam>
    public class FeatureCollection<TProperties> : FeatureCollectionBase
    {
        internal override ImmutableArray<GeoJsonObject> FeatureObjects
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

        /// <summary>
        /// Initializes a new instance of the <see cref="FeatureCollection{TProperties}"/> class with the specified collection of
        /// GeoJSON Feature objects.
        /// </summary>
        /// <remarks>All <see cref="GeoJsonObject"/>s must be of type <see cref="Feature{TProperties}"/></remarks>
        /// <param name="featureObjects">An immutable array of <see cref="GeoJsonObject"/> instances representing the features in the collection.</param>
        [JsonConstructor]
        public FeatureCollection(ImmutableArray<GeoJsonObject> featureObjects)
        {
            FeatureObjects = featureObjects;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeatureCollection{TProperties}"/> class with the specified collection of
        /// GeoJSON Feature objects.
        /// </summary>
        /// <param name="features">An immutable array of Features.</param>
        public FeatureCollection(ImmutableArray<Feature<TProperties>> features)
        {
            FeatureObjects = features.CastArray<GeoJsonObject>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeatureCollection{TProperties}"/> class with the specified collection of
        /// GeoJSON Feature objects.
        /// </summary>
        /// <param name="features">A collection of <see cref="Feature{TProperties}"/> objects to include in the feature collection. Cannot be
        /// null.</param>
        public FeatureCollection(IEnumerable<Feature<TProperties>> features)
        {
            FeatureObjects = [.. features ?? throw new ArgumentNullException(nameof(features))];
        }

        /// <summary>
        /// Gets the collection of features associated with the current object.
        /// </summary>
        [JsonIgnore]
        public IEnumerable<Feature<TProperties>> Features => FeatureObjects.Cast<Feature<TProperties>>();
    }

    /// <summary>
    /// Represents a collection of GeoJSON Features with associated properties of type <see cref="IDictionary{String, Object}"/>.
    /// </summary>
    public class FeatureCollection : FeatureCollection<IDictionary<string, object?>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FeatureCollection{TProperties}"/> class with the specified collection of
        /// GeoJSON Feature objects.
        /// </summary>
        /// <remarks>All <see cref="GeoJsonObject"/>s must be of type <see cref="Feature{TProperties}"/></remarks>
        /// <param name="featureObjects">An immutable array of <see cref="GeoJsonObject"/> instances representing the features in the collection.</param>
        [JsonConstructor]
        public FeatureCollection(ImmutableArray<GeoJsonObject> featureObjects)
            : base(featureObjects)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeatureCollection{TProperties}"/> class with the specified collection of
        /// GeoJSON Feature objects.
        /// </summary>
        /// <param name="features">An immutable array of Features.</param>
        public FeatureCollection(ImmutableArray<Feature<IDictionary<string, object?>>> features)
            : base(features)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeatureCollection{TProperties}"/> class with the specified collection of
        /// GeoJSON Feature objects.
        /// </summary>
        /// <param name="features">A collection of <see cref="Feature{TProperties}"/> objects to include in the feature collection. Cannot be
        /// null.</param>
        public FeatureCollection(IEnumerable<Feature<IDictionary<string, object?>>> features)
            : base(features)
        { }

        /// <summary>
        /// Gets the collection of features associated with the current object.
        /// </summary>
        [JsonIgnore]
        public new IEnumerable<Feature> Features => FeatureObjects.Cast<Feature>();
    }
}

/// <summary>
/// Provides a custom JSON converter that supports deserialization and serialization of objects as either strings,
/// integers, or floating-point numbers.
/// </summary>
public class FeatureIdConverter : JsonConverter<object>
{
    /// <summary>
    /// Reads and converts the JSON data to an appropriate .NET object based on the token type.
    /// </summary>
    /// <param name="reader">The <see cref="Utf8JsonReader"/> to read from.</param>
    /// <param name="typeToConvert">The type of the object to convert to. This parameter is not used in this implementation.</param>
    /// <param name="options">The <see cref="JsonSerializerOptions"/> to use. This parameter is not used in this implementation.</param>
    /// <returns>A <see cref="string"/> if the JSON token is a string, an <see cref="int"/> if the token is a number that fits in
    /// a 32-bit integer, a <see cref="long"/> if the token is a number that fits in a 64-bit integer, or a <see
    /// cref="double"/> for other numeric values.</returns>
    /// <exception cref="NotSupportedException">Thrown if the JSON token type is not supported by this method.</exception>
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

    /// <summary>
    /// Writes the specified value to the provided <see cref="Utf8JsonWriter"/> in JSON format.
    /// </summary>
    /// <remarks>This method writes the <paramref name="value"/> to the <paramref name="writer"/> in a
    /// type-specific manner: <list type="bullet"> <item><description>If the value is a <see cref="string"/>, it is
    /// written as a JSON string.</description></item> <item><description>If the value is an <see cref="int"/>, <see
    /// cref="long"/>, or <see cref="double"/>, it is written as a JSON number.</description></item> </list> Any other
    /// type will result in a <see cref="NotSupportedException"/> being thrown.</remarks>
    /// <param name="writer">The <see cref="Utf8JsonWriter"/> to which the value will be written. This parameter cannot be <see
    /// langword="null"/>.</param>
    /// <param name="value">The value to write. Supported types are <see cref="string"/>, <see cref="int"/>, <see cref="long"/>, and <see
    /// cref="double"/>.</param>
    /// <param name="options">The <see cref="JsonSerializerOptions"/> to use during serialization. This parameter is not used in this method
    /// but is required by the method signature.</param>
    /// <exception cref="NotSupportedException">Thrown if the <paramref name="value"/> is not of a supported type.</exception>
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
