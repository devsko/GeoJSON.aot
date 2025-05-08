// Copyright © devsko 2025. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GeoJSON;

public partial class Geo<TPosition, TCoordinate>
{
    public abstract class Crs
    { }

    public class NoCrs : Crs
    {
        public static readonly NoCrs Instance = new();

        private NoCrs()
        { }
    }

    public class NamedCrs(string name) : Crs
    {
        [JsonPropertyName("name")]
        [JsonRequired]
        public string Name { get; init; } = name ?? throw new ArgumentNullException(nameof(Name));
    }

    public class LinkedCrs(Uri href, string? type = null) : Crs
    {
        [JsonPropertyName("href")]
        [JsonRequired]
        public Uri Href { get; init; } = href ?? throw new ArgumentNullException(nameof(Href));

        [JsonPropertyName("type")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? Type { get; } = type;
    }

    public class CrsConverter : JsonConverter<Crs>
    {
        private JsonConverter<NamedCrs>? _namedCrsConverter;
        private JsonConverter<LinkedCrs>? _linkedCrsConverter;

        public override bool HandleNull => true;

        [MemberNotNull(nameof(_namedCrsConverter))]
        [MemberNotNull(nameof(_linkedCrsConverter))]
        private void EnsureConverters(JsonSerializerOptions options)
        {
            _namedCrsConverter ??= (JsonConverter<NamedCrs>)options.GetTypeInfo(typeof(NamedCrs)).Converter;
            _linkedCrsConverter ??= (JsonConverter<LinkedCrs>)options.GetTypeInfo(typeof(LinkedCrs)).Converter;
        }

        public override Crs? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return NoCrs.Instance;
            }
            if (reader.TokenType == JsonTokenType.StartObject &&
                reader.Read() && reader.TokenType == JsonTokenType.PropertyName && reader.ValueTextEquals("type"u8) &&
                reader.Read() && reader.TokenType == JsonTokenType.String)
            {
                EnsureConverters(options);

                Crs? crs = null;
                if (reader.ValueTextEquals("name"u8))
                {
                    if (TryReadStartProperties(ref reader))
                    {
                        crs = _namedCrsConverter.Read(ref reader, typeof(NamedCrs), options);
                    }
                }
                else if (reader.ValueTextEquals("link"u8))
                {
                    if (TryReadStartProperties(ref reader))
                    {
                        crs = _linkedCrsConverter.Read(ref reader, typeof(LinkedCrs), options);
                    }
                }

                if (crs is not null && reader.Read() && reader.TokenType == JsonTokenType.EndObject)
                {
                    return crs;
                }
            }

            throw new JsonException();

            static bool TryReadStartProperties(ref Utf8JsonReader reader)
            {
                return reader.Read() && reader.TokenType == JsonTokenType.PropertyName && reader.ValueTextEquals("properties"u8) &&
                    reader.Read();
            }
        }

        public override void Write(Utf8JsonWriter writer, Crs value, JsonSerializerOptions options)
        {
            Debug.Assert(value is not null);

            if (value is NoCrs)
            {
                writer.WriteNullValue();
            }
            else
            {
                EnsureConverters(options);

                writer.WriteStartObject();
                if (value is NamedCrs named)
                {
                    writer.WriteString("type"u8, "name"u8);
                    writer.WritePropertyName("properties"u8);
                    _namedCrsConverter.Write(writer, named, options);
                }
                else if (value is LinkedCrs linked)
                {
                    writer.WriteString("type"u8, "link"u8);
                    writer.WritePropertyName("properties"u8);
                    _linkedCrsConverter.Write(writer, linked, options);
                }
                else
                {
                    throw new NotSupportedException();
                }
                writer.WriteEndObject();
            }
        }
    }
}
