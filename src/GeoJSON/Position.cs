// Copyright © devsko 2025
// Licensed under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace GeoJSON;

public partial class Geo<TCoordinate>
{
    public interface IPosition<TPosition> : IEquatable<TPosition>
        where TPosition : struct, IPosition<TPosition>
    {
        static abstract int MaxLength { get; }
        static abstract int MinLength { get; }
        static abstract TPosition Create(params ReadOnlySpan<TCoordinate> coordinates);
        int GetCoordinates(Span<TCoordinate> coordinates);
    }

    // The PositionConverter cannot be used with JsonConverterAttribute because of CS0416.
    // It is registered in Serializer<TPosition>.CreateOptions().
    public readonly struct Position3D : IPosition<Position3D>
    {
        private static readonly TCoordinate _notSet = TCoordinate.MinValue;

        static int IPosition<Position3D>.MaxLength => 3;

        static int IPosition<Position3D>.MinLength => 2;

        static Position3D IPosition<Position3D>.Create(params scoped ReadOnlySpan<TCoordinate> coordinates) => coordinates.Length == 2 ? new Position3D(coordinates[0], coordinates[1]) : new Position3D(coordinates[0], coordinates[1], coordinates[2]);

        int IPosition<Position3D>.GetCoordinates(Span<TCoordinate> coordinates)
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

        public bool Equals(Position3D other) =>
            CoordinateComparer.Instance.Equals(Longitude, other.Longitude) &&
            CoordinateComparer.Instance.Equals(Latitude, other.Latitude) &&
            CoordinateComparer.Instance.Equals(_altitude, other._altitude);

        public override bool Equals(object? obj) => obj is Position3D position && Equals(position);

        public override int GetHashCode() => HashCode.Combine(Longitude, Latitude, _altitude);

        public override string ToString() => _altitude == _notSet ? $"(Lon={Longitude};Lat={Latitude})" : $"(Lon={Longitude};Lat={Latitude};Alt={Altitude})";

        public static bool operator ==(Position3D left, Position3D right) => left.Equals(right);

        public static bool operator !=(Position3D left, Position3D right) => !(left == right);
    }

    // The PositionConverter cannot be used with JsonConverterAttribute because of CS0416.
    // It is registered in Serializer<TPosition, TCoordinate>.CreateOptions().
    public readonly struct Position2D : IPosition<Position2D>
    {
        static int IPosition<Position2D>.MaxLength => 2;

        static int IPosition<Position2D>.MinLength => 2;

        static Position2D IPosition<Position2D>.Create(params scoped ReadOnlySpan<TCoordinate> coordinates) => new Position2D(coordinates[0], coordinates[1]);

        int IPosition<Position2D>.GetCoordinates(Span<TCoordinate> coordinates)
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

        public bool Equals(Position2D other) =>
            CoordinateComparer.Instance.Equals(Longitude, other.Longitude) &&
            CoordinateComparer.Instance.Equals(Latitude, other.Latitude);

        public override bool Equals(object? obj) => obj is Position2D position && Equals(position);

        public override int GetHashCode() => HashCode.Combine(Longitude, Latitude);

        public override string ToString() => $"(Lon={Longitude};Lat={Latitude})";

        public static bool operator ==(Position2D left, Position2D right) => left.Equals(right);

        public static bool operator !=(Position2D left, Position2D right) => !(left == right);
    }

    public sealed class CoordinateComparer : IEqualityComparer<TCoordinate>
    {
        private const int Factor = 10_000_000;

        public static CoordinateComparer Instance { get; } = new();

        private CoordinateComparer()
        { }

        public bool Equals(TCoordinate x, TCoordinate y)
        {
            if (x.Equals(y))
            {
                return true;
            }
            if (TCoordinate.IsNaN(x) || TCoordinate.IsNaN(y))
            {
                return false;
            }

            return TCoordinate.Abs(x - y) * TCoordinate.CreateChecked(Factor) < TCoordinate.One;
        }

        public int GetHashCode([DisallowNull] TCoordinate obj) => throw new NotImplementedException();
    }
}
