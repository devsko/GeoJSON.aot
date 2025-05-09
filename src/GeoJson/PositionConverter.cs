// Copyright © devsko 2025. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace GeoJson;

public partial class Geo<TPosition, TCoordinate>
{
    /// <summary>
    /// Provides custom JSON serialization and deserialization for <typeparamref name="TPosition"/> objects.
    /// </summary>
    /// <remarks>This converter handles <typeparamref name="TPosition"/> objects by serializing them as JSON
    /// arrays of coordinates and deserializing JSON arrays back into <typeparamref name="TPosition"/> instances. The
    /// <typeparamref name="TPosition"/> type must support the creation of instances from a span of coordinates and
    /// provide a method to retrieve its coordinates.</remarks>
    public class PositionConverter : JsonConverter<TPosition>
    {
        ///<inheritdoc/>
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

        ///<inheritdoc/>
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
