// Copyright © devsko 2025. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GeoJson;

public partial class Geo<TPosition, TCoordinate>
{
    /// <summary>
    /// The abstract base class for all Coordinate Reference System (CRS) definitions used in GeoJSON.
    /// </summary>
    public abstract class Crs
    { }

    /// <summary>
    /// Represents a Coordinate Reference System (CRS) that indicates the absence of a CRS.
    /// </summary>
    /// <remarks>This class is a singleton, and the <see cref="Instance"/> property provides the single shared
    /// instance.</remarks>
    public class NoCrs : Crs
    {
        /// <summary>
        /// Represents a singleton instance of the <see cref="NoCrs"/> class.
        /// </summary>
        public static readonly NoCrs Instance = new();

        private NoCrs()
        { }
    }

    /// <summary>
    /// Represents a Coordinate Reference System (CRS) identified by a specific name.
    /// </summary>
    public class NamedCrs(string name) : Crs
    {
        /// <summary>
        /// Gets the name of the CRS.
        /// </summary>
        [JsonPropertyName("name")]
        [JsonRequired]
        public string Name { get; init; } = name ?? throw new ArgumentNullException(nameof(Name));
    }

    /// <summary>
    /// Represents a linked Coordinate Reference System (CRS) with a reference URI.
    /// </summary>
    /// <param name="href">The URI that represents the CRS</param>
    /// <param name="type">The describing type of the CRS (optional)</param>
    public class LinkedCrs(Uri href, string? type = null) : Crs
    {
        /// <summary>
        /// Gets the URI that represents the CRS.
        /// </summary>
        [JsonPropertyName("href")]
        [JsonRequired]
        public Uri Href { get; init; } = href ?? throw new ArgumentNullException(nameof(Href));

        /// <summary>
        /// Gets a string representing the type of the CRS.
        /// </summary>
        [JsonPropertyName("type")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? Type { get; } = type;
    }

    /// <summary>
    /// Provides custom JSON serialization and deserialization logic for <see cref="Crs"/> objects.
    /// </summary>
    /// <remarks>This converter handles the serialization and deserialization of <see cref="Crs"/> instances,
    /// including its derived types, such as <see cref="NamedCrs"/> and <see cref="LinkedCrs"/>. It supports null values
    /// and ensures that the appropriate type-specific converters are used during the serialization process.</remarks>
    public class CrsConverter : JsonConverter<Crs>
    {
        private JsonConverter<NamedCrs>? _namedCrsConverter;
        private JsonConverter<LinkedCrs>? _linkedCrsConverter;

        /// <inheritdoc />
        public override bool HandleNull => true;

        [MemberNotNull(nameof(_namedCrsConverter))]
        [MemberNotNull(nameof(_linkedCrsConverter))]
        private void EnsureConverters(JsonSerializerOptions options)
        {
            _namedCrsConverter ??= (JsonConverter<NamedCrs>)options.GetTypeInfo(typeof(NamedCrs)).Converter;
            _linkedCrsConverter ??= (JsonConverter<LinkedCrs>)options.GetTypeInfo(typeof(LinkedCrs)).Converter;
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
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
