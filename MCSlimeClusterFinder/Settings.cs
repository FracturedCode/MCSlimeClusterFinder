using Mono.Options;
using OpenCL.NetCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCSlimeClusterFinder
{
    public class Settings
    {
        public long Start { get; set; } = 0;
        public long Stop { get; set; } = 1024;
        public short GpuWorkChunkDimension { get; set; } = 256;
        public int CandidateThreshold { get; set; } = 56;
        public long WorldSeed { get; set; }
        public string OutputFile { get; set; } = @"slimeResults.log";
        public Device Device { get; set; }
    }
}