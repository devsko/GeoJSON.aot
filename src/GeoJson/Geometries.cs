// Copyright © devsko 2025. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace GeoJson;

public partial class Geo<TPosition, TCoordinate>
{
    /// <summary>
    /// Represents a single geographic point with a specific position.
    /// </summary>
    /// <remarks>This class is a specialization of <see cref="Geometry"/> and is used to model a point in a
    /// geographic or spatial context.</remarks>
    /// <param name="coordinates"></param>
    public class Point(TPosition coordinates) : Geometry
    {
        /// <summary>
        /// Gets the geographic coordinates associated with the point.
        /// </summary>
        [JsonPropertyName("coordinates")]
        [JsonRequired]
        public TPosition Coordinates { get; init; } = coordinates;
    }

    /// <summary>
    /// Represents a geometric shape consisting of multiple points, where each point is defined by a position.
    /// </summary>
    /// <param name="coordinates"></param>
    [method: JsonConstructor]
    public class MultiPoint(ImmutableArray<TPosition> coordinates) : Geometry
    {
        /// <summary>
        /// Gets the collection of coordinates that define the positions in the geometry.
        /// </summary>
        [JsonPropertyName("coordinates")]
        [JsonRequired]
        public ImmutableArray<TPosition> Coordinates { get; init; } = coordinates.EnsureNotDefault();

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiPoint"/> class with the specified coordinates.
        /// </summary>
        /// <param name="coordinates">A collection of positions that define the points in the multi-point geometry. Cannot be null.</param>
        public MultiPoint(IEnumerable<TPosition> coordinates) : this([.. coordinates ?? throw new ArgumentNullException(nameof(coordinates))])
        { }
    }

    /// <summary>
    /// Represents a geometric line string, which is a series of at least 2 connected positions forming a continuous line.
    /// </summary>
    public class LineString : Geometry
    {
        private ImmutableArray<TPosition> _coordinates;

        /// <summary>
        /// Gets the collection of positions that define the geometry of the LineString.
        /// </summary>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="LineString"/> class with the specified coordinates.
        /// </summary>
        /// <param name="coordinates">The collection of positions that define the line string. This must be a <see
        /// cref="ImmutableArray{T}"/> of at least 2 positions.</param>
        public LineString(ImmutableArray<TPosition> coordinates)
        {
            Coordinates = coordinates;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LineString"/> class with the specified coordinates.
        /// </summary>
        /// <param name="coordinates">A collection of at least 2 positions that define the line string.</param>
        public LineString(IEnumerable<TPosition> coordinates) : this([.. coordinates ?? throw new ArgumentNullException(nameof(coordinates))])
        { }

        /// <summary>
        /// Gets a value indicating whether the line is a closed loop.
        /// </summary>
        [JsonIgnore]
        public bool IsClosed => Coordinates.Length > 0 && Coordinates[0].Equals(Coordinates[^1]);

        /// <summary>
        /// Gets a value indicating whether the geometry represents a linear ring which consist of at least 4 positions and is a closed loop.
        /// </summary>
        [JsonIgnore]
        public bool IsLinearRing => Coordinates.Length >= 4 && IsClosed;
    }

    /// <summary>
    /// Represents a geometric shape consisting of multiple line strings, where each line string is defined by a
    /// sequence of positions.
    /// </summary>
    public class MultiLineString : Geometry
    {
        private ImmutableArray<ImmutableArray<TPosition>> _coordinates;

        /// <summary>
        /// Gets the collection of line strings, where each line string is represented as an immutable array of
        /// positions.
        /// </summary>
        /// <remarks>Each line string in the collection must contain at least two positions. If this
        /// condition is not met during initialization, an <see cref="ArgumentException"/> is thrown.</remarks>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiLineString"/> class with the specified
        /// coordinates.
        /// </summary>
        /// <param name="coordinates">An immutable array of immutable arrays, where each inner array represents a line string defined by a
        /// sequence of at least 2 positions of type <typeparamref name="TPosition"/>.</param>
        public MultiLineString(ImmutableArray<ImmutableArray<TPosition>> coordinates)
        {
            Coordinates = coordinates;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiLineString"/> class using a collection of <see
        /// cref="LineString"/> objects.
        /// </summary>
        /// <param name="lineStrings">A collection of <see cref="LineString"/> objects that define the individual line segments of the multi-line
        /// geometry.</param>
        public MultiLineString(IEnumerable<LineString> lineStrings) : this(lineStrings.Select(lineString => lineString.Coordinates).ToImmutableArray())
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiLineString"/> class using the specified
        /// collection of line string coordinates.
        /// </summary>
        /// <param name="lineStringCoordinates">A collection of line strings, where each line string is represented as a collection of coordinates. Each
        /// coordinate defines a position in the line string.</param>
        public MultiLineString(IEnumerable<IEnumerable<TPosition>> lineStringCoordinates) : this(lineStringCoordinates.Select(coordinates => coordinates.ToImmutableArray()).ToImmutableArray())
        { }

        /// <summary>
        /// Gets a collection of <see cref="LineString"/> objects derived from the current coordinates.
        /// </summary>
        [JsonIgnore]
        public IEnumerable<LineString> LineStrings => Coordinates.Select(lineCoordinates => new LineString(lineCoordinates));
    }

