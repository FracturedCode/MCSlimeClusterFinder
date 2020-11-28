using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Tests;
using MCSlimeClusterFinder.Output;
using MoreLinq;

namespace MCSlimeClusterFinder.Tests
{
    [TestClass]
    public class Benchmark : Supervisor
    {
        public Benchmark() : base(new Progress()) { }

        [TestMethod]
        public async Task benchmarkOldVsNewRun()
        {
            settings.GpuWorkChunkDimension = 7000;
            settings.Stop = 100000;
            settings.Device = OpenCLWrapper.GetDevices()[0];
            var sw = new Stopwatch();
            sw.Start();
            await oldRun();
            sw.Stop();
            Debug.WriteLine(sw.ElapsedMilliseconds);
            sw.Reset();
            sw.Start();
            await newRun();
            sw.Stop();
            Debug.WriteLine(sw.ElapsedMilliseconds);
        }
        [TestMethod]
        public async Task oldRun()
        {
            settings.Device = OpenCLWrapper.GetDevices()[0];
            var opencl = new OpenCLWrapper(settings.GpuWorkChunkDimension, settings.Device, settings.WorldSeed);
            long start = getnFromWorkRadius(getWorkRadius(settings.Start));
            long stop = getnFromWorkRadius(getWorkRadius(settings.Stop));
            for (long i = start; i <= stop; i++)
            {
                var coords = scaleByWorkSize(getSpiralCoords(i));
                await opencl.WorkAsync(coords).ConfigureAwait(false);
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
        [TestMethod]
        public async Task newRun()
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
    }
}
