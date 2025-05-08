// Copyright © devsko 2025. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;

using static GeoJson.Geo<GeoJson.Position2D<double>, double>;

namespace GeoJson.Test;

public static partial class SerializerTests
{
    public record struct Properties(string Prop1, int Prop2);

    [JsonSerializable(typeof(Feature<Properties>))]
    [JsonSerializable(typeof(FeatureCollection<Properties>))]
    [JsonSourceGenerationOptions(WriteIndented = true)]
    private sealed partial class SerializerContext : JsonSerializerContext
    { }

    [JsonSerializable(typeof(Properties))]
    private sealed partial class WrongSerializerContext : JsonSerializerContext
    { }

    private static readonly GeoDouble2D _serializerWithOptions = new(new JsonSerializerOptions { WriteIndented = true });
    private static readonly GeoDouble2D _serializerWithAdditionalContext = new(SerializerContext.Default);
    private static readonly GeoDouble2D _serializerWithFeaturePropertiesType = new(SerializerContext.Default, typeof(Properties));

    [Fact]
    public static void DefaultSerializer()
    {
        Feature feature = new(new Point(new(10.5f, -15.5f)));
        feature.Properties!.Add("Prop1", "value1");
        feature.Properties.Add("Prop2", "15");
        string json = GeoDouble2D.Default.Serialize(feature);

        Assert.Equal("""
            {"type":"Feature","geometry":{"type":"Point","coordinates":[10.5,-15.5]},"properties":{"Prop1":"value1","Prop2":"15"}}
            """, json);

        GeoJsonObject? geo = GeoDouble2D.Default.Deserialize(json);

        Assert.NotNull(geo);
        Assert.IsType<Feature>(geo);
        Assert.IsType<Point>(((Feature)geo).Geometry);

        IDictionary<string, object?>? properties = ((Feature)geo).Properties;

        Assert.NotNull(properties);
        Assert.Collection(properties,
            pair => { Assert.Equal("Prop1", pair.Key); Assert.Equal("value1", ((JsonElement?)pair.Value)?.GetString()); },
            pair => { Assert.Equal("Prop2", pair.Key); Assert.Equal("15", ((JsonElement?)pair.Value)?.GetString()); });
    }

    [Fact]
    public static void SerializerWithOptions()
    {
        Feature feature = new(new Point(new(10.5f, -15.5f)));
        feature.Properties!.Add("Prop1", "value1");
        feature.Properties.Add("Prop2", "15");
        string json = _serializerWithOptions.Serialize(feature);

        Assert.Equal("""
            {
              "type": "Feature",
              "geometry": {
                "type": "Point",
                "coordinates": [
                  10.5,
                  -15.5
                ]
              },
              "properties": {
                "Prop1": "value1",
                "Prop2": "15"
              }
            }
            """, json);

        GeoJsonObject? geo = _serializerWithOptions.Deserialize(json);

        Assert.NotNull(geo);
        Assert.IsType<Feature>(geo);
        Assert.IsType<Point>(((Feature)geo).Geometry);

        IDictionary<string, object?>? properties = ((Feature)geo).Properties;

        Assert.NotNull(properties);
        Assert.Collection(properties,
            pair => { Assert.Equal("Prop1", pair.Key); Assert.Equal("value1", ((JsonElement?)pair.Value)?.GetString()); },
            pair => { Assert.Equal("Prop2", pair.Key); Assert.Equal("15", ((JsonElement?)pair.Value)?.GetString()); });
    }

    [Fact]
    public static void SerializerWithAdditionalContext()
    {
        Feature feature = new(new Point(new(10.5f, -15.5f)));
        feature.Properties!.Add("Prop1", "value1");
        feature.Properties.Add("Prop2", 15);
        string json = _serializerWithAdditionalContext.Serialize(feature);

        Assert.Equal("""
            {
              "type": "Feature",
              "geometry": {
                "type": "Point",
                "coordinates": [
                  10.5,
                  -15.5
                ]
              },
              "properties": {
                "Prop1": "value1",
                "Prop2": 15
              }
            }
            """, json);

        GeoJsonObject? geo = _serializerWithAdditionalContext.Deserialize(json);

        Assert.NotNull(geo);
        Assert.IsType<Feature>(geo);
        Assert.IsType<Point>(((Feature)geo).Geometry);

        IDictionary<string, object?>? properties = ((Feature)geo).Properties;

        Assert.NotNull(properties);
        Assert.Collection(properties,
            pair => { Assert.Equal("Prop1", pair.Key); Assert.Equal("value1", ((JsonElement?)pair.Value)?.GetString()); },
            pair => { Assert.Equal("Prop2", pair.Key); Assert.Equal(15, ((JsonElement?)pair.Value)?.GetInt32()); });
    }

    [Fact]
    public static void SerializerWithCustomProperties()
    {
        Feature<Properties> feature = new(new Point(new(10.5f, -15.5f))) { Properties = new() { Prop1 = "value1", Prop2 = 15 } };
        string json = _serializerWithFeaturePropertiesType.Serialize(feature);

        Assert.Equal("""
            {
              "type": "Feature",
              "geometry": {
                "type": "Point",
                "coordinates": [
                  10.5,
                  -15.5
                ]
              },
              "properties": {
                "Prop1": "value1",
                "Prop2": 15
              }
            }
            """, json);

        GeoJsonObject? geo = _serializerWithFeaturePropertiesType.Deserialize(json);

        Assert.NotNull(geo);
        Assert.IsType<Feature<Properties>>(geo);
        Assert.IsType<Point>(((Feature<Properties>)geo).Geometry);

        Properties properties = ((Feature<Properties>)geo).Properties;

        Assert.Equal("value1", properties.Prop1);
        Assert.Equal(15, properties.Prop2);
    }

    [Fact]
    public static void SerializerWithWrongAdditionalThrows()
    {
        Assert.Throws<InvalidOperationException>(() => new GeoDouble2D(WrongSerializerContext.Default, typeof(Properties)));
    }
}
