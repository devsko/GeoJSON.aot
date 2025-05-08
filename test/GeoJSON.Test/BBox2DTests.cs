// Copyright © devsko 2025. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

using static GeoJSON.Geo<GeoJSON.Position2D<double>, double>;

namespace GeoJSON.Test;

public static class BBox2DTests
{
    [Fact]
    public static void BBoxRoundtrip()
    {
        BBox bbox = new() { SouthWest = new(1, 2), NorthEast = new(3, 4) };

        string json = JsonSerializer.Serialize(bbox, GeoDouble2D.Default.Options.GetTypeInfo(typeof(BBox)));

        Assert.Equal("[1,2,3,4]", json);

        BBox? deserialized = (BBox?)JsonSerializer.Deserialize(json, GeoDouble2D.Default.Options.GetTypeInfo(typeof(BBox)));

        Assert.Equal(bbox, deserialized);
    }

    [Fact]
    public static void BBoxHasExactly4Coordinates()
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize("[1,1,1]", GeoDouble2D.Default.Options.GetTypeInfo(typeof(BBox))));
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize("[1,1,1,1,1]", GeoDouble2D.Default.Options.GetTypeInfo(typeof(BBox))));
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize("[1,1,1,1,1,1]", GeoDouble2D.Default.Options.GetTypeInfo(typeof(BBox))));
    }
}
