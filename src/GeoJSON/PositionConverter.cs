// Copyright © devsko 2025
// Licensed under the MIT license.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace GeoJSON;

public partial class Serializer<TPosition>
{
    public class PositionConverter : JsonConverter<TPosition>
    {
        public override TPosition Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartArray && reader.Read())
            {
                Span<double> values = stackalloc double[TPosition.MaxLength];
                int i = 0;
                while (i <= values.Length)
                {
                    if (reader.TokenType == JsonTokenType.EndArray)
                    {
                        if (i >= TPosition.MinLength)
                        {
                            return TPosition.Create(values[..i]);
                        }

                        break;
                    }
                    if (i < TPosition.MaxLength && reader.TryGetDouble(out values[i]) && reader.Read())
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
            writer.WriteStartArray();
            writer.WritePositionValues(value);
            writer.WriteEndArray();
        }
    }
}
