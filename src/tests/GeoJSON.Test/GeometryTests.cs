// Copyright © devsko 2025
// Licensed under the MIT license.

using System.Collections;
using System.Collections.Immutable;
using System.Text.Json;
using GeoJson;

using static GeoJson.Serializer<GeoJson.Position2D>;

namespace cycloid.Test.GeoJson;

public static class GeometryTests
{
    [Fact]
    public static void UnknownTypeThrows()
    {
        Assert.Throws<JsonException>(() => Serializer2D.Default.Deserialize("{\"type\":\"Unkown\"}"));
    }

    [Fact]
    public static void PointRoundtrip()
    {
        Point point = new(new(10, 20));

        string json = Serializer2D.Default.Serialize(point);

        Assert.Equal("{\"type\":\"Point\",\"coordinates\":[10,20]}", json);

        GeoJsonObject? geo = Serializer2D.Default.Deserialize(json);

        Assert.True(geo is Point);
        Assert.Equal(point.Coordinates, ((Point)geo).Coordinates);
    }

    [Fact]
    public static void PointCoordinatesRequired()
    {
        Assert.Throws<JsonException>(() => Serializer2D.Default.Deserialize("{\"type\":\"Point\"}"));
        Assert.Throws<JsonException>(() => Serializer2D.Default.Deserialize("{\"type\":\"Point\",\"coordinates\":null}"));
    }

    [Fact]
    public static void MulitPointRoundtrip()
    {
        MultiPoint multiPoint = new([new(10, 20)]);

        string json = Serializer2D.Default.Serialize(multiPoint);

        Assert.Equal("{\"type\":\"MultiPoint\",\"coordinates\":[[10,20]]}", json);

        GeoJsonObject? geo = Serializer2D.Default.Deserialize(json);

        Assert.True(geo is MultiPoint);
        CoordinatesEqual<Position2D>(multiPoint.Coordinates, ((MultiPoint)geo).Coordinates);
    }

    [Fact]
    public static void MultiPointCoordinatesRequired()
    {
        Assert.Throws<JsonException>(() => Serializer2D.Default.Deserialize("{\"type\":\"MultiPoint\"}"));
        Assert.Throws<JsonException>(() => Serializer2D.Default.Deserialize("{\"type\":\"MultiPoint\",\"coordinates\":null}"));
    }

    [Fact]
    public static void LineStringRoundtrip()
    {
        LineString line = new([new(10, 20), new(20, 30), new(30, 40)]);

        string json = Serializer2D.Default.Serialize(line);

        Assert.Equal("{\"type\":\"LineString\",\"coordinates\":[[10,20],[20,30],[30,40]]}", json);

        GeoJsonObject? geo = Serializer2D.Default.Deserialize(json);

        Assert.True(geo is LineString);
        CoordinatesEqual<Position2D>(line.Coordinates, ((LineString)geo).Coordinates);
    }

    [Fact]
    public static void LineStringCoordinatesRequired()
    {
        Assert.Throws<JsonException>(() => Serializer2D.Default.Deserialize("{\"type\":\"LineString\"}"));
        Assert.Throws<JsonException>(() => Serializer2D.Default.Deserialize("{\"type\":\"LineString\",\"coordinates\":null}"));
    }

    [Fact]
    public static void LineStringAtLeast2PositionsRequired()
    {
        Assert.Throws<ArgumentException>(() => Serializer2D.Default.Deserialize("{\"type\":\"LineString\",\"coordinates\":[[10,20]]}"));
    }

    [Fact]
    public static void MultiLineStringRoundtrip()
    {
        MultiLineString multiLine = new([[new(10, 20), new(20, 30), new(30, 40)], [new(-10, -20), new(-20, -30)], [new(50, 60), new(60, 70)]]);

        string json = Serializer2D.Default.Serialize(multiLine);

        Assert.Equal("{\"type\":\"MultiLineString\",\"coordinates\":[[[10,20],[20,30],[30,40]],[[-10,-20],[-20,-30]],[[50,60],[60,70]]]}", json);

        GeoJsonObject? geo = Serializer2D.Default.Deserialize(json);

        Assert.True(geo is MultiLineString);
        CoordinatesEqual<Position2D>(multiLine.Coordinates, ((MultiLineString)geo).Coordinates);
    }