    /// <summary>
    /// Represents a polygon geometry, defined by one or more linear rings.
    /// </summary>
    /// <remarks>A polygon consists of an outer boundary (the first ring) and zero or more inner boundaries
    /// (holes). Each ring must be a closed linear ring, meaning it contains at least four positions and the first and
    /// last positions are identical. The outer boundary defines the exterior of the polygon, while inner boundaries
    /// define holes within the polygon.</remarks>
    public class Polygon : Geometry
    {
        private ImmutableArray<ImmutableArray<TPosition>> _coordinates;

        /// <summary>
        /// Gets or initializes the collection of linear rings that define the polygon's geometry.
        /// </summary>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="Polygon"/> class with the specified coordinates.
        /// </summary>
        /// <param name="coordinates">A collection of linear rings, where each ring is represented as an <see cref="ImmutableArray{T}"/> of
        /// positions. The first ring represents the outer boundary of the polygon, and any subsequent rings represent
        /// holes within the polygon.</param>
        public Polygon(ImmutableArray<ImmutableArray<TPosition>> coordinates)
        {
            Coordinates = coordinates;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Polygon"/> class using the specified collection of <see
        /// cref="LineString"/> objects.
        /// </summary>
        /// <param name="lineStrings">A collection of <see cref="LineString"/> objects that define the boundaries of the polygon.  Each <see
        /// cref="LineString"/> must contain a valid sequence of coordinates.</param>
        public Polygon(IEnumerable<LineString> lineStrings) : this(lineStrings.Select(lineString => lineString.Coordinates).ToImmutableArray())
        { }
    }

    /// <summary>
    /// Represents a geometric shape consisting of multiple polygons, where each polygon is defined by one or more
    /// closed linear rings.
    /// </summary>
    /// <remarks>A <see cref="MultiPolygon"/> is a collection of polygons, where each polygon is represented
    /// as an array of linear rings.  The first ring in each polygon is the outer boundary, and any subsequent rings
    /// represent holes within the polygon.  Each ring must consist of at least four positions and must be closed (i.e.,
    /// the first and last positions must be identical).</remarks>
    public class MultiPolygon : Geometry
    {
        private ImmutableArray<ImmutableArray<ImmutableArray<TPosition>>> _coordinates;

        /// <summary>
        /// Gets or initializes the coordinates of the polygon geometry.
        /// </summary>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiPolygon"/> class with the specified coordinates.
        /// </summary>
        /// <param name="coordinates">An immutable array representing the coordinates of the multi-polygon. Each outer array
        /// represents a polygon, each inner array represents a linear ring,  and each innermost array represents the
        /// positions within a ring.</param>
        public MultiPolygon(ImmutableArray<ImmutableArray<ImmutableArray<TPosition>>> coordinates)
        {
            Coordinates = coordinates;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiPolygon"/> class using the specified collection of polygons.
        /// </summary>
        /// <param name="polygons">A collection of polygons, where each polygon is represented as a collection of <see cref="LineString"/>
        /// objects. Each <see cref="LineString"/> defines a sequence of coordinates that form the boundary of the polygon.</param>
        public MultiPolygon(IEnumerable<IEnumerable<LineString>> polygons) : this(polygons.Select(polygon => polygon.Select(lineString => lineString.Coordinates).ToImmutableArray()).ToImmutableArray())
        { }
    }

    /// <summary>
    /// Represents a collection of geometric objects.
    /// </summary>
    /// <remarks>A <see cref="GeometryCollection"/> is a composite geometry that contains multiple geometric
    /// objects. Each geometry in the collection must be non-null. This type is immutable once initialized.</remarks>
    public class GeometryCollection : Geometry
    {
        /// <summary>
        /// Gets the collection of geometries associated with this instance.
        /// </summary>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryCollection"/> class with the specified collection of geometries.
        /// </summary>
        /// <param name="geometries">An immutable array of <see cref="Geometry"/> objects that make up the collection.</param>
        [JsonConstructor]
        public GeometryCollection(ImmutableArray<Geometry> geometries)
        {
            Geometries = geometries;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryCollection"/> class with the specified collection of geometries.
        /// </summary>
        /// <param name="geometries">A collection of <see cref="Geometry"/> objects to include in the geometry collection.  This parameter cannot
        /// be null, and all elements in the collection must be non-null.</param>
        public GeometryCollection(IEnumerable<Geometry> geometries) : this([.. geometries ?? throw new ArgumentNullException(nameof(geometries))])
        { }
    }
}
