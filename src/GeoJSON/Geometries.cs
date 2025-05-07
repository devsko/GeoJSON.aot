// Copyright © devsko 2025
// Licensed under the MIT license.

using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace GeoJSON;

public partial class Geo<TPosition, TCoordinate>
{
    public class Point(TPosition coordinates) : Geometry
    {
        [JsonPropertyName("coordinates")]
        [JsonRequired]
        public TPosition Coordinates { get; init; } = coordinates;
    }

    [method: JsonConstructor]
    public class MultiPoint(ImmutableArray<TPosition> coordinates) : Geometry
    {
        [JsonPropertyName("coordinates")]
        [JsonRequired]
        public ImmutableArray<TPosition> Coordinates { get; init; } = coordinates.EnsureNotDefault();

        public MultiPoint(IEnumerable<TPosition> coordinates) : this([.. coordinates])
        { }
    }

    public class LineString : Geometry
    {
        private ImmutableArray<TPosition> _coordinates;

        [JsonPropertyName("coordinates")]
        [JsonRequired]
        public ImmutableArray<TPosition> Coordinates
        {
            get => _coordinates;
            init
            {
                value.EnsureNotDefault(nameof(Coordinates));
                if (value.Length < 2)
                {
                    throw new ArgumentException("A LineString consists of at least 2 positions.", nameof(Coordinates));
                }

                _coordinates = value;
            }
        }

        [JsonConstructor]
        internal LineString(ImmutableArray<TPosition> coordinates, bool _)
        {
            _coordinates = coordinates;
        }

        public LineString(ImmutableArray<TPosition> coordinates)
        {
            Coordinates = coordinates;
        }

        public LineString(IEnumerable<TPosition> coordinates) : this([.. coordinates])
        { }

        [JsonIgnore]
        public bool IsClosed => Coordinates.Length > 0 && Coordinates[0].Equals(Coordinates[^1]);

        [JsonIgnore]
        public bool IsLinearRing => Coordinates.Length >= 4 && IsClosed;
    }

    public class MultiLineString : Geometry
    {
        private ImmutableArray<ImmutableArray<TPosition>> _coordinates;

        [JsonPropertyName("coordinates")]
        [JsonRequired]
        public ImmutableArray<ImmutableArray<TPosition>> Coordinates
        {
            get => _coordinates;
            init
            {
                value.EnsureNotDefault(nameof(Coordinates));
                int i = 0;
                foreach (ImmutableArray<TPosition> lineCoordinates in value)
                {
                    lineCoordinates.EnsureNotDefault($"{nameof(Coordinates)}[{i}]");
                    if (lineCoordinates.Length < 2)
                    {
                        throw new ArgumentException("Each LineString of a MultiLineString consists of at least 2 positions.", $"{nameof(Coordinates)}[{i}]");
                    }
                    i++;
                }

                _coordinates = value;
            }
        }

        [JsonConstructor]
        internal MultiLineString(ImmutableArray<ImmutableArray<TPosition>> coordinates, bool _)
        {
            _coordinates = coordinates;
        }

        public MultiLineString(ImmutableArray<ImmutableArray<TPosition>> coordinates)
        {
            Coordinates = coordinates;
        }

        public MultiLineString(IEnumerable<LineString> lineStrings) : this(lineStrings.Select(lineString => lineString.Coordinates).ToImmutableArray())
        { }

        public MultiLineString(IEnumerable<IEnumerable<TPosition>> lineStringCoordinates) : this(lineStringCoordinates.Select(coordinates => coordinates.ToImmutableArray()).ToImmutableArray())
        { }

        [JsonIgnore]
        public IEnumerable<LineString> LineStrings => Coordinates.Select(lineCoordinates => new LineString(lineCoordinates));
    }

    public class Polygon : Geometry
    {
        private ImmutableArray<ImmutableArray<TPosition>> _coordinates;

