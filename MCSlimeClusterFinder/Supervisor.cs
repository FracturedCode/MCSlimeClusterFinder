using MoreLinq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using MCSlimeClusterFinder.Output;

namespace MCSlimeClusterFinder
{
    public class Supervisor
    {
        public bool Completed { get; protected set; }
        private SettingsResults settingsResults { get; }
        private Settings settings => settingsResults.Settings;
        private Results results => settingsResults.Results;
        private Thread thread { get; }
        public Supervisor(SettingsResults sr)
        {
            settingsResults = sr;
            thread = new Thread(run);
        }
        public void Start() => thread.Start();
        public void Pause() => throw new NotImplementedException();
        public void Abort() => thread.Abort(); // An extreme step
        private void run()
        {
            var opencl = new OpenCLWrapper(settings.GpuWorkChunkDimension, settings.Device, settings.WorldSeed);
            long start = getnFromWorkRadius(settings.Start / settings.GpuWorkChunkDimension);
            long stop = getnFromWorkRadius(settings.Stop / settings.GpuWorkChunkDimension);
            for (long i = start; i < stop; i++)
            {
                var coords = scaleByWorkSize(getSpiralCoords(i));
                opencl.Work(coords);
                opencl.candidates.ForEach((c, id) =>
                {
                    if (c >= settings.CandidateThreshold)
                    {
                        (long x, long z) = unflattenPosition(id, coords);
                        results.UncheckedCandidates.Add(new Result(x, z, c));
                    }
                });
            }
            Completed = true;
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
