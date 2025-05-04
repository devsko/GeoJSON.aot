// Copyright © devsko 2025
// Licensed under the MIT license.

using System.Text.Json;
using System.Text.Json.Serialization;
using GeoJson;

using static GeoJson.Serializer<GeoJson.Position2D>;

namespace cycloid.Test.GeoJson;

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

    private static readonly Serializer2D _serializerWithOptions = new(new JsonSerializerOptions { WriteIndented = true });
    private static readonly Serializer2D _serializerWithAdditionalContext = new(SerializerContext.Default);
    private static readonly Serializer2D _serializerWithFeaturePropertiesType = new(SerializerContext.Default, typeof(Properties));

    [Fact]
    public static void DefaultSerializer()
    {
        Feature feature = new(new Point(new(10.5f, -15.5f))) { Properties = new() { ["Prop1"] = "value1", ["Prop2"] = "15" } };
        string json = Serializer2D.Default.Serialize(feature);

        Assert.Equal("""
            {"type":"Feature","geometry":{"type":"Point","coordinates":[10.5,-15.5]},"properties":{"Prop1":"value1","Prop2":"15"}}
            """, json);

        GeoJsonObject? geo = Serializer2D.Default.Deserialize(json);

        Assert.NotNull(geo);
        Assert.IsType<Feature>(geo);
        Assert.IsType<Point>(((Feature)geo).Geometry);

        Dictionary<string, object>? properties = ((Feature)geo).Properties;

        Assert.NotNull(properties);
        Assert.Collection(properties,
            pair => { Assert.Equal("Prop1", pair.Key); Assert.Equal("value1", ((JsonElement)pair.Value).GetString()); },
            pair => { Assert.Equal("Prop2", pair.Key); Assert.Equal("15", ((JsonElement)pair.Value).GetString()); });
    }

    [Fact]
    public static void SerializerWithOptions()
    {
        Feature feature = new(new Point(new(10.5f, -15.5f))) { Properties = new() { ["Prop1"] = "value1", ["Prop2"] = "15" } };
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

        Dictionary<string, object>? properties = ((Feature)geo).Properties;

        Assert.NotNull(properties);
        Assert.Collection(properties,
            pair => { Assert.Equal("Prop1", pair.Key); Assert.Equal("value1", ((JsonElement)pair.Value).GetString()); },
            pair => { Assert.Equal("Prop2", pair.Key); Assert.Equal("15", ((JsonElement)pair.Value).GetString()); });
    }

    [Fact]
    public static void SerializerWithAdditionalContext()
    {
        Feature feature = new(new Point(new(10.5f, -15.5f))) { Properties = new() { ["Prop1"] = "value1", ["Prop2"] = 15 } };
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

        Dictionary<string, object>? properties = ((Feature)geo).Properties;

        Assert.NotNull(properties);
        Assert.Collection(properties,
            pair => { Assert.Equal("Prop1", pair.Key); Assert.Equal("value1", ((JsonElement)pair.Value).GetString()); },
            pair => { Assert.Equal("Prop2", pair.Key); Assert.Equal(15, ((JsonElement)pair.Value).GetInt32()); });
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
        Assert.Throws<InvalidOperationException>(() => new Serializer2D(WrongSerializerContext.Default, typeof(Properties)));
    }
}
