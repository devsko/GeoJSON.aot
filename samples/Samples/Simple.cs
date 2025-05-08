// Copyright © devsko 2025. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using GeoJson;

namespace Samples;

using static Geo<Position2D<double>, double>;

public static class Simple
{
    public static async Task Deserialization(Stream stream)
    {
        FeatureCollection? collection = await GeoDouble2D.Default.DeserializeAsync<FeatureCollection>(stream);
        if (collection is not null)
        {
            foreach (Feature feature in collection.Features)
            {
                if (feature.Geometry is Point point)
                {
                    Console.WriteLine($"{point.Coordinates.Latitude} / {point.Coordinates.Longitude}");
                }
            }
        }
    }
}
