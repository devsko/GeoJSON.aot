// Copyright © devsko 2025. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace GeoJSON;

public partial class Geo<TPosition, TCoordinate>
{
    public class PositionConverter : JsonConverter<TPosition>
    {
        public override TPosition Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartArray && reader.Read())
            {
                Span<TCoordinate> coordinates = stackalloc TCoordinate[TPosition.MaxLength];
                int i = 0;
                while (i <= coordinates.Length)
                {
                    if (reader.TokenType == JsonTokenType.EndArray)
                    {
                        if (i >= TPosition.MinLength)
                        {
                            return TPosition.Create(coordinates[..i]);
                        }

                        break;
                    }

                    if (i < TPosition.MaxLength && reader.TryGetCoordinate(ref coordinates[i]) && reader.Read())
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

        public override void Write(Utf8JsonWriter writer, TPosition value, JsonSerializerOptions options)
        {
            Span<TCoordinate> coordinates = stackalloc TCoordinate[TPosition.MaxLength];
            int written = value.GetCoordinates(coordinates);

            writer.WriteStartArray();
            writer.WriteCoordinates(coordinates[..written]);
            writer.WriteEndArray();
        }
    }
}
