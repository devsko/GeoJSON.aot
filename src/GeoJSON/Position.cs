// Copyright © devsko 2025
// Licensed under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace GeoJSON;

public interface IPosition<TPosition> : IEquatable<TPosition> where TPosition : struct
{
    static abstract int MaxLength { get; }
    static abstract int MinLength { get; }
    static abstract TPosition Create(params ReadOnlySpan<double> values);
    void GetValues(Span<double> values);
}

[JsonConverter(typeof(Serializer<Position3D>.PositionConverter))]
public readonly struct Position3D : IPosition<Position3D>
{
    static int IPosition<Position3D>.MaxLength => 3;

    static int IPosition<Position3D>.MinLength => 2;

    static Position3D IPosition<Position3D>.Create(params scoped ReadOnlySpan<double> values) => values.Length == 2 ? new Position3D(values[0], values[1]) : new Position3D(values[0], values[1], values[2]);

    void IPosition<Position3D>.GetValues(Span<double> values)
    {
        values[0] = Longitude;
        values[1] = Latitude;
        values[2] = Altitude;
    }

    public double Longitude { get; }

    public double Latitude { get; }

    public double Altitude { get; }

    public Position3D(double longitude, double latitude)
    {
        if (double.IsNaN(longitude) || longitude > 180 || longitude < -180)
        {
            throw new ArgumentOutOfRangeException(nameof(longitude));
        }
        if (double.IsNaN(latitude) || latitude > 90 || latitude < -90)
        {
            throw new ArgumentOutOfRangeException(nameof(latitude));
        }

        Longitude = longitude;
        Latitude = latitude;
        Altitude = double.NaN;
    }

    public Position3D(double longitude, double latitude, double altitude) : this(longitude, latitude)
    {
        if (double.IsNaN(altitude))
        {
            throw new ArgumentOutOfRangeException(nameof(altitude));
        }

        Altitude = altitude;
    }

    public bool Equals(Position3D other) =>
        DoubleComparer.Instance.Equals(Longitude, other.Longitude) &&
        DoubleComparer.Instance.Equals(Latitude, other.Latitude) &&
        DoubleComparer.Instance.Equals(Altitude, other.Altitude);

    public override bool Equals(object? obj) => obj is Position3D position && Equals(position);

    public override int GetHashCode() => throw new NotImplementedException();

    public override string ToString() => double.IsNaN(Altitude) ? $"(Lon={Longitude};Lat={Latitude})" : $"(Lon={Longitude};Lat={Latitude};Alt={Altitude})";

    public static bool operator ==(Position3D left, Position3D right) => left.Equals(right);

    public static bool operator !=(Position3D left, Position3D right) => !(left == right);
}

[JsonConverter(typeof(Serializer<Position2D>.PositionConverter))]
public readonly struct Position2D : IPosition<Position2D>
{
    static int IPosition<Position2D>.MaxLength => 2;

    static int IPosition<Position2D>.MinLength => 2;

    static Position2D IPosition<Position2D>.Create(params scoped ReadOnlySpan<double> values) => new Position2D(values[0], values[1]);

    void IPosition<Position2D>.GetValues(Span<double> values)
    {
        values[0] = Longitude;
        values[1] = Latitude;
    }

    public double Longitude { get; }

    public double Latitude { get; }

    public Position2D(double longitude, double latitude)
    {
        if (double.IsNaN(longitude) || longitude > 180 || longitude < -180)
        {
            throw new ArgumentOutOfRangeException(nameof(longitude));
        }
        if (double.IsNaN(latitude) || latitude > 90 || latitude < -90)
        {
            throw new ArgumentOutOfRangeException(nameof(latitude));
        }

        Longitude = longitude;
        Latitude = latitude;
    }

    public bool Equals(Position2D other) =>
        DoubleComparer.Instance.Equals(Longitude, other.Longitude) &&
        DoubleComparer.Instance.Equals(Latitude, other.Latitude);

    public override bool Equals(object? obj) => obj is Position2D position && Equals(position);

    public override int GetHashCode() => throw new NotImplementedException();

    public override string ToString() => $"(Lon={Longitude};Lat={Latitude})";

    public static bool operator ==(Position2D left, Position2D right) => left.Equals(right);

    public static bool operator !=(Position2D left, Position2D right) => !(left == right);
}

public sealed class DoubleComparer : IEqualityComparer<double>
{
    private const int Factor = 1_000_000_000;

    public static DoubleComparer Instance { get; } = new();

    private DoubleComparer()
    { }

    public bool Equals(double x, double y) => double.IsNaN(x) ? double.IsNaN(y) : !double.IsNaN(y) && double.Abs(x - y) * Factor < 1;

    public int GetHashCode([DisallowNull] double obj) => throw new NotImplementedException();
}
