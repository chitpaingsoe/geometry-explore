
namespace LoadTest
{
    public class TrackSimulator
    {
        public TrackSimulator() {
           
        }

        private BoundV1 getBaseBound()
        {

            var originalBound = new BoundV1();
            originalBound.east = 103.85478322213977;
            originalBound.north = 1.3401865597981608;
            originalBound.south = 1.3203864665473288;
            originalBound.west = 103.8365442008385;

            return originalBound;

        }
        public BoundV1 GetBound()
        {
            var rand = new Random();
            var item = new decimal(rand.NextDouble());

            var originalBound = getBaseBound();

            originalBound.east += (double)item;
            originalBound.north += (double)item;
            originalBound.south += (double)item;
            originalBound.west += (double)item;

            return originalBound;
        }
    }
}
