using System;
using System.Collections.Generic;
using System.Text;

namespace MCSlimeClusterFinder.Output
{
    public class Result
    {
        public Result(long x, long z, double chunks)
        {
            this.x = x;
            this.z = z;
            this.chunks = chunks;
        }
        public long x { get; set; }
        public long z { get; set; }
        public double chunks { get; set; }
    }
}
