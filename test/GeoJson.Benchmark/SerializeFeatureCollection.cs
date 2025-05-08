// Copyright © devsko 2025. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


using BenchmarkDotNet.Attributes;

using static GeoJson.Geo<GeoJson.Position2D<double>, double>;

namespace GeoJson.Benchmark;

[MemoryDiagnoser]
public class SerializeFeatureCollection
{
    private readonly GeoJSON.Net.Feature.FeatureCollection _geoJsonNetFeatureCollection = new();
    private readonly GeoJSON.Text.Feature.FeatureCollection _geoJsonTextFeatureCollection = new();
    private FeatureCollection _geoJsonFeatureCollection = null!;

    [Params(100_000)]
    public int N;

    [GlobalSetup]
    public void Setup()
    {
        Feature[] features = new Feature[N];

        double[] coordinates1 = [10, 50];
        double[] coordinates2 = [10, 50];
        double[] coordinates3 = [10, 50];
        double[] coordinates4 = [10, 50];
        List<IEnumerable<double>> line = [coordinates1, coordinates2, coordinates3, coordinates4];

        for (int i = 0; i < N; i++)
        {
            GeoJSON.Net.Geometry.LineString linestringNET = new(line);
            GeoJSON.Net.Feature.Feature featureNET = new(linestringNET);
            _geoJsonNetFeatureCollection.Features.Add(featureNET);

            GeoJSON.Text.Geometry.LineString linestringTEXT = new(line);
            GeoJSON.Text.Feature.Feature featureTEXT = new(linestringTEXT);
            _geoJsonTextFeatureCollection.Features.Add(featureTEXT);

            features[i] = new Feature(new LineString([new(10, 50), new(10, 50), new(10, 50), new(10, 50)]));
        }
        _geoJsonFeatureCollection = new FeatureCollection(features);
    }

    [Benchmark]
    public string SerializeNewtonsoft() => Newtonsoft.Json.JsonConvert.SerializeObject(_geoJsonNetFeatureCollection);

    [Benchmark]
    public string SerializeSystemTextJson() => System.Text.Json.JsonSerializer.Serialize(_geoJsonTextFeatureCollection);

    [Benchmark]
    public string SerializeGeoJson() => GeoDouble2D.Default.Serialize(_geoJsonFeatureCollection);
}
