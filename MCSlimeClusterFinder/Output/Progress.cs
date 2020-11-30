using System;
using System.Collections.Generic;
using System.Text;

namespace MCSlimeClusterFinder.Output
{
    public class Progress
    {
        public decimal RatioComplete { get; set; }
        public Settings Settings { get; set; } = new Settings();
        public Results Results { get; set; } = new Results();
        public long CurrentN { get; set; }
    }
}
