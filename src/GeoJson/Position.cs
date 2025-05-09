// Copyright © devsko 2025. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace GeoJson;

/// <summary>
/// Represents a position in a multi-dimensional space, defined by a set of coordinates.
/// </summary>
/// <remarks>This interface defines the contract for working with positions in a coordinate system, including
/// creating positions, retrieving their coordinates, and determining their equality.</remarks>
/// <typeparam name="TPosition">The type of the position, which must be a value type implementing <see cref="IPosition{TPosition, TCoordinate}"/>.</typeparam>
/// <typeparam name="TCoordinate">The type of the coordinate, which must be an unmanaged type that supports floating-point operations.</typeparam>
public interface IPosition<TPosition, TCoordinate> : IEquatable<TPosition>
    where TPosition : struct, IPosition<TPosition, TCoordinate>
    where TCoordinate : unmanaged, IFloatingPoint<TCoordinate>, IMinMaxValue<TCoordinate>
{
    /// <summary>
    /// Gets the maximum number of dimensions of the implementing type.
    /// </summary>
    static abstract int MaxLength { get; }
    /// <summary>
    /// Gets the minimum number of dimensions of the implementing type.
    /// </summary>
    static abstract int MinLength { get; }
    /// <summary>
    /// Creates a new instance of the implementing type using the specified coordinates.
    /// </summary>
    /// <param name="coordinates">A collection of coordinates used to define the position. Each coordinate is provided as a <typeparamref name="TCoordinate"/>.</param>
    /// <returns>A new instance of the implementing type representing the specified position.</returns>
    static abstract TPosition Create(params ReadOnlySpan<TCoordinate> coordinates);
    /// <summary>
    /// Retrieves a collection of coordinates and writes them into the provided span.
    /// </summary>
    /// <param name="coordinates">A span to which the coordinates will be written. The span must have sufficient capacity to hold the coordinates.</param>
    /// <returns>The number of coordinates written to the span.</returns>
    int GetCoordinates(Span<TCoordinate> coordinates);
}

/// <summary>
/// Represents a three-dimensional geographic position defined by longitude, latitude, and an optional altitude.
/// </summary>
/// <remarks>The <see cref="Position3D{TCoordinate}"/> struct is immutable and provides functionality for working
/// with geographic coordinates. Longitude and latitude are required, while altitude is optional. If altitude is not
/// specified, it will be <see langword="null"/>.</remarks>
/// <typeparam name="TCoordinate">The type of the coordinate values. Must be an unmanaged type that implements <see cref="IFloatingPoint{T}"/>.</typeparam>
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

    /// <summary>
    /// Gets the longitude coordinate.
    /// </summary>
    public TCoordinate Longitude { get; }

    /// <summary>
    /// Gets the latitude coordinate.
    /// </summary>
    public TCoordinate Latitude { get; }

    private readonly TCoordinate _altitude;

    /// <summary>
    /// Initializes a new instance of the <see cref="Position3D{TCoordinate}"/> class with the specified longitude and
    /// latitude.
    /// </summary>
    /// <param name="longitude">The longitude of the position, in degrees. Must be within the range -180 to 180, inclusive.</param>
    /// <param name="latitude">The latitude of the position, in degrees. Must be within the range -90 to 90, inclusive.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="longitude"/> is not a number (NaN) or is outside the range -180 to 180, or if
    /// <paramref name="latitude"/> is not a number (NaN) or is outside the range -90 to 90.</exception>
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

    /// <summary>
    /// Initializes a new instance of the <see cref="Position3D{TCoordinate}"/> class with the specified longitude,
    /// latitude, and altitude.
    /// </summary>
    /// <param name="longitude">The longitude of the position, in degrees. Must be within the range -180 to 180, inclusive.</param>
    /// <param name="latitude">The latitude of the position, in degrees. Must be within the range -90 to 90, inclusive.</param>
    /// <param name="altitude">The altitude coordinate of the position. Must be a valid <typeparamref name="TCoordinate"/> value that is not
    /// NaN or infinity.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="longitude"/> is not a number (NaN) or is outside the range -180 to 180, or if
    /// <paramref name="latitude"/> is not a number (NaN) or is outside the range -90 to 90 or if <paramref name="altitude"/> is NaN or infinity.</exception>
    public Position3D(TCoordinate longitude, TCoordinate latitude, TCoordinate altitude) : this(longitude, latitude)
    {
        if (TCoordinate.IsNaN(altitude) || TCoordinate.IsInfinity(altitude))
        {
            throw new ArgumentOutOfRangeException(nameof(altitude));
        }

        _altitude = altitude;
    }

    /// <summary>
    /// Gets the altitude value, if available.
    /// </summary>
    public TCoordinate? Altitude => _altitude == _notSet ? null : _altitude;

    /// <summary>
    /// Determines whether the current <see cref="Position3D{TCoordinate}"/> is equal to another <see
    /// cref="Position3D{TCoordinate}"/> instance.
    /// </summary>
    /// <remarks>Two <see cref="Position3D{TCoordinate}"/> instances are considered equal if their <see
    /// cref="Longitude"/>, <see cref="Latitude"/>,  and altitude values are equal as determined by the <see
    /// cref="CoordinateComparer{TCoordinate}"/>.</remarks>
    /// <param name="other">The <see cref="Position3D{TCoordinate}"/> instance to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the current instance is equal to the <paramref name="other"/> instance; otherwise,
    /// <see langword="false"/>.</returns>
    public bool Equals(Position3D<TCoordinate> other) =>
        CoordinateComparer<TCoordinate>.Instance.Equals(Longitude, other.Longitude) &&
        CoordinateComparer<TCoordinate>.Instance.Equals(Latitude, other.Latitude) &&
        CoordinateComparer<TCoordinate>.Instance.Equals(_altitude, other._altitude);

    ///<inheritdoc/>
    public override bool Equals(object? obj) => obj is Position3D<TCoordinate> position && Equals(position);

    ///<inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Longitude, Latitude, _altitude);

    ///<inheritdoc/>
    public override string ToString() => _altitude == _notSet ? $"(Lon={Longitude};Lat={Latitude})" : $"(Lon={Longitude};Lat={Latitude};Alt={Altitude})";

    /// <summary>
    /// Determines whether two <see cref="Position3D{TCoordinate}"/> instances are equal.
    /// </summary>
    /// <param name="left">The first <see cref="Position3D{TCoordinate}"/> instance to compare.</param>
    /// <param name="right">The second <see cref="Position3D{TCoordinate}"/> instance to compare.</param>
    /// <returns><see langword="true"/> if the two <see cref="Position3D{TCoordinate}"/> instances are equal; otherwise, <see
    /// langword="false"/>.</returns>
    public static bool operator ==(Position3D<TCoordinate> left, Position3D<TCoordinate> right) => left.Equals(right);

    /// <summary>
    /// Determines whether two <see cref="Position3D{TCoordinate}"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first <see cref="Position3D{TCoordinate}"/> instance to compare.</param>
    /// <param name="right">The second <see cref="Position3D{TCoordinate}"/> instance to compare.</param>
    /// <returns><see langword="true"/> if the two <see cref="Position3D{TCoordinate}"/> instances are not equal;  otherwise,
    /// <see langword="false"/>.</returns>
    public static bool operator !=(Position3D<TCoordinate> left, Position3D<TCoordinate> right) => !(left == right);
}

