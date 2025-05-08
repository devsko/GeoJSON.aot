// Copyright © devsko 2025. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

using static GeoJson.Geo<GeoJson.Position2D<double>, double>;

namespace AotTest;

public record struct Properties(string Prop1, int Prop2);

[JsonSerializable(typeof(Feature<Properties>))]
[JsonSerializable(typeof(FeatureCollection<Properties>))]
[JsonSourceGenerationOptions(WriteIndented = true)]
public sealed partial class SerializerContext : JsonSerializerContext
{ }
