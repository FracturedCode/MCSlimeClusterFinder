using System;
using System.Collections.Generic;
using System.Text;

namespace MCSlimeClusterFinder.Output
{
    public record Result
    {
        public Result(long xChunkCoord, long zChunkCoord, double chunksInRange) => (x, z, chunks) = (xChunkCoord, zChunkCoord, chunksInRange);

        public long x { get; }
        public long z { get; }
        public double chunks { get; }
    }
}