/// <summary>
/// Represents a two-dimensional geographic position defined by longitude and latitude coordinates.
/// </summary>
/// <remarks>The <see cref="Position2D{TCoordinate}"/> struct is immutable and provides functionality for  working
/// with geographic coordinates. Longitude values must be in the range [-180, 180], and  latitude values must be in the
/// range [-90, 90].</remarks>
/// <typeparam name="TCoordinate">The type of the coordinate values. Must be an unmanaged type that implements  <see cref="IFloatingPoint{TSelf}"/>.</typeparam>
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

    /// <summary>
    /// Gets the longitude coordinate of the location.
    /// </summary>
    public TCoordinate Longitude { get; }

    /// <summary>
    /// Gets the latitude coordinate.
    /// </summary>
    public TCoordinate Latitude { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Position2D{TCoordinate}"/> class with the specified longitude and
    /// latitude.
    /// </summary>
    /// <param name="longitude">The longitude of the position. Must be within the range [-180, 180].</param>
    /// <param name="latitude">The latitude of the position. Must be within the range [-90, 90].</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="longitude"/> is not within the range [-180, 180] or if <paramref name="latitude"/> is
    /// not within the range [-90, 90].</exception>
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

    /// <summary>
    /// Determines whether the current <see cref="Position2D{TCoordinate}"/> is equal to the specified <paramref
    /// name="other"/> instance.
    /// </summary>
    /// <param name="other">The <see cref="Position2D{TCoordinate}"/> instance to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the current instance is equal to the specified <paramref name="other"/> instance;
    /// otherwise, <see langword="false"/>.</returns>
    public bool Equals(Position2D<TCoordinate> other) =>
        CoordinateComparer<TCoordinate>.Instance.Equals(Longitude, other.Longitude) &&
        CoordinateComparer<TCoordinate>.Instance.Equals(Latitude, other.Latitude);

    ///<inheritdoc/>
    public override bool Equals(object? obj) => obj is Position2D<TCoordinate> position && Equals(position);

    ///<inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Longitude, Latitude);

    ///<inheritdoc/>
    public override string ToString() => $"(Lon={Longitude};Lat={Latitude})";

    /// <summary>
    /// Determines whether two <see cref="Position2D{TCoordinate}"/> instances are equal.
    /// </summary>
    /// <param name="left">The first <see cref="Position2D{TCoordinate}"/> instance to compare.</param>
    /// <param name="right">The second <see cref="Position2D{TCoordinate}"/> instance to compare.</param>
    /// <returns><see langword="true"/> if the two instances are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(Position2D<TCoordinate> left, Position2D<TCoordinate> right) => left.Equals(right);

    /// <summary>
    /// Determines whether two <see cref="Position2D{TCoordinate}"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first <see cref="Position2D{TCoordinate}"/> instance to compare.</param>
    /// <param name="right">The second <see cref="Position2D{TCoordinate}"/> instance to compare.</param>
    /// <returns><see langword="true"/> if the two <see cref="Position2D{TCoordinate}"/> instances are not equal;  otherwise,
    /// <see langword="false"/>.</returns>
    public static bool operator !=(Position2D<TCoordinate> left, Position2D<TCoordinate> right) => !(left == right);
}

internal sealed class CoordinateComparer<TCoordinate> : IEqualityComparer<TCoordinate>
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
