// Copyright © devsko 2025. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace GeoJSON;

internal static class ImmutableArrayExtensions
{
    public static ImmutableArray<T> EnsureNotDefault<T>(this ImmutableArray<T> array, [CallerArgumentExpression(nameof(array))] string? paramName = null)
    {
        return array.IsDefault ? throw new ArgumentNullException(paramName) : array;
    }
}

internal static class UTf8JsonWriterExtensions
{
    public static void WriteCoordinates<TCoordinate>(this Utf8JsonWriter writer, ReadOnlySpan<TCoordinate> coordinates) where TCoordinate : unmanaged, IFloatingPoint<TCoordinate>, IMinMaxValue<TCoordinate>
    {
        foreach (TCoordinate coordinate in coordinates)
        {
            if (coordinate == TCoordinate.MinValue)
            {
                break;
            }

            if (typeof(TCoordinate) == typeof(float))
            {
                writer.WriteNumberValue(float.CreateTruncating(coordinate));
            }
            else if (typeof(TCoordinate) == typeof(double))
            {
                writer.WriteNumberValue(float.CreateTruncating(coordinate));
            }
            else if (typeof(TCoordinate) == typeof(Half))
            {
                writer.WriteNumberValue(float.CreateTruncating(coordinate));
            }
            else if (typeof(TCoordinate) == typeof(decimal))
            {
                writer.WriteNumberValue(float.CreateTruncating(coordinate));
            }
            else
            {
                throw new UnreachableException();
            }
        }
    }
}

internal static class Utf8ReaderExtensions
{
    public static bool TryGetCoordinate<TCoordinate>(ref this Utf8JsonReader reader, ref TCoordinate coordinate) where TCoordinate : unmanaged, IFloatingPoint<TCoordinate>, IMinMaxValue<TCoordinate>
    {
        return typeof(TCoordinate) == typeof(float)
            ? reader.TryGetSingle(out Unsafe.As<TCoordinate, float>(ref coordinate))
            : typeof(TCoordinate) == typeof(double)
            ? reader.TryGetDouble(out Unsafe.As<TCoordinate, double>(ref coordinate))
            : typeof(TCoordinate) == typeof(Half)
            ? reader.TryGetHalf(out Unsafe.As<TCoordinate, Half>(ref coordinate))
            : typeof(TCoordinate) == typeof(decimal)
            ? reader.TryGetDecimal(out Unsafe.As<TCoordinate, decimal>(ref coordinate))
            : throw new UnreachableException();
    }

    public static bool TryGetHalf(ref this Utf8JsonReader reader, out Half value)
    {
        if (reader.TokenType != JsonTokenType.Number)
        {
            throw new InvalidOperationException();
        }

        ReadOnlySpan<byte> span = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan;

        if (Half.TryParse(span, NumberFormatInfo.InvariantInfo, out Half tmp))
        {
            value = tmp;
            return true;
        }

        value = default;
        return false;
    }
}
