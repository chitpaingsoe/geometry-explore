using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Geometries;
using GeomertryDev.Dtos;
using System.Diagnostics;
using GeometryDev;
using StackExchange.Redis;
using Geomertry.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GeomertryDev.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BusController : ControllerBase
    {
        private LTAContext _context;

        private IDatabase _cache;
        public BusController(LTAContext context, IDatabase cache) {
            _context = context;
            _cache = cache;
        }
        // GET: api/<BusController>
        [HttpGet]
        public ActionResult Get()
        {

            var buses = _context.BusInfos
                .Select(x => new { ID = x.Id, Name = x.Name, Location = x.Location.ToString() })
                .ToArray();
            return Ok(buses);
        }


        [HttpPost("Seed")]
        public void Seed()
        {
            var buses = _context.BusInfos
                .Select(x => new { ID = x.Id, Name = x.Name, Lon = x.Location.X, Lat = x.Location.Y });
            foreach(var bus in buses)
            {
                string Script = $"redis.call('GEOADD', @key,  @lon, @lat, @name)";

                var prepared = LuaScript.Prepare(Script);
                var result = _cache.ScriptEvaluate(prepared, new { key = (RedisKey)"buses", lon = bus.Lon, lat = bus.Lat, name = bus.ID });
              
            }
        }

        [HttpPost("Search")]
        public async Task<ActionResult> Search([FromBody] SearchDto bound)
        {

            var box = ConvertBoundToPolygon(bound);

            var stw = new Stopwatch();

            stw.Start();

            var buses = _context.BusInfos
                .Where(x => box.Contains(x.Location))
                .Select(x => new { ID = x.Id, Name = x.Name, Lat = x.Location.Y, Lng = x.Location.X }).ToList();
            var elapsed = stw.Elapsed;

            //Response.ContentType = "application/json";
            //StreamWriter sw;
            //await using ((sw = new StreamWriter(Response.Body)).ConfigureAwait(false))
            //{
            //    foreach (var item in buses)
            //    {
            //        await sw.WriteLineAsync(JsonConvert.SerializeObject(item)).ConfigureAwait(false);
            //        await sw.FlushAsync().ConfigureAwait(false);
            //    }
            //}

            return Ok(new { executionTime = elapsed.TotalMilliseconds, data = buses });
        }
        [HttpPost("SearchInCache")]
        public ActionResult SearchInCache([FromBody] SearchDto bound)
        {

           var box = ConvertBoundToPolygon(bound);

            var centroid = box.Centroid;


            var point1 = new Point(bound.X1, bound.Y2) { SRID = 4326 };
            var point2 = new Point(bound.X1, bound.Y1) { SRID = 4326 };
            var point3 = new Point(bound.X2, bound.Y1) { SRID = 4326 };  

            var height = DistanceHelper.GreatCircleDistance(point1.X, point1.Y, point2.X, point2.Y);
            var width = DistanceHelper.GreatCircleDistance(point2.X, point2.Y, point3.X, point3.Y);


            var sw = new Stopwatch();

            sw.Start();

            var data = _cache.GeoSearch("buses", centroid.X, centroid.Y, new GeoSearchBox(height, width, GeoUnit.Meters))
                .Select(x => new { ID = x.Member.ToString(), Name = x.Member.ToString(), Lat = x.Position.Value.Latitude, Lng = x.Position.Value.Longitude}).ToList();

            var elapsed = sw.Elapsed;
            return Ok(new { executionTime = elapsed.TotalMilliseconds, data });
        }

        // POST api/<BusController>
        [HttpPost]
        public void Post([FromBody] BusInfoDto[] payload)
        {

            foreach(var value in payload)
            {
                var bus = new BusInfo();
                bus.Name = value.Name;
                bus.Location = new Point(value.Longitude, value.Latitude) { SRID = 4326 };
                _context.BusInfos.Add(bus);
            }
            _context.SaveChanges();
        }

        // PUT api/<BusController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<BusController>/5
        [HttpDelete]
        public void Delete()
        {
            var buses = _context.BusInfos;
            _context.BusInfos.RemoveRange(buses);
            _context.SaveChanges();
        }

        private Polygon ConvertBoundToPolygon(SearchDto bound)
        {

            var x1 = bound.X1;
            var y1 = bound.Y1;
            var x2 = bound.X2;
            var y2 = bound.Y2;

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
