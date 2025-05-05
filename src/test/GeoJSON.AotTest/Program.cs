
using System.Text.Json;
using AotTest;
using GeoJSON;

using static GeoJSON.Geo<GeoJSON.Position2D<double>, double>;

internal class Program
{
    private static void Main(string[] args)
    {
        GeoDouble2D serializer = new(SerializerContext.Default, typeof(Properties));

        Point point = new Point(new(10.5f, -15.5f)) { BBox = new() { SouthWest = new(-30, 20), NorthEast = new(10, 20) } };
        Feature<Properties> feature = new(point) { Properties = new() { Prop1 = "value1", Prop2 = 15 } };
        FeatureCollection<Properties> collection = new([feature]);

        string json = serializer.Serialize(collection);

        Console.WriteLine(json);

        GeoJsonObject? geo = serializer.Deserialize(json);

        Console.WriteLine(geo?.ToString());

        BBox bbox = new() { SouthWest = new(1, 2), NorthEast = new(3, 4) };

        json = JsonSerializer.Serialize(bbox, serializer.Options.GetTypeInfo(typeof(BBox)));

        Console.WriteLine(json);
    }
}
