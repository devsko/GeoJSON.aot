// Copyright © devsko 2025. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GeoJson;

public partial class Geo<TPosition, TCoordinate>
{
    /// <summary>
    /// Represents a bounding box defined by two geographical positions: the southwest and northeast corners.
    /// </summary>
    /// <remarks>The <see cref="BBox"/> struct is immutable and implements <see cref="IEquatable{BBox}"/>.
    /// It is commonly used to define rectangular regions in geographical or spatial contexts.</remarks>
    public readonly struct BBox : IEquatable<BBox>
    {
        /// <summary>
        /// Gets the southwest corner of the bounding area.
        /// </summary>
        public required TPosition SouthWest { get; init; }

        /// <summary>
        /// Gets the northeast corner of the bounding area.
        /// </summary>
        public required TPosition NorthEast { get; init; }

        /// <summary>
        /// Determines whether the current bounding box is equal to the specified bounding box.
        /// </summary>
        /// <param name="other">The bounding box to compare with the current bounding box.</param>
        /// <returns><see langword="true"/> if the specified bounding box has the same coordinates for  the southwest and
        /// northeast corners as the current bounding box; otherwise, <see langword="false"/>.</returns>
        public bool Equals(BBox other) => SouthWest.Equals(other.SouthWest) && NorthEast.Equals(other.NorthEast);

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is BBox box && Equals(box);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(SouthWest, NorthEast);

        /// <inheritdoc/>
        public override string ToString() => $"[{SouthWest}-{NorthEast}]";

        /// <summary>
        /// Determines whether two <see cref="BBox"/> instances are equal.
        /// </summary>
        /// <param name="left">The first <see cref="BBox"/> instance to compare.</param>
        /// <param name="right">The second <see cref="BBox"/> instance to compare.</param>
        /// <returns><see langword="true"/> if the two <see cref="BBox"/> instances are equal; otherwise, <see
        /// langword="false"/>.</returns>
        public static bool operator ==(BBox left, BBox right) => left.Equals(right);

        /// <summary>
        /// Determines whether two <see cref="BBox"/> instances are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="BBox"/> instance to compare.</param>
        /// <param name="right">The second <see cref="BBox"/> instance to compare.</param>
        /// <returns><see langword="true"/> if the two <see cref="BBox"/> instances are not equal; otherwise, <see
        /// langword="false"/>.</returns>
        public static bool operator !=(BBox left, BBox right) => !(left == right);
    }

    /// <summary>
    /// Provides functionality to convert <see cref="BBox"/> objects to and from JSON.
    /// </summary>
    /// <remarks>This converter serializes a <see cref="BBox"/> as a JSON array containing the coordinates of
    /// the  southwest and northeast corners. It deserializes a JSON array into a <see cref="BBox"/> object,  ensuring
    /// the array contains a valid number of coordinates.</remarks>
    public sealed class BBoxConverter : JsonConverter<BBox>
    {
        /// <inheritdoc/>
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

        /// <inheritdoc/>
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
