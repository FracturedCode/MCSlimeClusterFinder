using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace MCSlimeClusterFinder.Tests
{
    [TestClass]
    public class Benchmark : Program
    {
        private const int _testLength = 1000;   // In chunks
        private const long _worldSeed = 420;
        [TestMethod]
        public static void HashVsRuntimeCalc()
        {

            Time(TestHashMethod);
            Time(TestRuntimeCalc);
        }

        [TestMethod]
        public static void TestHashMethod()
        {

        }
        [TestMethod]
        public static void TestRuntimeCalc()
        {

        }
        
    }
}
