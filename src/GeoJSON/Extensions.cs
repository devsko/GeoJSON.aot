// Copyright © devsko 2025
// Licensed under the MIT license.

using System.Collections.Immutable;
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
    public static void WritePositionValues<TPosition>(this Utf8JsonWriter writer, TPosition position) where TPosition : struct, IPosition<TPosition>
    {
        Span<double> values = stackalloc double[TPosition.MaxLength];
        position.GetValues(values);
        foreach (double value in values)
        {
            if (double.IsNaN(value))
            {
                break;
            }
            writer.WriteNumberValue(value);
        }
    }
}