    [Fact]
    public static void MultiLineStringCoordinatesRequired()
    {
        Assert.Throws<JsonException>(() => Serializer2D.Default.Deserialize("{\"type\":\"MultiLineString\"}"));
        Assert.Throws<JsonException>(() => Serializer2D.Default.Deserialize("{\"type\":\"MultiLineString\",\"coordinates\":null}"));
    }

    [Fact]
    public static void MultiLineStringAtLeast2PositionsRequired()
    {
        Assert.Throws<ArgumentException>(() => Serializer2D.Default.Deserialize("{\"type\":\"MultiLineString\",\"coordinates\":[[[10,20]]]}"));
    }

    [Fact]
    public static void PolygonRoundtrip()
    {
        Polygon polygon = new([[new(10, 20), new(20, 30), new(30, 40), new(10, 20)], [new(-10, -20), new(-20, -30), new(-30, -40), new(-10, -20)]]);

        string json = Serializer2D.Default.Serialize(polygon);

        Assert.Equal("{\"type\":\"Polygon\",\"coordinates\":[[[10,20],[20,30],[30,40],[10,20]],[[-10,-20],[-20,-30],[-30,-40],[-10,-20]]]}", json);

        GeoJsonObject? geo = Serializer2D.Default.Deserialize(json);

        Assert.True(geo is Polygon);
        CoordinatesEqual<Position2D>(polygon.Coordinates, ((Polygon)geo).Coordinates);
    }

    [Fact]
    public static void PolygonCoordinatesRequired()
    {
        Assert.Throws<JsonException>(() => Serializer2D.Default.Deserialize("{\"type\":\"Polygon\"}"));
        Assert.Throws<JsonException>(() => Serializer2D.Default.Deserialize("{\"type\":\"Polygon\",\"coordinates\":null}"));
    }

    [Fact]
    public static void PolygonAtLeast4PositionsRequired()
    {
        Assert.Throws<ArgumentException>(() => Serializer2D.Default.Deserialize("{\"type\":\"Polygon\",\"coordinates\":[[[10,20],[20,30],[10,20]]]}"));
    }

    [Fact]
    public static void PolygonClosedPositionsRequired()
    {
        Assert.Throws<ArgumentException>(() => Serializer2D.Default.Deserialize("{\"type\":\"Polygon\",\"coordinates\":[[[10,20],[20,30],[30,40],[40,50]]]}"));
    }

    [Fact]
    public static void MultiPolygonStringRoundtrip()
    {
        MultiPolygon multiPolygon = new([[[new(10, 20), new(20, 30), new(30, 40), new(10, 20)], [new(-10, -20), new(-20, -30), new(-30, -40), new(-10, -20)]], [[new(10, 20), new(20, 30), new(30, 40), new(10, 20)], [new(-10, -20), new(-20, -30), new(-30, -40), new(-10, -20)]]]);

        string json = Serializer2D.Default.Serialize(multiPolygon);

        Assert.Equal("{\"type\":\"MultiPolygon\",\"coordinates\":[[[[10,20],[20,30],[30,40],[10,20]],[[-10,-20],[-20,-30],[-30,-40],[-10,-20]]],[[[10,20],[20,30],[30,40],[10,20]],[[-10,-20],[-20,-30],[-30,-40],[-10,-20]]]]}", json);

        GeoJsonObject? geo = Serializer2D.Default.Deserialize(json);

        Assert.True(geo is MultiPolygon);
        CoordinatesEqual<Position2D>(multiPolygon.Coordinates, ((MultiPolygon)geo).Coordinates);
    }

    [Fact]
    public static void MultiPolygonCoordinatesRequired()
    {
        Assert.Throws<JsonException>(() => Serializer2D.Default.Deserialize("{\"type\":\"MultiPolygon\"}"));
        Assert.Throws<JsonException>(() => Serializer2D.Default.Deserialize("{\"type\":\"MultiPolygon\",\"coordinates\":null}"));
    }

    [Fact]
    public static void MultiPolygonAtLeast4PositionsRequired()
    {
        Assert.Throws<ArgumentException>(() => Serializer2D.Default.Deserialize("{\"type\":\"MultiPolygon\",\"coordinates\":[[[[10,20],[20,30],[10,20]]]]}"));
    }

    [Fact]
    public static void MultiPolygonClosedPositionsRequired()
    {
        Assert.Throws<ArgumentException>(() => Serializer2D.Default.Deserialize("{\"type\":\"MultiPolygon\",\"coordinates\":[[[[10,20],[20,30],[30,40],[40,50]]]]}"));
    }

