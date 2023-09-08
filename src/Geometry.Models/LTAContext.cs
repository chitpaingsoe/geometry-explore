using Microsoft.EntityFrameworkCore;

namespace Geomertry.Models
{
    public class LTAContext : DbContext
    {
        public LTAContext() { }
        public LTAContext(DbContextOptions<LTAContext> options) : base(options){ }

        public DbSet<BusInfo> BusInfos { get; set; }

    }
}
