using System;
using System.Collections.Generic;
using System.Text;

namespace MCSlimeClusterFinder.Output
{
    public record Result
    {
        public Result(long X, long Z, double Chunks) => (x, z, chunks) = (X, Z, Chunks);

        public long x { get; }
        public long z { get; }
        public double chunks { get; }
    }
}
