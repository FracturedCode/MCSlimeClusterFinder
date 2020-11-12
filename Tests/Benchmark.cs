using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Tests;
using MCSlimeClusterFinder.Output;

namespace MCSlimeClusterFinder.Tests
{
    [TestClass]
    public class Benchmark : Supervisor
    {
        public Benchmark() : base(new Progress()) { }
    }
}
