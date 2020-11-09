using Mono.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

namespace MCSlimeClusterFinder
{
    public class MainThread
    {
        private static Supervisor workSupervisor { get; set; }
        private static SettingsResults settingsResults { get; } = new SettingsResults();
        public static void Main(string[] args)
        {
            if (!parseArgs(args))
            {
                return;
            }
            workSupervisor = new Supervisor(settingsResults);
            workSupervisor.Start();
            waitForWorkEnd();
            System.IO.File.WriteAllText(settingsResults.Settings.OutputFile, JsonSerializer.Serialize(settingsResults, new JsonSerializerOptions() { WriteIndented = true }));
        }

        private static bool parseArgs(string[] args)
        {
            var stng = settingsResults.Settings;
            try
            {
                bool seedInput = false;
                bool shouldShowHelp = false;
                bool printReadme = false;
                string inputFile = null;

                var options = new OptionSet
                {
                    { "s|seed=", "world seed, type long", (long s) => {stng.WorldSeed = s; seedInput = true; } },
                    { "i|in=", "input file to continue saved work", i => inputFile = i },
                    { "o|out=", "file to save the results",  o => stng.OutputFile = o },
                    { "h|help", "show this message and exit", h => shouldShowHelp = h != null },
                    { "start=", "work group step to start at. Learn more in readme (-r)", (long s) => stng.Start = s },
                    { "stop=", "work group step to stop at. Learn more in readme (-r)", (long s) => stng.Stop = s },
                    { "w|work-size=", "length of the square chunk of work sent to the GPU at once less than 2^14", (short w) => stng.GpuWorkChunkDimension = w  },
                    { "r|readme", "print the readme and exit", r => printReadme = r != null }
                };
                options.Parse(args);

                
                if (shouldShowHelp)
                {
                    Console.Write(optionsHeader);
                    options.WriteOptionDescriptions(Console.Out);
                    Console.WriteLine(optionsFooter);
                    return false;
                }
                if (printReadme)
                {
                    throw new NotImplementedException();
                    return false;
                }
                if (!seedInput)
                {
                    Console.WriteLine(getOptionsOutputString("A world seed must be specified with -s [world seed]"));
                    return false;
                }
                if (!string.IsNullOrEmpty(inputFile))
                {
                    throw new NotImplementedException();
                }

            } catch (OptionException e)
            {
                Console.WriteLine(getOptionsOutputString(e.Message));
                return false;
            }
            return true;
        }

        private static string getOptionsOutputString(string content) =>
            optionsHeader + content + optionsFooter;
        private const string optionsHeader = "MCSlimeClusterFinder: \nUsage: MCSlimeClusterFinder -s WORLD_SEED [OPTIONS]\n\n";
        private const string optionsFooter = "Try `MCSlimeClusterFinder --help' for more information.";

        private static void waitForWorkEnd()
        {
            while (!workSupervisor.Completed)
            {
                Thread.Sleep(100);
                //TODO progress meter
            }
        }
    }
}
