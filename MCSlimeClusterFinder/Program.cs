using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using MoreLinq;

// Author FracturedCode

// This algorithm isn't perfect, it's just meant as a starting point to find areas that will be useful.
// It all depends on where the player stands to maximize the number of spawning platforms.
// For instance, one of my outputs was labeled as 56, but when tested in the minecraft world, had 58 chunks in range.

// What this algorithm does, to be specific, is iterate over every chunk in the area from -_length to +_length
// Each iteration, a total count of every slime chunk within about a 128 block range is created.
// If the count is >= _threshold, it gets noted.
// These iterations are split into certain work units and passed off to other threads, speeding up the process by an order of magnitude.
// a 200,000 block radius took over an hour. 
// I spent all that effort exporting from java and importing chunks to c# because I don't like java. I just needed its Random class.
// Here's a sample output (with incorrect times because I messed up the ToString())

/*
Importing chunks...Chunks imported in 03:02.527
Brute force searching...
Starting 8 threads
Aggregate: 99.00%   Individual: 99%     99%     99%     99%     99%     99%     99%     99%
Brute force search complete in 07:22.204 using a maximum of 8.293084432GB of memory
Found 2255 candidates with a max of 56 slime chunks.
2550, -3627, 56
2775, -1115, 56
-30, -10891, 56
-9068, -6347, 55
-9067, -6347, 55
-11783, 4804, 55
...
...
-31, -10897, 49
Program ran in 10:24.875
*/

namespace MCSlimeClusterFinder
{
    public class Program
    {
        private const int _length = 1600000;
        protected const int _threshold = 45;
        private const int _threadCount = 8; // with the POWA OF AMD, I SUMMON *YOU*! RYZEN 3600
        private const long _worldSeed = 423338365327502521;
        public static void Main()
        {
            var p = new Program();
            Time(p.Run, "Total runtime");
        }

        protected static List<(int x, int z)> _deltas { get; } = CreateDeltas();
        private int _chunkHalfLength { get; } = _length / 32;
        public List<(int x, int z, int sc)> Candidates { get; } = new List<(int x, int z, int sc)>();

        
        public void Run()
        {
            Time(BruteForceAllTheChunksLMFAO);
            SaveAndPrintOutput();
        }
        
        static List<(int, int)> CreateDeltas()
        {
            // Creates deltas of chunks within 128 blocks radius for any generic chunk.
            var deltas = new List<(int, int)>();
            for (int i = -8; i < 9; i++)
            {
                for (int j = -8; j < 9; j++)
                {
                    // 128 / 16 = 8, but The rule is 9 chunks away because
                    // some chunks can have edges inside that distance and we want to capture those
                    if (Math.Sqrt(Math.Pow(i, 2) + Math.Pow(j, 2)) <= 8.0)
                        deltas.Add((i, j));
                }
            }
            return deltas;
        }

        private void SaveAndPrintOutput()
        {
            string output = $"Found {Candidates.Count} candidates with a max of {Candidates.Max(c => c.sc)} slime chunks.\n";
            var individualOrdered = Candidates.OrderByDescending(c => c.sc).Select(c => $"{c.x}, {c.z}, {c.sc}");
            string fileOutput = individualOrdered.Aggregate((x, y) => $"{x}\n{y}");
            Console.Write(output + "Saving...");
            File.WriteAllText("candidates.txt", output + fileOutput);
            Console.WriteLine("Complete\nTop 10 List:\n" + individualOrdered.Take(10).Aggregate((x, y) => $"{x}\n{y}"));
        }

