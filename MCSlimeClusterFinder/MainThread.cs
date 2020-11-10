using Mono.Options;
using MoreLinq;
using OpenCL.NetCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;

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
                bool deviceInput = false;

                var options = new OptionSet
                {
                    { "s|seed=", "world seed, type long", (long s) => {stng.WorldSeed = s; seedInput = true; } },
                    { "i|in=", "input file to continue saved work", i => inputFile = i },
                    { "o|out=", "file to save the results",  o => stng.OutputFile = o },
                    { "h|help", "show this message and exit", h => shouldShowHelp = h != null },
                    { "start=", "work group step to start at. Learn more in readme (-r)", (long s) => stng.Start = s },
                    { "stop=", "work group step to stop at. Learn more in readme (-r)", (long s) => stng.Stop = s },
                    { "w|work-size=", "length of the square chunk of work sent to the GPU at once less than 2^14", (short w) => stng.GpuWorkChunkDimension = w  },
                    { "r|readme", "print the readme and exit", r => printReadme = r != null },
                    { "d|device=", "the index of the OpenCL device to use", (int d) => { stng.Device = OpenCLWrapper.GetDevices()[d]; deviceInput = true; } }
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
                    Console.WriteLine(getOptionsOutputString(System.IO.File.ReadAllText("README-copy.md")));
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
                if (!deviceInput)
                {
                    try
                    {
                        List<Device> devices = OpenCLWrapper.GetDevices();
                        string output = devices.Select((d, i) => $"[{i}]: {d.DeviceInfoLine()}").Aggregate((a, b) => $"{a}\n{b}");
                        Console.Write("Devices:\n\n" + output + "\nSelect a device index: ");
                        int index = int.Parse(Console.ReadLine());
                        stng.Device = devices[index];
                    } catch (Exception)
                    {
                        Console.WriteLine("Invalid device number selected");
                        return false;
                    }
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
        private const string optionsHeader = "\nUsage: MCSlimeClusterFinder -s WORLD_SEED [OPTIONS]\n\n";
        private const string optionsFooter = "\nTry `MCSlimeClusterFinder --help' for more information.\n";

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