namespace GeometryDev
{

    // ref: https://stackoverflow.com/questions/67062622/calculate-geographic-distance-between-points-using-nettopologysuite
    public class DistanceHelper
    {
        public static double Radians(double x)
        {
            return x * Math.PI / 180;
        }

        public static double GreatCircleDistance(double lon1, double lat1, double lon2, double lat2)
        {
            double R = 6371e3; // m

            double sLat1 = Math.Sin(Radians(lat1));
            double sLat2 = Math.Sin(Radians(lat2));
            double cLat1 = Math.Cos(Radians(lat1));
            double cLat2 = Math.Cos(Radians(lat2));
            double cLon = Math.Cos(Radians(lon1) - Radians(lon2));

            double cosD = sLat1 * sLat2 + cLat1 * cLat2 * cLon;

            double d = Math.Acos(cosD);

            double dist = R * d;

            return dist;

        }
        public static double GetDistance(double lat, double lng)
        {
            return Math.Acos(Math.Cos(37) * Math.Cos(Radians(lat)) * Math.Cos(Radians(lng)) -
                    Radians(-122) + Math.Sin(Radians(37)) * Math.Sin(Radians(lat))
                );
        }
       
    }
}
