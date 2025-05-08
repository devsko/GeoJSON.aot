// Copyright © devsko 2025. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;

using static GeoJson.Geo<GeoJson.Position2D<double>, double>;

namespace GeoJson.Test;

public static partial class FeatureTests
{
    private static GeoDouble2D _serializer = new(FeatureContext.Default, typeof(Properties?));

    [Fact]
    public static void UnknownObjectThrows()
    {
        Assert.Throws<JsonException>(() => GeoDouble2D.Default.Deserialize("{\"type\":\"Unkown\"}"));
    }

    [Fact]
    public static void FeatureRoundtrip()
    {
        Point point = new(new(10, 20));
        Feature feature = new(point);
        feature.Properties!.Add("Prop1", "value1");

        string json = GeoDouble2D.Default.Serialize(feature);

        Assert.Equal("{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[10,20]},\"properties\":{\"Prop1\":\"value1\"}}", json);

        GeoJsonObject? geo = GeoDouble2D.Default.Deserialize(json);

        Assert.True(geo is Feature);
        Assert.NotNull(((Feature)geo).Properties);
        Assert.Collection(((Feature)geo).Properties!, p =>
        {
            Assert.Equal(feature.Properties.First().Key, p.Key);
            Assert.Equal(feature.Properties.First().Value, ((JsonElement?)p.Value)?.GetString());
        });
        Assert.NotNull(((Feature)geo).Geometry);
        Assert.Equal(point.Coordinates, ((Point)((Feature)geo).Geometry!).Coordinates);
    }

    [Fact]
    public static void FeatureOfTRoundtrip()
    {
        Point point = new(new(10, 20));
        Properties properties = new() { Prop1 = "value1", Prop2 = 42 };
        Feature<Properties?> feature = new(point) { Properties = properties };

        string json = _serializer.Serialize(feature);

        Assert.Equal("{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[10,20]},\"properties\":{\"Prop1\":\"value1\",\"Prop2\":42}}", json);

        GeoJsonObject? geo = _serializer.Deserialize(json);

        Assert.True(geo is Feature<Properties?>);
        Assert.NotNull(((Feature<Properties?>)geo).Properties);
        Assert.Equal("value1", ((Feature<Properties?>)geo).Properties!.Value.Prop1);
        Assert.Equal(42, ((Feature<Properties?>)geo).Properties!.Value.Prop2);
        Assert.NotNull(((Feature<Properties?>)geo).Geometry);
        Assert.Equal(point.Coordinates, ((Point)((Feature<Properties?>)geo).Geometry!).Coordinates);
    }

    [Fact]
    public static void FeatureNullableGeometryRequired()
    {
        Assert.Throws<JsonException>(() => GeoDouble2D.Default.Deserialize("{\"type\":\"Feature\",\"properties\":{}}"));
        Assert.Null(((Feature)GeoDouble2D.Default.Deserialize("{\"type\":\"Feature\",\"properties\":{},\"geometry\":null}")!).Geometry);
    }

    [Fact]
    public static void FeatureOfTNullableGeometryRequired()
    {
        Assert.Throws<JsonException>(() => _serializer.Deserialize("{\"type\":\"Feature\",\"properties\":{}}"));
        Assert.Null(((Feature<Properties?>)_serializer.Deserialize("{\"type\":\"Feature\",\"properties\":{},\"geometry\":null}")!).Geometry);
    }

    [Fact]
    public static void FeatureNullablePropertiesRequired()
    {
        Assert.Throws<JsonException>(() => GeoDouble2D.Default.Deserialize("{\"type\":\"Feature\",\"geometry\":null}"));
        Assert.Null(((Feature)GeoDouble2D.Default.Deserialize("{\"type\":\"Feature\",\"properties\":null,\"geometry\":null}")!).Properties);
    }

    [Fact]
    public static void FeatureOfTNullablePropertiesRequired()
    {
        Assert.Throws<JsonException>(() => _serializer.Deserialize("{\"type\":\"Feature\",\"geometry\":null}"));
        Assert.Null(((Feature<Properties?>)_serializer.Deserialize("{\"type\":\"Feature\",\"properties\":null,\"geometry\":null}")!).Properties);
    }

    [Theory]
    [InlineData("id", true)]
    [InlineData(42, true)]
    [InlineData((long)int.MaxValue * 2, true)]
    [InlineData(123.456, true)]
    [InlineData(true, false)]
    [InlineData(typeof(object), false)]
    public static void FeatureIdSerializingStringOrNumber(object id, bool shouldSucceed)
    {
        Feature feature = new(null) { Id = id };
        if (shouldSucceed)
        {
            string json = GeoDouble2D.Default.Serialize(feature);
            Assert.NotEmpty(json);
        }
        else
        {
            Assert.Throws<NotSupportedException>(() => GeoDouble2D.Default.Serialize(feature));
        }
    }

