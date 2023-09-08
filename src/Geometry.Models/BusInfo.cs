using NetTopologySuite.Geometries;

namespace Geomertry.Models
{
    public class BusInfo
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public Point Location { get; set; }
    }
}
