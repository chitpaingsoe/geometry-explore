using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadTest
{
    public class QueryResult
    {
        public double ExecutionTime { get; set; }
        public IQueryable<dynamic> Data { get; set; }
    }
}
