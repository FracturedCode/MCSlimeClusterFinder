using MoreLinq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;

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
            var opencl = new OpenCLWrapper(settings.GpuWorkChunkDimension, settings.Device);
            for (long i = settings.Start; i < settings.Stop; i++)
            {
                var coords = scaleByWorkSize(getSpiralCoords(i));
                Console.WriteLine(coords);
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
        private (long, long) scaleByWorkSize((long x, long z) input)
            => (input.x * settings.GpuWorkChunkDimension, input.z * settings.GpuWorkChunkDimension);
        private (long x, long z) unflattenPosition(int id, (long x, long z) startingPos)
        {
            int rowSize = (int)Math.Sqrt(settings.GpuWorkChunkDimension);
            return ((id / rowSize) + startingPos.x, (id % rowSize) + startingPos.z);
        }
    }
}
