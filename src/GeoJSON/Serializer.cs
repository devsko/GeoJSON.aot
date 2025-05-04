// Copyright © devsko 2025
// Licensed under the MIT license.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace GeoJson;

public abstract partial class Serializer<TPosition> where TPosition : struct, IPosition<TPosition>
{
    private readonly JsonSerializerOptions _options;

    protected Serializer()
    {
        _options = CreateOptions();
    }

    protected Serializer(JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _options = CreateOptions(options: options);
    }

    protected Serializer(JsonSerializerContext additional, Type? featurePropertiesType = null)
    {
        ArgumentNullException.ThrowIfNull(additional);

        _options = CreateOptions(additional, featurePropertiesType);
    }

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
        options = new(options ?? additional?.Options ?? BaseContext.Options)
        {
            RespectNullableAnnotations = true,
            RespectRequiredConstructorParameters = true,
            TypeInfoResolver = JsonTypeInfoResolver
                .Combine(BaseContext, additional)
                .WithAddedModifier(RegisterImplementations)
        };
        options.Converters.Add(new BBoxConverter());
        options.Converters.Add(new CrsConverter());

        if (featurePropertiesType is not null)
        {
            featureTypes = (typeof(Feature<>).MakeGenericType(typeof(TPosition), featurePropertiesType),
                typeof(FeatureCollection<>).MakeGenericType(typeof(TPosition), featurePropertiesType));

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
}
