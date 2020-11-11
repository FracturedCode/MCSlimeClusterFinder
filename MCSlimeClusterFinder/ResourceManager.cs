using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace MCSlimeClusterFinder
{
    internal static class ResourceManager
    {
        public static string OpenClKernels => GetResource("kernels.cl");

        public static string Readme => GetResource("README-copy.md");

        private static string GetResource(string name) =>
            new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("MCSlimeClusterFinder."+name)).ReadToEnd();
    }
}
