using System.Text.Json.Serialization;

using static GeoJSON.Geo<GeoJSON.Position<double>.TwoD, double>;

namespace AotTest;

public record struct Properties(string Prop1, int Prop2);

[JsonSerializable(typeof(Feature<Properties>))]
[JsonSerializable(typeof(FeatureCollection<Properties>))]
[JsonSourceGenerationOptions(WriteIndented = true)]
public sealed partial class SerializerContext : JsonSerializerContext
{ }
