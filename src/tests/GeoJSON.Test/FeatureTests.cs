// Copyright © devsko 2025
// Licensed under the MIT license.

using System.Text.Json;
using System.Text.Json.Serialization;
using GeoJson;

using static GeoJson.Serializer<GeoJson.Position2D>;

namespace cycloid.Test.GeoJson;

public static partial class FeatureTests
{
    private static Serializer2D _serializer = new(FeatureContext.Default, typeof(Properties));

    [Fact]
    public static void FeatureRoundtrip()
    {
        Point point = new(new(10, 20));
        Dictionary<string, object> properties = new() { ["Prop1"] = "value1" };
        Feature feature = new(point) { Properties = properties };

        string json = Serializer2D.Default.Serialize(feature);

        Assert.Equal("{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[10,20]},\"properties\":{\"Prop1\":\"value1\"}}", json);

        GeoJsonObject? geo = Serializer2D.Default.Deserialize(json);

        Assert.True(geo is Feature);
        Assert.Collection(((Feature)geo).Properties!, p =>
        {
            Assert.Equal(feature.Properties.First().Key, p.Key);
            Assert.Equal(feature.Properties.First().Value, ((JsonElement)p.Value).GetString());
        });
        Assert.Equal(point.Coordinates, ((Point)((Feature)geo).Geometry).Coordinates);
    }

    [Fact]
    public static void FeatureOfTRoundtrip()
    {
        Point point = new(new(10, 20));
        Properties properties = new() { Prop1 = "value1", Prop2 = 42 };
        Feature<Properties> feature = new(point) { Properties = properties };

        string json = _serializer.Serialize(feature);

        Assert.Equal("{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[10,20]},\"properties\":{\"Prop1\":\"value1\",\"Prop2\":42}}", json);

        GeoJsonObject? geo = _serializer.Deserialize(json);

        Assert.True(geo is Feature<Properties>);
        Assert.Equal("value1", ((Feature<Properties>)geo).Properties!.Prop1);
        Assert.Equal(42, ((Feature<Properties>)geo).Properties!.Prop2);
        Assert.Equal(point.Coordinates, ((Point)((Feature<Properties>)geo).Geometry).Coordinates);
    }

    [Fact]
    public static void FeatureGeometryRequired()
    {
        Assert.Throws<JsonException>(() => Serializer2D.Default.Deserialize("{\"type\":\"Feature\"}"));
        Assert.Throws<JsonException>(() => Serializer2D.Default.Deserialize("{\"type\":\"Feature\",\"geometry\":null}"));
    }

    [Fact]
    public static void FeatureOfTGeometryRequired()
    {
        Assert.Throws<JsonException>(() => _serializer.Deserialize("{\"type\":\"Feature\"}"));
        Assert.Throws<JsonException>(() => _serializer.Deserialize("{\"type\":\"Feature\",\"geometry\":null}"));
    }

    [Fact]
    public static void FeatureCollectionRoundtrip()
    {
        Point point = new(new(10, 20));
        Dictionary<string, object> properties = new() { ["Prop1"] = "value1" };
        Feature feature = new(point) { Properties = properties };
        FeatureCollection collection = new([feature]);

        string json = Serializer2D.Default.Serialize(collection);

        Assert.Equal("{\"type\":\"FeatureCollection\",\"features\":[{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[10,20]},\"properties\":{\"Prop1\":\"value1\"}}]}", json);

        GeoJsonObject? geo = Serializer2D.Default.Deserialize(json);

        Assert.True(geo is FeatureCollection);
        Assert.Collection(((FeatureCollection)geo).Features.First().Properties!, p =>
        {
            Assert.Equal(feature.Properties.First().Key, p.Key);
            Assert.Equal(feature.Properties.First().Value, ((JsonElement)p.Value).GetString());
        });
        Assert.Equal(point.Coordinates, ((Point)((FeatureCollection)geo).Features.First().Geometry).Coordinates);
    }

    [Fact]
    public static void FeatureCollectionOfTRoundtrip()
    {
        Point point = new(new(10, 20));
        Properties properties = new() { Prop1 = "value1", Prop2 = 42 };
        Feature<Properties> feature = new(point) { Properties = properties };
        FeatureCollection<Properties> collection = new([feature]);

        string json = _serializer.Serialize(collection);

        Assert.Equal("{\"type\":\"FeatureCollection\",\"features\":[{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[10,20]},\"properties\":{\"Prop1\":\"value1\",\"Prop2\":42}}]}", json);

        GeoJsonObject? geo = _serializer.Deserialize(json);

        Assert.True(geo is FeatureCollection<Properties>);
        Assert.Equal("value1", ((FeatureCollection<Properties>)geo).Features.First().Properties!.Prop1);
        Assert.Equal(42, ((FeatureCollection<Properties>)geo).Features.First().Properties!.Prop2);
        Assert.Equal(point.Coordinates, ((Point)((FeatureCollection<Properties>)geo).Features.First().Geometry).Coordinates);
    }

    [Fact]
    public static void FeatureCollectionFeaturesRequired()
    {
        Assert.Throws<JsonException>(() => Serializer2D.Default.Deserialize("{\"type\":\"FeatureCollection\"}"));
        Assert.Throws<JsonException>(() => Serializer2D.Default.Deserialize("{\"type\":\"FeatureCollection\",\"features\":null}"));
    }

    [Fact]
    public static void FeatureCollectionOfTFeaturesRequired()
    {
        Assert.Throws<JsonException>(() => _serializer.Deserialize("{\"type\":\"FeatureCollection\"}"));
        Assert.Throws<JsonException>(() => _serializer.Deserialize("{\"type\":\"FeatureCollection\",\"features\":null}"));
    }

    [Fact]
    public static void FeatureCollectionContainsOnlyFeatures()
    {
        Assert.Throws<ArgumentException>(() => Serializer2D.Default.Deserialize("{\"type\":\"FeatureCollection\",\"features\":[{\"type\":\"Point\",\"coordinates\":[10,20]}]}"));
    }

    [Fact]
    public static void FeatureCollectionOfTContainsOnlyFeatures()
    {
        Assert.Throws<ArgumentException>(() => _serializer.Deserialize("{\"type\":\"FeatureCollection\",\"features\":[{\"type\":\"Point\",\"coordinates\":[10,20]}]}"));
    }

    public record struct Properties(string Prop1, int Prop2);

    [JsonSerializable(typeof(Feature<Properties>))]
    [JsonSerializable(typeof(FeatureCollection<Properties>))]
    private sealed partial class FeatureContext : JsonSerializerContext
    { }
}
