using MoreLinq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using MCSlimeClusterFinder.Output;
using System.Threading.Tasks;

namespace MCSlimeClusterFinder
{
    public class Supervisor
    {
        public bool IsCompleted { get; protected set; }
        private Progress settingsResults { get; }
        private Settings settings => settingsResults.Settings;
        private Results results => settingsResults.Results;
        public Supervisor(Progress sr)
        {
            settingsResults = sr;
        }
        public void Pause() => throw new NotImplementedException();
        public async Task Run()
        {
            var opencl = new OpenCLWrapper(settings.GpuWorkChunkDimension, settings.Device, settings.WorldSeed);
            long start = getnFromWorkRadius(settings.Start / settings.GpuWorkChunkDimension);
            long stop = getnFromWorkRadius(settings.Stop / settings.GpuWorkChunkDimension);
            for (long i = start; i < stop; i++)
            {
                var coords = scaleByWorkSize(getSpiralCoords(i));
                await Task.Run(() => opencl.Work(coords)).ConfigureAwait(false);
                opencl.candidates.ForEach((c, id) =>
                {
                    if (c >= settings.CandidateThreshold)
                    {
                        (long x, long z) = unflattenPosition(id, coords);
                        results.UncheckedCandidates.Add(new Result(x, z, c));
                    }
                });
            }
            IsCompleted = true;
        }
        private (long, long) getSpiralCoords(long n)
        {
            long k = (long)Math.Ceiling((Math.Sqrt(n) - 1) / 2.0);
            long t = (2 * k) + 1;
            long m = t * t;
            t--;
            if (n >= m - t)
                return (k - (m - n), k);
            else m -= t;
            if (n >= m - t)
                return (-k, -(-k + (m - n)));
            else m -= t;
            if (n >= m - t)
                return (-k + (m - n), -k);
            else return (k, -(k - (m - n - t)));
        }
        
        // According to getSpiralCoords where x == z
        // k - (2k+1)^2 - n = x
        // k-m-n == k according to x == z
        // therefore k == x
        // and n = (2x+1)^2 where x == z
        // also if you think about it it's a square that increases 2 in length every spiral
        private long getnFromWorkRadius(long x)
            => (2 * x + 1) * (2 * x + 1);
        private (long, long) scaleByWorkSize((long x, long z) input)
            => (input.x * settings.GpuWorkChunkDimension, input.z * settings.GpuWorkChunkDimension);
        private (long x, long z) unflattenPosition(int id, (long x, long z) startingPos)
        {
            int rowSize = (int)Math.Sqrt(settings.GpuWorkChunkDimension);
            return ((id / rowSize) + startingPos.x, (id % rowSize) + startingPos.z);
        }
    }
}
