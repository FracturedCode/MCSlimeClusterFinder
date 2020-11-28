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
        protected Progress settingsResults { get; }
        protected Settings settings => settingsResults.Settings;
        protected Results results => settingsResults.Results;
        public Supervisor(Progress sr)
        {
            settingsResults = sr;
        }
        public void Pause() => throw new NotImplementedException();
        public async Task Run()
        {
            settings.Device = OpenCLWrapper.GetDevices()[0];
            var wrappers = new OpenCLWrapper[]
            {
                new OpenCLWrapper(settings.GpuWorkChunkDimension, settings.Device, settings.WorldSeed),
                new OpenCLWrapper(settings.GpuWorkChunkDimension, settings.Device, settings.WorldSeed)
            };
            int currentWrapper = 0;
            long start = getnFromWorkRadius(getWorkRadius(settings.Start));
            long stop = getnFromWorkRadius(getWorkRadius(settings.Stop));
            for (long i = start; i <= stop; i++)
            {
                var coords = scaleByWorkSize(getSpiralCoords(i));
                Task gpuWork = wrappers[currentWrapper].WorkAsync(coords);
                currentWrapper = currentWrapper == 0 ? 1 : 0;
                wrappers[currentWrapper].candidates.ForEach((c, id) =>
                {
                    if (c >= settings.CandidateThreshold)
                    {
                        (long x, long z) = unflattenPosition(id, coords);
                        results.UncheckedCandidates.Add(new Result(x, z, c));
                    }
                });
                await gpuWork.ConfigureAwait(false);
            }
            IsCompleted = true;
        }
        protected (long, long) getSpiralCoords(long n)
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
        protected long getnFromWorkRadius(long x)
            => (2 * x + 1) * (2 * x + 1);
        protected (long, long) scaleByWorkSize((long x, long z) input)
            => (input.x * settings.GpuWorkChunkDimension, input.z * settings.GpuWorkChunkDimension);
        protected (long x, long z) unflattenPosition(int id, (long x, long z) startingPos)
        {
            int rowSize = (int)Math.Sqrt(settings.GpuWorkChunkDimension);
            return ((id / rowSize) + startingPos.x, (id % rowSize) + startingPos.z);
        }
        protected long getWorkRadius(long blockRadius) => (int)Math.Ceiling(blockRadius / 16.0) / settings.GpuWorkChunkDimension;
    }
}
