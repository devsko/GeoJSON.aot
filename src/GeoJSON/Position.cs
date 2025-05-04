// Copyright © devsko 2025
// Licensed under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GeoJson;

public interface IPosition<TPosition> : IEquatable<TPosition> where TPosition : struct
{
    static abstract int MaxLength { get; }
    static abstract int MinLength { get; }
    static abstract TPosition Create(params ReadOnlySpan<float> values);
    void GetValues(Span<float> values);
}

[JsonConverter(typeof(Serializer<Position3D>.PositionConverter))]
public readonly struct Position3D : IPosition<Position3D>
{
    static int IPosition<Position3D>.MaxLength => 3;

    static int IPosition<Position3D>.MinLength => 2;

    static Position3D IPosition<Position3D>.Create(params scoped ReadOnlySpan<float> values) => values.Length == 2 ? new Position3D(values[0], values[1]) : new Position3D(values[0], values[1], values[2]);

    void IPosition<Position3D>.GetValues(Span<float> values)
    {
        values[0] = Longitude;
        values[1] = Latitude;
        values[2] = Altitude;
    }

    public float Longitude { get; }

    public float Latitude { get; }

    public float Altitude { get; }

    public Position3D(float longitude, float latitude)
    {
        if (float.IsNaN(longitude) || longitude > 180 || longitude < -180)
        {
            throw new ArgumentOutOfRangeException(nameof(longitude));
        }
        if (float.IsNaN(latitude) || latitude > 90 || latitude < -90)
        {
            throw new ArgumentOutOfRangeException(nameof(latitude));
        }

        Longitude = longitude;
        Latitude = latitude;
        Altitude = float.NaN;
    }

    public Position3D(float longitude, float latitude, float altitude) : this(longitude, latitude)
    {
        if (float.IsNaN(altitude))
        {
            throw new ArgumentOutOfRangeException(nameof(altitude));
        }

        Altitude = altitude;
    }

    public bool Equals(Position3D other) =>
        FloatComparer.Instance.Equals(Longitude, other.Longitude) &&
        FloatComparer.Instance.Equals(Latitude, other.Latitude) &&
        FloatComparer.Instance.Equals(Altitude, other.Altitude);

    public override bool Equals(object? obj) => obj is Position3D position && Equals(position);

    public override int GetHashCode() => throw new NotImplementedException();

    public override string ToString() => float.IsNaN(Altitude) ? $"(Lon={Longitude};Lat={Latitude})" : $"(Lon={Longitude};Lat={Latitude};Alt={Altitude})";

    public static bool operator ==(Position3D left, Position3D right) => left.Equals(right);

    public static bool operator !=(Position3D left, Position3D right) => !(left == right);
}

[JsonConverter(typeof(Serializer<Position2D>.PositionConverter))]
public readonly struct Position2D : IPosition<Position2D>
{
    static int IPosition<Position2D>.MaxLength => 2;

    static int IPosition<Position2D>.MinLength => 2;

    static Position2D IPosition<Position2D>.Create(params scoped ReadOnlySpan<float> values) => new Position2D(values[0], values[1]);

    void IPosition<Position2D>.GetValues(Span<float> values)
    {
        values[0] = Longitude;
        values[1] = Latitude;
    }

    public float Longitude { get; }

    public float Latitude { get; }

    public Position2D(float longitude, float latitude)
    {
        if (float.IsNaN(longitude) || longitude > 180 || longitude < -180)
        {
            throw new ArgumentOutOfRangeException(nameof(longitude));
        }
        if (float.IsNaN(latitude) || latitude > 90 || latitude < -90)
        {
            throw new ArgumentOutOfRangeException(nameof(latitude));
        }

        Longitude = longitude;
        Latitude = latitude;
    }

    public bool Equals(Position2D other) =>
        FloatComparer.Instance.Equals(Longitude, other.Longitude) &&
        FloatComparer.Instance.Equals(Latitude, other.Latitude);

    public override bool Equals(object? obj) => obj is Position2D position && Equals(position);

    public override int GetHashCode() => throw new NotImplementedException();

    public override string ToString() => $"(Lon={Longitude};Lat={Latitude})";

    public static bool operator ==(Position2D left, Position2D right) => left.Equals(right);

    public static bool operator !=(Position2D left, Position2D right) => !(left == right);
}

public sealed class FloatComparer : IEqualityComparer<float>
{
    private const int Factor = 1_000_000;

    public static FloatComparer Instance { get; } = new();

    private FloatComparer()
    { }

    public bool Equals(float x, float y) => float.IsNaN(x) ? float.IsNaN(y) : !float.IsNaN(y) && float.Abs(x - y) * Factor < 1;

    public int GetHashCode([DisallowNull] float obj) => throw new NotImplementedException();
}

internal static class UTf8JsonWriterExtensions
{
    public static void WritePositionValues<TPosition>(this Utf8JsonWriter writer, TPosition position) where TPosition : struct, IPosition<TPosition>
    {
        Span<float> values = stackalloc float[TPosition.MaxLength];
        position.GetValues(values);
        foreach (float value in values)
        {
            if (float.IsNaN(value))
            {
                break;
            }
            writer.WriteNumberValue(value);
        }
    }
}
