// Copyright © devsko 2025. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using GeoJSON;

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
