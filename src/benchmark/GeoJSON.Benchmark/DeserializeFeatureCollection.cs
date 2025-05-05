// Copyright © devsko 2025
// Licensed under the MIT license.


using System.Reflection;
using BenchmarkDotNet.Attributes;

using static GeoJSON.Geo<GeoJSON.Geo<double>.Position2D, double>;

namespace GeoJSON.Benchmark
{
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

            using Stream stream = typeof(DeserializeFeatureCollection).Assembly.GetManifestResourceStream("GeoJSON.Benchmark.FeatureCollection.json")!;
            _json = new StreamReader(stream).ReadToEnd();
        }

        [Benchmark]
        public Net.Feature.FeatureCollection? DeserializeNewtonsoft() => Newtonsoft.Json.JsonConvert.DeserializeObject<Net.Feature.FeatureCollection>(_json);


        [Benchmark]
        public Text.Feature.FeatureCollection? DeserializeSystemTextJson() => System.Text.Json.JsonSerializer.Deserialize<Text.Feature.FeatureCollection>(_json);

        [Benchmark]
        public FeatureCollection? DeserializeGeoJSON() => (FeatureCollection?)GeoDouble2D.Default.Deserialize(_json);
    }
}