        [JsonPropertyName("coordinates")]
        [JsonRequired]
        public ImmutableArray<ImmutableArray<TPosition>> Coordinates
        {
            get => _coordinates;
            init
            {
                value.EnsureNotDefault(nameof(Coordinates));
                int i = 0;
                foreach (ImmutableArray<TPosition> ringCoordinates in value)
                {
                    ringCoordinates.EnsureNotDefault($"{nameof(Coordinates)}[{i}]");
                    if (!IsLinearRing(ringCoordinates))
                    {
                        throw new ArgumentException("Each ring of a Polygon consists of at least 4 positions and must be closed.", $"{nameof(Coordinates)}[{i}]");
                    }
                    i++;

                }
                static bool IsLinearRing(ImmutableArray<TPosition> coordinates) => coordinates.Length >= 4 && coordinates[0].Equals(coordinates[^1]);

                _coordinates = value;
            }
        }

        [JsonConstructor]
        internal Polygon(ImmutableArray<ImmutableArray<TPosition>> coordinates, bool _)
        {
            _coordinates = coordinates;
        }

        public Polygon(ImmutableArray<ImmutableArray<TPosition>> coordinates)
        {
            Coordinates = coordinates;
        }

        public Polygon(IEnumerable<LineString> lineStrings) : this(lineStrings.Select(lineString => lineString.Coordinates).ToImmutableArray())
        { }
    }

    public class MultiPolygon : Geometry
    {
        private ImmutableArray<ImmutableArray<ImmutableArray<TPosition>>> _coordinates;

        [JsonPropertyName("coordinates")]
        [JsonRequired]
        public ImmutableArray<ImmutableArray<ImmutableArray<TPosition>>> Coordinates
        {
            get => _coordinates;
            init
            {
                value.EnsureNotDefault(nameof(Coordinates));
                int i = 0;
                foreach (ImmutableArray<ImmutableArray<TPosition>> polygonCorrdinates in value)
                {
                    polygonCorrdinates.EnsureNotDefault($"{nameof(Coordinates)}[{i}]");
                    int j = 0;
                    foreach (ImmutableArray<TPosition> ringCoordinates in polygonCorrdinates)
                    {
                        ringCoordinates.EnsureNotDefault($"{nameof(Coordinates)}[{i}][{j}]");
                        if (!IsLinearRing(ringCoordinates))
                        {
                            throw new ArgumentException("Each ring of a Polygon consists of at least 4 positions and must be closed.", $"{nameof(Coordinates)}[{i}][{j}]");
                        }
                        j++;
                    }
                    i++;
                }
                static bool IsLinearRing(ImmutableArray<TPosition> coordinates) => coordinates.Length >= 4 && coordinates[0].Equals(coordinates[coordinates.Length - 1]);

                _coordinates = value;
            }
        }

        [JsonConstructor]
        internal MultiPolygon(ImmutableArray<ImmutableArray<ImmutableArray<TPosition>>> coordinates, bool _)
        {
            _coordinates = coordinates;
        }

        public MultiPolygon(ImmutableArray<ImmutableArray<ImmutableArray<TPosition>>> coordinates)
        {
            Coordinates = coordinates;
        }

        public MultiPolygon(IEnumerable<IEnumerable<LineString>> polygons) : this(polygons.Select(polygon => polygon.Select(lineString => lineString.Coordinates).ToImmutableArray()).ToImmutableArray())
        { }
    }

    public class GeometryCollection : Geometry
    {
        [JsonPropertyName("geometries")]
        [JsonRequired]
        public ImmutableArray<Geometry> Geometries
        {
            get;
            init
            {
                value.EnsureNotDefault(nameof(Geometries));
                foreach (Geometry geometry in value)
                {
                    ArgumentNullException.ThrowIfNull(geometry);
                }

                field = value;
            }
        }

        [JsonConstructor]
        public GeometryCollection(ImmutableArray<Geometry> geometries)
        {
            Geometries = geometries;
        }

        public GeometryCollection(IEnumerable<Geometry> geometries) : this([.. geometries])
        { }
    }
}
