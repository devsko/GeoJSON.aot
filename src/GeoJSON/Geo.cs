// Copyright © devsko 2025
// Licensed under the MIT license.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace GeoJSON;

public abstract partial class Geo<TPosition, TCoordinate>
    where TPosition : struct, IPosition<TPosition, TCoordinate>
    where TCoordinate : unmanaged, IFloatingPoint<TCoordinate>, IMinMaxValue<TCoordinate>
{
    private readonly JsonSerializerOptions _options;

    protected Geo()
    {
        _options = CreateOptions();
    }

    protected Geo(JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _options = CreateOptions(options: options);
    }

    protected Geo(JsonSerializerContext additional, Type? featurePropertiesType = null)
    {
        ArgumentNullException.ThrowIfNull(additional);

        _options = CreateOptions(additional, featurePropertiesType);
    }

    public JsonSerializerOptions Options => _options;

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
            else if (typeInfo.Type == typeof(LineString))
            {
                // Creating a property to associate with the '_' ctor parameter
                // This way the "at least 2 coordinates" validation is turned of for deseralization
                JsonPropertyInfo dummyProperty = JsonMetadataServices.CreatePropertyInfo<bool>(options, new JsonPropertyInfoValues<bool>()
                {
                    IsProperty = true,
                    DeclaringType = typeof(Geo<Position2D<double>, double>.LineString),
                    IgnoreCondition = JsonIgnoreCondition.Always,
                    PropertyName = "_",
                });
                typeInfo.Properties.Add(dummyProperty);
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

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = SuppressJustification)]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = SuppressJustification)]
    public string Serialize(GeoJsonObject? geoJsonObject)
    {
        return JsonSerializer.Serialize(geoJsonObject, _options);
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = SuppressJustification)]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = SuppressJustification)]
    public void Serialize(Stream utf8Json, GeoJsonObject? geoJsonObject)
    {
        JsonSerializer.Serialize(utf8Json, geoJsonObject, _options);
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = SuppressJustification)]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = SuppressJustification)]
    public Task SerializeAsync(Stream utf8Json, GeoJsonObject? geoJsonObject, CancellationToken cancellationToken = default)
    {
        return JsonSerializer.SerializeAsync(utf8Json, geoJsonObject, _options, cancellationToken);
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = SuppressJustification)]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = SuppressJustification)]
    public GeoJsonObject? Deserialize(string json)
    {
        return JsonSerializer.Deserialize<GeoJsonObject>(json, _options);
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = SuppressJustification)]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = SuppressJustification)]
    public GeoJsonObject? Deserialize(Stream utf8Json)
    {
        return JsonSerializer.Deserialize<GeoJsonObject>(utf8Json, _options);
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = SuppressJustification)]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = SuppressJustification)]
    public ValueTask<GeoJsonObject?> DeserializeAsync(Stream utf8Json, CancellationToken cancellationToken = default)
    {
        return JsonSerializer.DeserializeAsync<GeoJsonObject>(utf8Json, _options, cancellationToken);
    }
}
