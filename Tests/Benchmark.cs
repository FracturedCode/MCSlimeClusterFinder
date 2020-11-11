using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Tests;

namespace MCSlimeClusterFinder.Tests
{
    [TestClass]
    public class Benchmark : Program
    {
        private const int _testLength = 1000;   // In chunks, must be even
        public Benchmark() : base(_testLength, 420, 8, "benchmarkOut.log") { }
        /*Debug Trace:
        <HashVsRuntimeCalc>b__0 ran in 00:00:06.714
        TestRuntimeCalc ran in 00:00:03.931*/
        [TestMethod]
        public void BenchmarkHashVsRuntimeCalc()
        {
            HashSet<(int, int)> slimeChunks = new HashSet<(int, int)>();
            {
                for (int i = -_testLength / 2 - 8; i<_testLength / 2 - 7; i++)
                {
                    for (int j = -_testLength / 2 + 8; j < _testLength / 2 - 8; j++)
                    {
                        if (isSlimeChunk(i, j))
                            slimeChunks.Add((i, j));
                    }
                }
            }   //Even not considering the 3 minutes it can take to create the hashset, I bet calculating if deltas are slime chunks at runtime will be faster than checking in the hashset
            Time(() => TestHashMethod(slimeChunks), nameof(TestHashMethod));
            Time(TestRuntimeCalc);
        }

        public void TestHashMethod(HashSet<(int, int)> slimeChunks)
        {
            List<(int x, int z, int sc)> candidates = new List<(int x, int z, int sc)>();
            int startX = -_testLength / 2 - 8;
            int stopX = _testLength / 2 - 7;
            

            for (int i = startX; i < stopX; i++)
            {

                for (int j = -_testLength / 2 + 8; j < _testLength / 2 - 8; j++)
                {
                    int slimeRadiusCounter = 0;
                    foreach (var delta in _deltas)
                    {
                        if (slimeChunks.Contains((i + delta.x, j + delta.z)))
                            slimeRadiusCounter++;
                    }
                    if (slimeRadiusCounter >= _threshold)
                        candidates.Add((i, j, slimeRadiusCounter));
                }
            }
        }
        [TestMethod]
        public void TestRuntimeCalc()
        {
            List<(int x, int z, int sc)> candidates = new List<(int x, int z, int sc)>();
            int startX = -_testLength / 2 - 8;
            int stopX = _testLength / 2 - 7;
            for (int i = startX; i < stopX; i++)
            {
                for (int j = -_testLength / 2 + 8; j < _testLength / 2 - 8; j++)
                {
                    int slimeRadiusCounter = 0;
                    foreach (var delta in _deltas)
                    {
                        if (isSlimeChunk(i + delta.x, j + delta.z))
                            slimeRadiusCounter++;
                    }
                    if (slimeRadiusCounter >= _threshold)
                        candidates.Add((i, j, slimeRadiusCounter));
                }
            }
        }
        [TestMethod]
        public void TestWorkerThread()
        {
            var tp = new ThreadParams()
            {
                StartX = -_testLength / 2 - 8,
                StopX = -_testLength / 2 - 7,
                ChunkHalfLength = _testLength / 2
            };
            Time(() => WorkerThread(tp), nameof(WorkerThread));
        }

        [TestMethod]
        public void TestNoMemoryDeltas()
        {
            List<(int x, int z, int sc)> candidates = new List<(int x, int z, int sc)>();
            int startX = -_testLength / 2 - 8;
            int stopX = _testLength / 2 - 7;
            for (int x = startX; x < stopX; x++)
            {
                for (int z = -_testLength / 2 + 8; z < _testLength / 2 - 8; z++)
                {
                    int slimeRadiusCounter = 0;
                    for (int i = -8; i<9; i++)
                    {
                        for (int j = -8; j<9; j++)
                        {
                            if ((Math.Sqrt(i * i + j * j) <= 8.0) && isSlimeChunk(x + i, z + j))
                                slimeRadiusCounter++;
                        }
                        
                    }
                    if (slimeRadiusCounter >= _threshold)
                        candidates.Add((x, z, slimeRadiusCounter));
                }
            }
        }
        [TestMethod]
        public void BenchmarkNoMemoryVsMemoryDeltas()
        {
            Time(TestNoMemoryDeltas);//storing the deltas wins this benchmark
            Time(TestRuntimeCalc);
        }
        [TestMethod]
        public void BenchmarkSplitVsChunk()
        {
            var sw = new Stopwatch();
            sw.Start();
            new OpenCLTests().TestSlimeFinder();
            sw.Stop();
            
        }
    }
}