    [Fact]
    public static void GeometryCollectionRoundtrip()
    {
        Point point = new(new(10, 20));
        LineString line = new([new(10, 20), new(20, 30), new(30, 40)]);
        MultiLineString multiLine = new([[new(10, 20), new(20, 30), new(30, 40)], [new(-10, -20), new(-20, -30)], [new(50, 60), new(60, 70)]]);
        Polygon polygon = new([[new(10, 20), new(20, 30), new(30, 40), new(10, 20)], [new(-10, -20), new(-20, -30), new(-30, -40), new(-10, -20)]]);
        MultiPolygon multiPolygon = new([[[new(10, 20), new(20, 30), new(30, 40), new(10, 20)], [new(-10, -20), new(-20, -30), new(-30, -40), new(-10, -20)]], [[new(10, 20), new(20, 30), new(30, 40), new(10, 20)], [new(-10, -20), new(-20, -30), new(-30, -40), new(-10, -20)]]]);
        GeometryCollection nestedCollection = new([]);

        GeometryCollection collection = new([point, line, multiLine, polygon, multiPolygon, nestedCollection]);

        string json = Serializer2D.Default.Serialize(collection);

        Assert.Equal("{\"type\":\"GeometryCollection\",\"geometries\":[{\"type\":\"Point\",\"coordinates\":[10,20]},{\"type\":\"LineString\",\"coordinates\":[[10,20],[20,30],[30,40]]},{\"type\":\"MultiLineString\",\"coordinates\":[[[10,20],[20,30],[30,40]],[[-10,-20],[-20,-30]],[[50,60],[60,70]]]},{\"type\":\"Polygon\",\"coordinates\":[[[10,20],[20,30],[30,40],[10,20]],[[-10,-20],[-20,-30],[-30,-40],[-10,-20]]]},{\"type\":\"MultiPolygon\",\"coordinates\":[[[[10,20],[20,30],[30,40],[10,20]],[[-10,-20],[-20,-30],[-30,-40],[-10,-20]]],[[[10,20],[20,30],[30,40],[10,20]],[[-10,-20],[-20,-30],[-30,-40],[-10,-20]]]]},{\"type\":\"GeometryCollection\",\"geometries\":[]}]}", json);

        GeoJsonObject? geo = Serializer2D.Default.Deserialize(json);
        Assert.True(geo is GeometryCollection);
        Assert.Collection(((GeometryCollection)geo).Geometries,
            g => CoordinatesEqual<Position2D>(point.Coordinates, ((Point)g).Coordinates),
            g => CoordinatesEqual<Position2D>(line.Coordinates, ((LineString)g).Coordinates),
            g => CoordinatesEqual<Position2D>(multiLine.Coordinates, ((MultiLineString)g).Coordinates),
            g => CoordinatesEqual<Position2D>(polygon.Coordinates, ((Polygon)g).Coordinates),
            g => CoordinatesEqual<Position2D>(multiPolygon.Coordinates, ((MultiPolygon)g).Coordinates),
            g => CoordinatesEqual<Position2D>(nestedCollection.Geometries, ((GeometryCollection)g).Geometries));
    }

    [Fact]
    public static void GeometryCollectoinGeometriesRequired()
    {
        Assert.Throws<JsonException>(() => Serializer2D.Default.Deserialize("{\"type\":\"GeometryCollection\"}"));
        Assert.Throws<JsonException>(() => Serializer2D.Default.Deserialize("{\"type\":\"GeometryCollection\",\"geometries\":null}"));
    }

    private static void CoordinatesEqual<TPosition>(object? left, object? right) where TPosition : struct, IPosition<TPosition>
    {
        if (!Equal(left, right))
        {
            Assert.Fail("Coordinates not equal");
        }

        bool Equal(object? left, object? right)
        {
            if (left is TPosition leftPos && right is TPosition rightPos)
            {
                return leftPos.Equals(rightPos);
            }
            if (left is IList leftList && right is IList rightList)
            {
                if (leftList.Count != rightList.Count)
                {
                    return false;
                }
                for (int i = 0; i < leftList.Count; i++)
                {
                    if (!Equal(leftList[i], rightList[i]))
                    {
                        return false;
                    }
                }
                return true;
            }

            return left is null && right is null;
        }
    }
}
