using Mono.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCSlimeClusterFinder
{
    public class Settings
    {
        public long Start { get; set; } = 0;
        public long Stop { get; set; } = 100;
        public short GpuWorkChunkDimension { get; set; } = 10000;
        public long WorldSeed { get; set; }
        public string OutputFile { get; set; } = "slimeResults.log";
    }
}