    [Theory]
    [InlineData("\"id\"", typeof(string))]
    [InlineData("42", typeof(int))]
    [InlineData("4294967296", typeof(long))]
    [InlineData("true", null)]
    [InlineData("{}", null)]
    public static void FeatureIdDeserializingStringOrNumber(string id, Type? type)
    {
        string json = $"{{\"type\":\"Feature\",\"geometry\":null,\"properties\":null,\"id\":{id}}}";
        if (type is null)
        {
            Assert.Throws<NotSupportedException>(() => GeoDouble2D.Default.Deserialize(json));
        }
        else
        {
            Assert.Equal(type, ((Feature?)GeoDouble2D.Default.Deserialize(json))!.Id!.GetType());
        }
    }

    [Fact]
    public static void FeatureCollectionRoundtrip()
    {
        Point point = new(new(10, 20));
        Feature feature = new(point);
        feature.Properties!.Add("Prop1", "value1");
        FeatureCollection collection = new([feature]);

        string json = GeoDouble2D.Default.Serialize(collection);

        Assert.Equal("{\"type\":\"FeatureCollection\",\"features\":[{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[10,20]},\"properties\":{\"Prop1\":\"value1\"}}]}", json);

        GeoJsonObject? geo = GeoDouble2D.Default.Deserialize(json);

        Assert.True(geo is FeatureCollection);
        Assert.Collection(((FeatureCollection)geo).Features.First().Properties!, p =>
        {
            Assert.Equal(feature.Properties.First().Key, p.Key);
            Assert.Equal(feature.Properties.First().Value, ((JsonElement?)p.Value)?.GetString());
        });
        Assert.Single(((FeatureCollection)geo).Features);
        Assert.NotNull(((FeatureCollection)geo).Features.First().Geometry);
        Assert.Equal(point.Coordinates, ((Point)((FeatureCollection)geo).Features.First().Geometry!).Coordinates);
    }

    [Fact]
    public static void FeatureCollectionOfTRoundtrip()
    {
        Point point = new(new(10, 20));
        Properties properties = new() { Prop1 = "value1", Prop2 = 42 };
        Feature<Properties?> feature = new(point) { Properties = properties };
        FeatureCollection<Properties?> collection = new([feature]);

        string json = _serializer.Serialize(collection);

        Assert.Equal("{\"type\":\"FeatureCollection\",\"features\":[{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[10,20]},\"properties\":{\"Prop1\":\"value1\",\"Prop2\":42}}]}", json);

        GeoJsonObject? geo = _serializer.Deserialize(json);

        Assert.True(geo is FeatureCollection<Properties?>);
        Assert.Single(((FeatureCollection<Properties?>)geo).Features);
        Assert.NotNull(((FeatureCollection<Properties?>)geo).Features.First().Properties);
        Assert.Equal("value1", ((FeatureCollection<Properties?>)geo).Features.First().Properties!.Value.Prop1);
        Assert.Equal(42, ((FeatureCollection<Properties?>)geo).Features.First().Properties!.Value.Prop2);
        Assert.NotNull(((FeatureCollection<Properties?>)geo).Features.First().Geometry);
        Assert.Equal(point.Coordinates, ((Point)((FeatureCollection<Properties?>)geo).Features.First().Geometry!).Coordinates);
    }

    [Fact]
    public static void FeatureCollectionFeaturesRequired()
    {
        Assert.Throws<JsonException>(() => GeoDouble2D.Default.Deserialize("{\"type\":\"FeatureCollection\"}"));
        Assert.Throws<JsonException>(() => GeoDouble2D.Default.Deserialize("{\"type\":\"FeatureCollection\",\"features\":null}"));
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
        Assert.Throws<ArgumentException>(() => GeoDouble2D.Default.Deserialize("{\"type\":\"FeatureCollection\",\"features\":[{\"type\":\"Point\",\"coordinates\":[10,20]}]}"));
    }

    [Fact]
    public static void FeatureCollectionOfTContainsOnlyFeatures()
    {
        Assert.Throws<ArgumentException>(() => _serializer.Deserialize("{\"type\":\"FeatureCollection\",\"features\":[{\"type\":\"Point\",\"coordinates\":[10,20]}]}"));
    }

    public record struct Properties(string Prop1, int Prop2);

    [JsonSerializable(typeof(Feature<Properties?>))]
    [JsonSerializable(typeof(FeatureCollection<Properties?>))]
    private sealed partial class FeatureContext : JsonSerializerContext
    { }
}
