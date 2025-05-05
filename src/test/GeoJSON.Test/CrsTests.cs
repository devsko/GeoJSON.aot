// Copyright © devsko 2025
// Licensed under the MIT license.

using System.Text.Json;

using static GeoJSON.Geo<GeoJSON.Geo<double>.Position2D, double>;

namespace GeoJSON.Test;

public static class CrsTests
{
    [Fact]
    public static void UnknownCrsThrows()
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize("{\"type\":\"Unknown\"}", GeoDouble2D.Default.Options.GetTypeInfo(typeof(Crs))));
    }

    [Fact]
    public static void NoCrsRoundtrip()
    {
        string json = JsonSerializer.Serialize(NoCrs.Instance, GeoDouble2D.Default.Options.GetTypeInfo(typeof(Crs)));
        Assert.Equal("null", json);

        NoCrs? deserialized = (NoCrs?)JsonSerializer.Deserialize(json, GeoDouble2D.Default.Options.GetTypeInfo(typeof(Crs)));
        Assert.Equal(NoCrs.Instance, deserialized);
    }

    [Fact]
    public static void NamedCrsRoundtrip()
    {
        NamedCrs namedCrs = new("TheName");
        string json = JsonSerializer.Serialize(namedCrs, GeoDouble2D.Default.Options.GetTypeInfo(typeof(Crs)));
        Assert.Equal("{\"type\":\"name\",\"properties\":{\"name\":\"TheName\"}}", json);

        NamedCrs? deserialized = (NamedCrs?)JsonSerializer.Deserialize(json, GeoDouble2D.Default.Options.GetTypeInfo(typeof(Crs)));
        Assert.NotNull(deserialized);
        Assert.Equal(namedCrs.Name, deserialized.Name);
    }

    [Fact]
    public static void NamedCrsNameRequired()
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize("{\"type\":\"name\"}", GeoDouble2D.Default.Options.GetTypeInfo(typeof(Crs))));
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize("{\"type\":\"name\",\"properties\":null}", GeoDouble2D.Default.Options.GetTypeInfo(typeof(Crs))));
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize("{\"type\":\"name\",\"properties\":{}}", GeoDouble2D.Default.Options.GetTypeInfo(typeof(Crs))));
    }

    [Fact]
    public static void LinkedCrsRoundtrip()
    {
        LinkedCrs linkedCrs = new(new Uri("TheHref", UriKind.Relative), "TheType");
        string json = JsonSerializer.Serialize(linkedCrs, GeoDouble2D.Default.Options.GetTypeInfo(typeof(Crs)));
        Assert.Equal("{\"type\":\"link\",\"properties\":{\"href\":\"TheHref\",\"type\":\"TheType\"}}", json);

        LinkedCrs? deserialized = (LinkedCrs?)JsonSerializer.Deserialize(json, GeoDouble2D.Default.Options.GetTypeInfo(typeof(Crs)));
        Assert.NotNull(deserialized);
        Assert.Equal(linkedCrs.Href, deserialized.Href);
        Assert.Equal(linkedCrs.Type, deserialized.Type);
    }

    [Fact]
    public static void LinkedCrsHrefRequired()
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize("{\"type\":\"link\"}", GeoDouble2D.Default.Options.GetTypeInfo(typeof(Crs))));
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize("{\"type\":\"link\",\"properties\":null}", GeoDouble2D.Default.Options.GetTypeInfo(typeof(Crs))));
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize("{\"type\":\"link\",\"properties\":{}}", GeoDouble2D.Default.Options.GetTypeInfo(typeof(Crs))));
    }
}
