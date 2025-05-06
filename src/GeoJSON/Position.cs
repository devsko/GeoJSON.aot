// Copyright © devsko 2025
// Licensed under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace GeoJSON;

public interface IPosition<TPosition, TCoordinate> : IEquatable<TPosition>
    where TPosition : struct, IPosition<TPosition, TCoordinate>
    where TCoordinate : unmanaged, IFloatingPoint<TCoordinate>, IMinMaxValue<TCoordinate>
{
    static abstract int MaxLength { get; }
    static abstract int MinLength { get; }
    static abstract TPosition Create(params ReadOnlySpan<TCoordinate> coordinates);
    int GetCoordinates(Span<TCoordinate> coordinates);
}

public readonly struct Position3D<TCoordinate> : IPosition<Position3D<TCoordinate>, TCoordinate>
where TCoordinate : unmanaged, IFloatingPoint<TCoordinate>, IMinMaxValue<TCoordinate>
{
    private static readonly TCoordinate _notSet = TCoordinate.MinValue;

    static int IPosition<Position3D<TCoordinate>, TCoordinate>.MaxLength => 3;

    static int IPosition<Position3D<TCoordinate>, TCoordinate>.MinLength => 2;

    static Position3D<TCoordinate> IPosition<Position3D<TCoordinate>, TCoordinate>.Create(params scoped ReadOnlySpan<TCoordinate> coordinates) => coordinates.Length == 2 ? new Position3D<TCoordinate>(coordinates[0], coordinates[1]) : new Position3D<TCoordinate>(coordinates[0], coordinates[1], coordinates[2]);

    int IPosition<Position3D<TCoordinate>, TCoordinate>.GetCoordinates(Span<TCoordinate> coordinates)
    {
        coordinates[0] = Longitude;
        coordinates[1] = Latitude;
        if (_altitude == _notSet)
        {
            return 2;
        }
        coordinates[2] = _altitude;

        return 3;
    }

    public TCoordinate Longitude { get; }

    public TCoordinate Latitude { get; }

    private readonly TCoordinate _altitude;

    public Position3D(TCoordinate longitude, TCoordinate latitude)
    {
        if (TCoordinate.IsNaN(longitude) || longitude > TCoordinate.CreateChecked(180) || longitude < TCoordinate.CreateChecked(-180))
        {
            throw new ArgumentOutOfRangeException(nameof(longitude));
        }
        if (TCoordinate.IsNaN(latitude) || latitude > TCoordinate.CreateChecked(90) || latitude < TCoordinate.CreateChecked(-90))
        {
            throw new ArgumentOutOfRangeException(nameof(latitude));
        }

        Longitude = longitude;
        Latitude = latitude;
        _altitude = _notSet;
    }

    public Position3D(TCoordinate longitude, TCoordinate latitude, TCoordinate altitude) : this(longitude, latitude)
    {
        if (TCoordinate.IsNaN(altitude) || TCoordinate.IsInfinity(altitude))
        {
            throw new ArgumentOutOfRangeException(nameof(altitude));
        }

        _altitude = altitude;
    }

    public TCoordinate? Altitude => _altitude == _notSet ? null : _altitude;

    public bool Equals(Position3D<TCoordinate> other) =>
        CoordinateComparer<TCoordinate>.Instance.Equals(Longitude, other.Longitude) &&
        CoordinateComparer<TCoordinate>.Instance.Equals(Latitude, other.Latitude) &&
        CoordinateComparer<TCoordinate>.Instance.Equals(_altitude, other._altitude);

    public override bool Equals(object? obj) => obj is Position3D<TCoordinate> position && Equals(position);

    public override int GetHashCode() => HashCode.Combine(Longitude, Latitude, _altitude);

    public override string ToString() => _altitude == _notSet ? $"(Lon={Longitude};Lat={Latitude})" : $"(Lon={Longitude};Lat={Latitude};Alt={Altitude})";

    public static bool operator ==(Position3D<TCoordinate> left, Position3D<TCoordinate> right) => left.Equals(right);

    public static bool operator !=(Position3D<TCoordinate> left, Position3D<TCoordinate> right) => !(left == right);
}

public readonly struct Position2D<TCoordinate> : IPosition<Position2D<TCoordinate>, TCoordinate>
    where TCoordinate : unmanaged, IFloatingPoint<TCoordinate>, IMinMaxValue<TCoordinate>
{
    static int IPosition<Position2D<TCoordinate>, TCoordinate>.MaxLength => 2;

    static int IPosition<Position2D<TCoordinate>, TCoordinate>.MinLength => 2;

    static Position2D<TCoordinate> IPosition<Position2D<TCoordinate>, TCoordinate>.Create(params scoped ReadOnlySpan<TCoordinate> coordinates) => new Position2D<TCoordinate>(coordinates[0], coordinates[1]);

    int IPosition<Position2D<TCoordinate>, TCoordinate>.GetCoordinates(Span<TCoordinate> coordinates)
    {
        coordinates[0] = Longitude;
        coordinates[1] = Latitude;

        return 2;
    }

    public TCoordinate Longitude { get; }

    public TCoordinate Latitude { get; }

    public Position2D(TCoordinate longitude, TCoordinate latitude)
    {
        if (TCoordinate.IsNaN(longitude) || longitude > TCoordinate.CreateChecked(180) || longitude < TCoordinate.CreateChecked(-180))
        {
            throw new ArgumentOutOfRangeException(nameof(longitude));
        }
        if (TCoordinate.IsNaN(latitude) || latitude > TCoordinate.CreateChecked(90) || latitude < TCoordinate.CreateChecked(-90))
        {
            throw new ArgumentOutOfRangeException(nameof(latitude));
        }

        Longitude = longitude;
        Latitude = latitude;
    }

    public bool Equals(Position2D<TCoordinate> other) =>
        CoordinateComparer<TCoordinate>.Instance.Equals(Longitude, other.Longitude) &&
        CoordinateComparer<TCoordinate>.Instance.Equals(Latitude, other.Latitude);

    public override bool Equals(object? obj) => obj is Position2D<TCoordinate> position && Equals(position);

    public override int GetHashCode() => HashCode.Combine(Longitude, Latitude);

    public override string ToString() => $"(Lon={Longitude};Lat={Latitude})";

    public static bool operator ==(Position2D<TCoordinate> left, Position2D<TCoordinate> right) => left.Equals(right);

    public static bool operator !=(Position2D<TCoordinate> left, Position2D<TCoordinate> right) => !(left == right);
}

public sealed class CoordinateComparer<TCoordinate> : IEqualityComparer<TCoordinate>
    where TCoordinate : unmanaged, IFloatingPoint<TCoordinate>, IMinMaxValue<TCoordinate>
{
    private const int Factor = 10_000_000;

    public static CoordinateComparer<TCoordinate> Instance { get; } = new();

    private CoordinateComparer()
    { }

    public bool Equals(TCoordinate x, TCoordinate y) =>
        x.Equals(y)
            ? true
            : TCoordinate.IsNaN(x) || TCoordinate.IsNaN(y)
                ? false
                : TCoordinate.Abs(x - y) * TCoordinate.CreateChecked(Factor) < TCoordinate.One;

    public int GetHashCode([DisallowNull] TCoordinate obj) => throw new NotImplementedException();
}