        void BruteForceAllTheChunksLMFAO()
        {
            Console.WriteLine($"Brute force searching. Starting {_threadCount} threads");

            var threadObjects = new ThreadParams[_threadCount];
            int sectionLength = (_chunkHalfLength * 2 - 16) / _threadCount;

            for (int i = 0; i < _threadCount; i++)
            {
                // We're moving a circle, not a point around.
                // This means we have to adjust by 8 chunks so we don't go searching through data we don't have.
                // Hence the ternary and the +/-8
                threadObjects[i] = new ThreadParams()
                {
                    StartX = -_chunkHalfLength + 8 + i * sectionLength,
                    StopX = i == _threadCount - 1 ? _chunkHalfLength - 8 : -_chunkHalfLength + 8 + ((i + 1) * sectionLength),
                    ChunkHalfLength = _chunkHalfLength
                };
                var th = new Thread(WorkerThread);
                th.Start(threadObjects[i]);
            }

            double threadWeight = 1.0 / _threadCount;
            long greatestTotalMemory = 0;
            while (!threadObjects.All(tp => tp.Complete))
            {
                Thread.Sleep(200);
                string threadPercentLine = "";
                double percentComplete = 0.0;
                threadObjects.ForEach(tp =>
                {
                    percentComplete += tp.PercentComplete / 100.0 * threadWeight;
                    threadPercentLine += $"\t{tp.PercentComplete}%";
                });

                if (GC.GetTotalMemory(false) > greatestTotalMemory)
                    greatestTotalMemory = GC.GetTotalMemory(false);

                string output = $"\rAggregate: {percentComplete:P}   Individual:" + threadPercentLine;
                Console.Write(output);
            }

            Console.WriteLine($"\nBrute force search complete using a maximum of {(greatestTotalMemory/(double)1000000000):0.##}GB of memory");
        }

        public class ThreadParams
        {
            public int StartX;
            public int StopX;
            public int ChunkHalfLength;
            public int PercentComplete;
            public bool Complete;
        }
        protected void WorkerThread(Object param)
        {
            ThreadParams tParams = (ThreadParams)param;

            int startX = tParams.StartX;    // Optimizing by putting oft-used vars on the heap, maybe it's unnecessary, but I assume compiler or interpreter don't do this automagically
            int stopX = tParams.StopX;
            int diff = stopX - startX;
            int chunkHalfLength = tParams.ChunkHalfLength;

            for (int i = startX; i < stopX; i++)
            {
                if ((int)((i - startX) / (double)(diff) * 100) > tParams.PercentComplete)
                    tParams.PercentComplete++; // TODO Only works with large borders, ie > a thousand

                for (int j = -chunkHalfLength + 8; j < chunkHalfLength / 2 - 7; j++)
                {
                    int slimeRadiusCounter = 0;
                    foreach (var delta in _deltas)
                    {
                        if (isSlimeChunk(i + delta.x, j + delta.z))
                            slimeRadiusCounter++;
                    }
                    if (slimeRadiusCounter >= _threshold)
                        Candidates.Add((i, j, slimeRadiusCounter));
                }
            }
            tParams.Complete = true;
        }
        protected static bool isSlimeChunk(int x, int z)
        {
            // Implementation of this from java:
            // new Random(seed + (long) (i * i * 4987142) + (long) (i * 5947611) + (long) (j * j) * 4392871L + (long) (j * 389711) ^ 987234911L).nextInt(10) == 0
            long seed = ((_worldSeed + (long) (x * x * 4987142) + (long) (x * 5947611) + (long) (z * z) * 4392871L + (long) (z * 389711) ^ 987234911L) ^ 0x5DEECE66DL) & ((1L << 48) - 1);
            int bits, val;
            do
            {
                seed = (seed * 0x5DEECE66DL + 0xBL) & ((1L << 48) - 1);
                bits = (int)((ulong)seed >> 17);
                val = bits % 10;
            } while (bits - val + 9 < 0);
            return val == 0;
        }

        protected static TimeSpan Time(Action action, string actionName = null)
        {
            var sw = new Stopwatch();
            sw.Start();
            action.Invoke();
            sw.Stop();
            Console.WriteLine($"{actionName ?? action.Method.Name} completed in {sw.Elapsed:hh\\:mm\\:ss\\.fff}");
            return sw.Elapsed;
        }
    }
}
