using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using MCSlimeClusterFinder;

namespace Tests
{
    [TestClass]
    public class Misc
    {
        [TestMethod]
        public void TestAgainstJavaRandomOutput()
        {
            // Taken from branch JavaUtilRandomTest
            // I don't want to flesh out the Program class with all these debugging values, so
            // this is a reference. It shouldn't be a problem anyway.
            // Just don't touch isSlimeChunk. Easy...Right?
            string[] results = File.ReadAllLines("randomOutput.txt");
            foreach (string[] r in results.Select(r => r.Split(',')))
            {
                int inputSeedr = int.Parse(r[0]);
                long initSeedr = long.Parse(r[1]);
                int nextIntr = int.Parse(r[2]);
                long finalSeedr = long.Parse(r[3]);

                long initSeed = (inputSeedr ^ 0x5DEECE66DL) & ((1L << 48) - 1);
                (int nextInt, long finalSeed) = this.nextInt(initSeed);
                if (initSeedr != initSeed || nextIntr != nextInt || finalSeedr != finalSeed)
                {
                    throw new Exception();
                }
            }
        }
        (int, long) nextInt(long seed)
        {
            int bits, val;
            do
            {
                seed = (seed * 0x5DEECE66DL + 0xBL) & ((1L << 48) - 1);
                bits = (int)((ulong)seed >> 17);
                val = bits % 10;
            } while (bits - val + 9 < 0);
            return (val, seed);
        }
        [TestMethod]
        public void TestArgParsing()
        {
            Program p;
            p = Program.ParseArgs(new string[] { });
            Assert.IsNull(p);
            p = Program.ParseArgs(new string[] { "-s=420" });
            Assert.IsNotNull(p);
            p = Program.ParseArgs(new string[] { "-l", "2300", "--seed", "420", "-t=8" });
            Assert.IsNotNull(p);
        }
    }
}
