// Copyright © devsko 2025. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;
using GeoJson;

namespace Samples;

using static Geo<Position2D<float>, float>;

public class WithFeatureProperties
{
    private readonly GeoSingle2D _geo = new(MyContext.Default, typeof(MyProperties));

    public void Deserialization(string json)
    {
        Feature<MyProperties>? feature = _geo.Deserialize<Feature<MyProperties>>(json);
        if (feature is not null)
        {
            Console.WriteLine(feature.Properties.Speed);
        }
    }
}

public record struct MyProperties(string Name, int Distance, float Speed);

[JsonSerializable(typeof(Feature<MyProperties>))]
[JsonSerializable(typeof(FeatureCollection<MyProperties>))]
[JsonSourceGenerationOptions(WriteIndented = true)]
public sealed partial class MyContext : JsonSerializerContext
{ }
