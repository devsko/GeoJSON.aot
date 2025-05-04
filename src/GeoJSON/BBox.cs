// Copyright © devsko 2025
// Licensed under the MIT license.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace GeoJson;

public partial class Serializer<TPosition>
{
    // The BBox converter cannot be declared by JsonConverterAttribute because of CS0416.
    // It is registered in Serializer<TPosition>.CreateOptions().
    public readonly struct BBox
    {
        public required TPosition SouthWest { get; init; }
        public required TPosition NorthEast { get; init; }
    }

    public sealed class BBoxConverter : JsonConverter<BBox>
    {
        public override BBox Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartArray && reader.Read())
            {
                Span<float> values = stackalloc float[TPosition.MaxLength * 2];
                int i = 0;
                while (i <= values.Length)
                {
                    if (reader.TokenType == JsonTokenType.EndArray)
                    {
                        if (i == TPosition.MinLength * 2)
                        {
                            return new BBox
                            {
                                SouthWest = TPosition.Create(values[..TPosition.MinLength]),
                                NorthEast = TPosition.Create(values[TPosition.MinLength..])
                            };
                        }
                        if (i == TPosition.MaxLength * 2)
                        {
                            return new BBox
                            {
                                SouthWest = TPosition.Create(values[..TPosition.MaxLength]),
                                NorthEast = TPosition.Create(values[TPosition.MaxLength..])
                            };
                        }

                        break;
                    }
                    if (i < TPosition.MaxLength * 2 && reader.TryGetSingle(out values[i]) && reader.Read())
                    {
                        i++;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, BBox value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WritePositionValues(value.SouthWest);
            writer.WritePositionValues(value.NorthEast);
            writer.WriteEndArray();
        }
    }
}
