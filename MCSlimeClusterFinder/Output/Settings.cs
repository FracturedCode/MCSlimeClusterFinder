using Mono.Options;
using OpenCL.NetCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCSlimeClusterFinder.Output
{
    public class Settings
    {
        public long Start { get; set; } = 0;
        public long Stop { get; set; } = 500000;
        public short GpuWorkChunkDimension { get; set; } = 5000;
        public int CandidateThreshold { get; set; } = 56;
        public long WorldSeed { get; set; }
        public string OutputFile { get; set; } = @"slimeResults.log";
        public Device Device { get; set; }
    }
}