using Geomertry.Models;
using Microsoft.EntityFrameworkCore;


namespace Geometry.Models
{
    public class LTADbContextFactory : IDbContextFactory<LTAContext>
    {
        private readonly IDbContextFactory<LTAContext> _pooledFactory;
        public LTADbContextFactory(
        IDbContextFactory<LTAContext> pooledFactory)
        {
            _pooledFactory = pooledFactory;
        }
        public LTAContext CreateDbContext()
        {
            var context = _pooledFactory.CreateDbContext();
            return context;
        }
    }
}
