// Copyright © devsko 2025. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace GeoJson;

/// <summary>
/// Represents the base class for working with GeoJSON objects and defines the object model as nested types.
/// </summary>
/// <remarks>The <see cref="Geo{TPosition, TCoordinate}"/> class provides serialization and deserialization 
/// capabilities for geographic data in GeoJSON format. It supports custom serialization options and additional JSON
/// contexts for extending functionality.</remarks>
/// <typeparam name="TPosition">The type representing a geographic position. Must implement <see cref="IPosition{TPosition, TCoordinate}"/>.</typeparam>
/// <typeparam name="TCoordinate">The type representing a coordinate value. Must implement  <see cref="IFloatingPoint{TCoordinate}"/>.</typeparam>
public abstract partial class Geo<TPosition, TCoordinate>
    where TPosition : struct, IPosition<TPosition, TCoordinate>
    where TCoordinate : unmanaged, IFloatingPoint<TCoordinate>, IMinMaxValue<TCoordinate>
{
    private readonly JsonSerializerOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="Geo{TPosition, TCoordinate}"/> class.
    /// </summary>
    protected Geo()
    {
        _options = CreateOptions();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Geo{TPosition, TCoordinate}"/> class with the specified JSON serializer options.
    /// </summary>
    /// <param name="options">The <see cref="JsonSerializerOptions"/> used to configure the serialization behavior. Cannot be <see
    /// langword="null"/>.</param>
    protected Geo(JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _options = CreateOptions(options: options);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Geo{TPosition, TCoordinate}"/> class with the specified JSON serializer context and optional
    /// feature properties type that must be serializable by the provided constext.
    /// </summary>
    /// <param name="additional">The <see cref="JsonSerializerContext"/> used to configure serialization options. If a <paramref name="featurePropertiesType"/> is provided,
    /// the types <see cref="Feature{TProperties}"/> and <see cref="FeatureCollection{TProperties}"/> must be serializable by the context. This parameter cannot be <see
    /// langword="null"/>.</param>
    /// <param name="featurePropertiesType">An optional <see cref="Type"/> representing the feature properties.</param>
    protected Geo(JsonSerializerContext additional, Type? featurePropertiesType = null)
    {
        ArgumentNullException.ThrowIfNull(additional);

        _options = CreateOptions(additional, featurePropertiesType);
    }

    /// <summary>
    /// Gets the <see cref="JsonSerializerOptions"/> used to configure the behavior of JSON serialization and deserialization.
    /// </summary>
    public JsonSerializerOptions Options => _options;

    /// <summary>
    /// When overriden in a derived class, gets the base <see cref="JsonSerializerContext"/> used for JSON serialization.
    /// </summary>
    protected abstract JsonSerializerContext BaseContext { get; }

    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "The types 'Feature<TProperties>' and 'FeatureCollection<TProperties>' *MUST* be explicitally created as JsonSerializable's of the additional context. ")]
    private JsonSerializerOptions CreateOptions(JsonSerializerContext? additional = null, Type? featurePropertiesType = null, JsonSerializerOptions? options = null)
    {
        Debug.Assert(additional is null || options is null);

        if (featurePropertiesType is not null && additional is null)
        {
            throw new InvalidOperationException("An additional conext must be provided when using a custom properties type.");
        }

        (Type? Feature, Type? Collection) featureTypes = default;
        options = new(options ?? additional?.Options ?? BaseContext.Options);
        options.Converters.Add(new PositionConverter());
        options.Converters.Add(new BBoxConverter());
        options.Converters.Add(new CrsConverter());
        options.RespectNullableAnnotations = true;
        options.TypeInfoResolver = JsonTypeInfoResolver
            .Combine(BaseContext, additional)
            .WithAddedModifier(RegisterImplementations);

        if (featurePropertiesType is not null)
        {
            featureTypes = (typeof(Feature<>).MakeGenericType(typeof(TPosition), typeof(TCoordinate), featurePropertiesType),
                typeof(FeatureCollection<>).MakeGenericType(typeof(TPosition), typeof(TCoordinate), featurePropertiesType));

            if (!options.TryGetTypeInfo(featureTypes.Feature, out _) ||
                !options.TryGetTypeInfo(featureTypes.Collection, out _))
            {
                throw new InvalidOperationException($"The custom properties type '{featureTypes.Feature}' or '{featureTypes.Collection}' could not be resolved by the additional context.");
            }
        }

        return options;

        void RegisterImplementations(JsonTypeInfo typeInfo)
        {
            if (typeInfo.Type == typeof(GeoJsonObject))
            {
                typeInfo.PolymorphismOptions?.DerivedTypes.Add(new JsonDerivedType(featureTypes.Feature ?? typeof(Feature), nameof(Feature)));
                typeInfo.PolymorphismOptions?.DerivedTypes.Add(new JsonDerivedType(featureTypes.Collection ?? typeof(FeatureCollection), nameof(FeatureCollection)));

                RegisterGeometryTypes(typeInfo);
            }
            else if (typeInfo.Type == typeof(Geometry))
            {
                RegisterGeometryTypes(typeInfo);
            }
            else if (typeInfo.Type.BaseType == typeof(Geometry))
            {
                // Creating a property to associate with the '_' ctor parameter
                // This way the internal ctor's are called and the validation is skipped when deserializing
                JsonPropertyInfo dummyProperty = JsonMetadataServices.CreatePropertyInfo(options, new JsonPropertyInfoValues<bool>()
                {
                    IsProperty = true,
                    DeclaringType = typeInfo.Type,
                    IgnoreCondition = JsonIgnoreCondition.Always,
                    PropertyName = "_",
                });
                typeInfo.Properties.Add(dummyProperty);
            }
            else if (typeInfo.Type.BaseType == typeof(FeatureCollectionBase) || typeInfo.Type.BaseType?.BaseType == typeof(FeatureCollectionBase))
            {
                JsonPropertyInfo featuresProperty = JsonMetadataServices.CreatePropertyInfo(options, new JsonPropertyInfoValues<ImmutableArray<GeoJsonObject>>()
                {
                    IsProperty = true,
                    DeclaringType = typeInfo.Type,
                    PropertyName = "FeatureObjects",
                    JsonPropertyName = "features",
                    Getter = o => ((FeatureCollectionBase)o).FeatureObjects,
                    Setter = (o, v) => { },
                });
                featuresProperty.IsRequired = true;
                typeInfo.Properties.Add(featuresProperty);
            }

            static void RegisterGeometryTypes(JsonTypeInfo typeInfo)
            {
                typeInfo.PolymorphismOptions?.DerivedTypes.Add(new JsonDerivedType(typeof(Point), nameof(Point)));
                typeInfo.PolymorphismOptions?.DerivedTypes.Add(new JsonDerivedType(typeof(MultiPoint), nameof(MultiPoint)));
                typeInfo.PolymorphismOptions?.DerivedTypes.Add(new JsonDerivedType(typeof(LineString), nameof(LineString)));
                typeInfo.PolymorphismOptions?.DerivedTypes.Add(new JsonDerivedType(typeof(MultiLineString), nameof(MultiLineString)));
                typeInfo.PolymorphismOptions?.DerivedTypes.Add(new JsonDerivedType(typeof(Polygon), nameof(Polygon)));
                typeInfo.PolymorphismOptions?.DerivedTypes.Add(new JsonDerivedType(typeof(MultiPolygon), nameof(MultiPolygon)));
                typeInfo.PolymorphismOptions?.DerivedTypes.Add(new JsonDerivedType(typeof(GeometryCollection), nameof(GeometryCollection)));
            }
        }
    }

    private const string SuppressJustification = "The _options are only using AOT compatible code.";

    /// <summary>
    /// Serializes the specified <see cref="GeoJsonObject"/> into a JSON string.
    /// </summary>
    /// <param name="geoJsonObject">The <see cref="GeoJsonObject"/> to serialize. Can be <see langword="null"/>.</param>
    /// <returns>A JSON string representation of the <paramref name="geoJsonObject"/>. Returns an empty JSON object ("{}") if
    /// <paramref name="geoJsonObject"/> is <see langword="null"/>.</returns>
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = SuppressJustification)]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = SuppressJustification)]
    public string Serialize(GeoJsonObject? geoJsonObject)
    {
        return JsonSerializer.Serialize(geoJsonObject, _options);
    }

    /// <summary>
    /// Serializes the specified <see cref="GeoJsonObject"/> to a UTF-8 encoded JSON format and writes it to the provided stream.
    /// </summary>
    /// <param name="utf8Json">The stream to which the serialized JSON will be written. Must be writable and not null.</param>
    /// <param name="geoJsonObject">The <see cref="GeoJsonObject"/> to serialize. Can be <see langword="null"/>.</param>
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = SuppressJustification)]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = SuppressJustification)]
    public void Serialize(Stream utf8Json, GeoJsonObject? geoJsonObject)
    {
        JsonSerializer.Serialize(utf8Json, geoJsonObject, _options);
    }

    /// <summary>
    /// Asynchronously serializes the specified <see cref="GeoJsonObject"/> to a UTF-8 encoded JSON format and writes it
    /// to the provided stream.
    /// </summary>
    /// <param name="utf8Json">The stream to which the JSON data will be written. Must be writable and not null.</param>
    /// <param name="geoJsonObject">The <see cref="GeoJsonObject"/> to serialize. Can be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete. Defaults to <see
    /// cref="CancellationToken.None"/>.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous serialization operation.</returns>
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = SuppressJustification)]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = SuppressJustification)]
    public Task SerializeAsync(Stream utf8Json, GeoJsonObject? geoJsonObject, CancellationToken cancellationToken = default)
    {
        return JsonSerializer.SerializeAsync(utf8Json, geoJsonObject, _options, cancellationToken);
    }

    /// <summary>
    /// Deserializes the specified JSON string into a <see cref="GeoJsonObject"/>.
    /// </summary>
    /// <param name="json">A JSON string representing a GeoJSON object. Cannot be <see langword="null"/> or empty.</param>
    /// <returns>A <see cref="GeoJsonObject"/> instance deserialized from the JSON string, or <see langword="null"/>.</returns>
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = SuppressJustification)]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = SuppressJustification)]
    public GeoJsonObject? Deserialize([StringSyntax(StringSyntaxAttribute.Json)] string json)
    {
        return JsonSerializer.Deserialize<GeoJsonObject>(json, _options);
    }

    /// <summary>
    /// Deserializes the specified JSON string into an object of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize. Must derive from <see cref="GeoJsonObject"/>.</typeparam>
    /// <param name="json">The JSON string to deserialize. Must be a valid JSON representation of a <typeparamref name="T"/> object.</param>
    /// <returns>An instance of type <typeparamref name="T"/> or <see langword="null"/>.</returns>
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = SuppressJustification)]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = SuppressJustification)]
    public T? Deserialize<T>([StringSyntax(StringSyntaxAttribute.Json)] string json) where T : GeoJsonObject
    {
        return (T?)JsonSerializer.Deserialize<GeoJsonObject>(json, _options);
    }

    /// <summary>
    /// Deserializes a JSON stream into a <see cref="GeoJsonObject"/> instance.
    /// </summary>
    /// <param name="utf8Json">A <see cref="Stream"/> containing the JSON data encoded in UTF-8.</param>
    /// <returns>A <see cref="GeoJsonObject"/> instance representing the deserialized JSON data, or <see langword="null"/></returns>
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = SuppressJustification)]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = SuppressJustification)]
    public GeoJsonObject? Deserialize(Stream utf8Json)
    {
        return JsonSerializer.Deserialize<GeoJsonObject>(utf8Json, _options);
    }

    /// <summary>
    /// Deserializes the JSON data from the specified stream into an object of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize. Must inherit from <see cref="GeoJsonObject"/>.</typeparam>
    /// <param name="utf8Json">The stream containing the JSON data encoded in UTF-8.</param>
    /// <returns>An instance of type <typeparamref name="T"/> deserialized from the JSON data,  or <see langword="null"/> if the
    /// JSON data is empty or cannot be deserialized.</returns>
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = SuppressJustification)]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = SuppressJustification)]
    public T? Deserialize<T>(Stream utf8Json) where T : GeoJsonObject
    {
        return JsonSerializer.Deserialize<T>(utf8Json, _options);
    }

    /// <summary>
    /// Asynchronously deserializes a UTF-8 encoded JSON stream into a <see cref="GeoJsonObject"/> instance.
    /// </summary>
    /// <param name="utf8Json">The stream containing the UTF-8 encoded JSON data to deserialize.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="ValueTask{TResult}"/> representing the asynchronous operation. The result contains the deserialized
    /// <see cref="GeoJsonObject"/> if successful, or <see langword="null"/>.</returns>
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = SuppressJustification)]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = SuppressJustification)]
    public ValueTask<GeoJsonObject?> DeserializeAsync(Stream utf8Json, CancellationToken cancellationToken = default)
    {
        return JsonSerializer.DeserializeAsync<GeoJsonObject>(utf8Json, _options, cancellationToken);
    }

    /// <summary>
    /// Asynchronously deserializes the JSON data from the specified stream into an object of type <typeparamref
    /// name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize. Must derive from <see cref="GeoJsonObject"/>.</typeparam>
    /// <param name="utf8Json">The stream containing the JSON data encoded in UTF-8.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="ValueTask{T}"/> representing the asynchronous operation. The result contains the deserialized
    /// object of type <typeparamref name="T"/>,  or <see langword="null"/>.</returns>
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = SuppressJustification)]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = SuppressJustification)]
    public ValueTask<T?> DeserializeAsync<T>(Stream utf8Json, CancellationToken cancellationToken = default) where T : GeoJsonObject
    {
        return JsonSerializer.DeserializeAsync<T>(utf8Json, _options, cancellationToken);
    }
}
