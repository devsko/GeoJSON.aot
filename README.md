# GeoJson.aot

***A fast and easy-to-use serializer for the GeoJSON format. NativeAOT-compatible and trim safe.***

[![NuGet package](https://img.shields.io/nuget/v/GeoJson.aot.svg)](https://www.nuget.org/packages/GeoJSON.aot)

## Features

* RFC 7946 compliant. Including custom feature property
 types and alternative coordinate reference systems as described in the 2008 specification.
* NativeAOT-compatibel and trim safe.
* Leveraging the `System.Text.Json` source generator under the hood.
* Fast and memory efficient.
* Easy to use but highly configurable™.
* `Feature` and `FeatureCollection` are using properties of type `IDictionary<string, object?>` but can be customized with specialized properties type.
* Choose coordinate precision: `double`, `float`, `Half` or `decimal`
* Choose positions:
  * 2D (without altitude) or
  * 3D (with optional altitude)

# Usage

#### Deserialization from stream (2D positions, double precision)

```cs
using GeoJson;

namespace Samples;

using static Geo<Position2D<double>, double>;

public static class Simple
{
    public static async Task Deserialization(Stream stream)
    {
        FeatureCollection? collection = await GeoDouble2D.Default.DeserializeAsync<FeatureCollection>(stream);
    }
}
```

#### Serialization into stream with customized JSON options (3D positions, single precision)
```cs
using System.Text.Json;
using GeoJson;

namespace Samples;

using static Geo<Position3D<float>, float>;

public class WithOptions
{
    private readonly GeoSingle3D _geo = new(new JsonSerializerOptions { WriteIndented = true });

    public async Task Serialization(Stream stream, MultiPolygon geometry)
    {
        await _geo.SerializeAsync(stream, geometry);
    }
}
```
#### Deserialization from string with specialized feature property type and JSON options (2D positions, single precision)
```cs
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
```
