// Copyright © devsko 2025. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using BenchmarkDotNet.Attributes;

using static GeoJson.Geo<GeoJson.Position2D<double>, double>;

namespace GeoJson.Benchmark;

[MemoryDiagnoser]
public class DeserializeFeatureCollection
{
    private string _json = null!;

    [Params(100_000)]
    public int N;

    [GlobalSetup]
    public void Setup()
    {
        Console.WriteLine(typeof(DeserializeFeatureCollection).Assembly.Location);

        using Stream stream = typeof(DeserializeFeatureCollection).Assembly.GetManifestResourceStream("GeoJson.Benchmark.FeatureCollection.json")!;
        _json = new StreamReader(stream).ReadToEnd();
    }

    [Benchmark]
    public GeoJSON.Net.Feature.FeatureCollection? DeserializeNewtonsoft() => Newtonsoft.Json.JsonConvert.DeserializeObject<GeoJSON.Net.Feature.FeatureCollection>(_json);


    [Benchmark]
    public GeoJSON.Text.Feature.FeatureCollection? DeserializeSystemTextJson() => System.Text.Json.JsonSerializer.Deserialize<GeoJSON.Text.Feature.FeatureCollection>(_json);

    [Benchmark]
    public FeatureCollection? DeserializeGeoJSON() => (FeatureCollection?)GeoDouble2D.Default.Deserialize(_json);
}
