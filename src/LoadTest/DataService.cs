using Geomertry.Models;
using NetTopologySuite.Geometries;
using StackExchange.Redis;
using System.Diagnostics;

namespace LoadTest
{
    public class DataService
    {

        private LTAContext _context;
        private IDatabase _cache;
        public DataService(LTAContext context, IDatabase cache)
        {
            _context = context;
            _cache = cache;
        }

        public QueryResult GetFromDb(BoundV1 bound)
        {
            var box = ConvertBoundToPolygon(bound);

            var sw = new Stopwatch();

            sw.Start();

            var buses = _context.BusInfos
                .Where(x => box.Contains(x.Location))
                .Select(x => new { ID = x.Id, Name = x.Name, Lat = x.Location.Y, Lng = x.Location.X });
            var elapsed = sw.Elapsed;
            return new QueryResult { ExecutionTime = elapsed.TotalMilliseconds, Data = buses };
        }
        public QueryResult GetFromCache(BoundV1 bound)
        {
            var box = ConvertBoundToPolygon(bound);

            var centroid = box.Centroid;


            var point1 = new Point(bound.west, bound.north) { SRID = 4326 };
            var point2 = new Point(bound.west, bound.south) { SRID = 4326 };
            var point3 = new Point(bound.east, bound.south) { SRID = 4326 };

            var height = DistanceHelper.GreatCircleDistance(point1.X, point1.Y, point2.X, point2.Y);
            var width = DistanceHelper.GreatCircleDistance(point2.X, point2.Y, point3.X, point3.Y);


            var sw = new Stopwatch();

            sw.Start();

            var data = _cache.GeoSearch("buses", centroid.X, centroid.Y, new GeoSearchBox(height, width, GeoUnit.Meters))
                .Select(x => new { ID = x.Member.ToString(), Name = x.Member.ToString(), Lat = x.Position.Value.Latitude, Lng = x.Position.Value.Longitude });

            var elapsed = sw.Elapsed;

            return new QueryResult { ExecutionTime = elapsed.TotalMilliseconds, Data = data.AsQueryable() };
        }

        private Polygon ConvertBoundToPolygon(BoundV1 bound)
        {

            var x1 = bound.west;
            var y1 = bound.south;
            var x2 = bound.east;
            var y2 = bound.north;

            var cord5 = new Coordinate(x1, y2);
            var cord4 = new Coordinate(x2, y2);
            var cord3 = new Coordinate(x2, y1);
            var cord2 = new Coordinate(x1, y1);
            var cord1 = new Coordinate(x1, y2);

            var coordinates = new List<Coordinate>();
            coordinates.Add(cord1);
            coordinates.Add(cord2);
            coordinates.Add(cord3);
            coordinates.Add(cord4);
            coordinates.Add(cord5);


            var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
            var box = new Polygon(new LinearRing(coordinates.ToArray()), geometryFactory);
            return box;
        }
    }
}
