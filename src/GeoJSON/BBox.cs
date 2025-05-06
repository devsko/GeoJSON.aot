// Copyright © devsko 2025
// Licensed under the MIT license.

using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GeoJSON;

public partial class Geo<TPosition, TCoordinate>
{
    public readonly struct BBox : IEquatable<BBox>
    {
        public required TPosition SouthWest { get; init; }
        public required TPosition NorthEast { get; init; }

        public bool Equals(BBox other) => SouthWest.Equals(other.SouthWest) && NorthEast.Equals(other.NorthEast);

        public override bool Equals(object? obj) => obj is BBox box && Equals(box);

        public override int GetHashCode() => HashCode.Combine(SouthWest, NorthEast);

        public override string ToString() => $"[{SouthWest}-{NorthEast}]";

        public static bool operator ==(BBox left, BBox right) => left.Equals(right);

        public static bool operator !=(BBox left, BBox right) => !(left == right);
    }

    public sealed class BBoxConverter : JsonConverter<BBox>
    {
        public override BBox Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartArray && reader.Read())
            {
                Span<TCoordinate> coordinates = stackalloc TCoordinate[TPosition.MaxLength * 2];
                int i = 0;
                while (i <= coordinates.Length)
                {
                    if (reader.TokenType == JsonTokenType.EndArray)
                    {
                        if (i >= TPosition.MinLength * 2 && i % 2 == 0)
                        {
                            return new BBox
                            {
                                SouthWest = TPosition.Create(coordinates[..(i / 2)]),
                                NorthEast = TPosition.Create(coordinates[(i / 2)..i])
                            };
                        }

                        break;
                    }
                    if (i < TPosition.MaxLength * 2 && reader.TryGetCoordinate(ref coordinates[i]) && reader.Read())
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
            Span<TCoordinate> southWest = stackalloc TCoordinate[TPosition.MaxLength];
            Span<TCoordinate> northEast = stackalloc TCoordinate[TPosition.MaxLength];
            int writtenSW = value.SouthWest.GetCoordinates(southWest);
            int writtenNE = value.NorthEast.GetCoordinates(northEast);

            Debug.Assert(writtenSW == writtenNE);

            writer.WriteStartArray();
            writer.WriteCoordinates(southWest[..writtenSW]);
            writer.WriteCoordinates(northEast[..writtenNE]);
            writer.WriteEndArray();
        }
    }
}
