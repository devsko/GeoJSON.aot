// Copyright © devsko 2025
// Licensed under the MIT license.

using System.Text.Json;
using static GeoJSON.Geo<GeoJSON.Geo<double>.Position3D, double>;

namespace GeoJSON.Test;

public static class BBox3DTests
{
    [Fact]
    public static void BBoxRoundtripWithAltitude()
    {
        BBox bbox = new() { SouthWest = new(1, 2, 3), NorthEast = new(4, 5, 6) };

        string json = JsonSerializer.Serialize(bbox, GeoDouble3D.Default.Options.GetTypeInfo(typeof(BBox)));

        Assert.Equal("[1,2,3,4,5,6]", json);

        BBox? deserialized = (BBox?)JsonSerializer.Deserialize(json, GeoDouble3D.Default.Options.GetTypeInfo(typeof(BBox)));

        Assert.Equal(bbox, deserialized);
    }

    [Fact]
    public static void BBoxRoundtripWithoutAltitude()
    {
        BBox bbox = new() { SouthWest = new(1, 2), NorthEast = new(3, 4) };

        string json = JsonSerializer.Serialize(bbox, GeoDouble3D.Default.Options.GetTypeInfo(typeof(BBox)));

        Assert.Equal("[1,2,3,4]", json);

        BBox? deserialized = (BBox?)JsonSerializer.Deserialize(json, GeoDouble3D.Default.Options.GetTypeInfo(typeof(BBox)));

        Assert.Equal(bbox, deserialized);
    }

    [Fact]
    public static void BBoxHas4Or6Coordinates()
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize("[1,1,1]", GeoDouble3D.Default.Options.GetTypeInfo(typeof(BBox))));
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize("[1,1,1,1,1]", GeoDouble3D.Default.Options.GetTypeInfo(typeof(BBox))));
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize("[1,1,1,1,1,1,1]", GeoDouble3D.Default.Options.GetTypeInfo(typeof(BBox))));
    }
}
