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
using MCSlimeClusterFinder.Output;
using MCSlimeClusterFinder.Resources;
using System.Runtime.CompilerServices;

namespace MCSlimeClusterFinder
{
    public static class MainThread
    {
        private static Supervisor workSupervisor { get; set; }
        private static Progress settingsResults { get; set; } = new Progress();
        public static void Main(string[] args)
        {
            if (!parseArgs(args))
            {
                return;
            }
            workSupervisor = new Supervisor(settingsResults);
            Task work = workSupervisor.Run();
            
            outputInitialSettings();
            Console.CancelKeyPress += handleCancel;
            while (!work.IsCompleted)
            {
                outputProgress();
                Thread.Sleep(50);
            }
            saveToFile();
        }

        private static void handleCancel(object sender, ConsoleCancelEventArgs args)
        {
            saveToFile();
            System.Environment.Exit(0);
        }

        private static void saveToFile() =>
            System.IO.File.WriteAllText(settingsResults.Settings.OutputFile, JsonSerializer.Serialize(settingsResults, new JsonSerializerOptions() { WriteIndented = true }));

        private static void outputInitialSettings() =>
            Console.WriteLine(JsonSerializer.Serialize(settingsResults, new JsonSerializerOptions() { WriteIndented = true }));

        private static void outputProgress()
        {
            Console.Write($"\r{settingsResults.RatioComplete*100:###.###}");
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
                    { "i|in=", "input file to continue saved work. overrides all command line arguments except --device", i => inputFile = i },
                    { "o|out=", "file to save the results",  o => stng.OutputFile = o },
                    { "h|help", "show this message and exit", h => shouldShowHelp = h != null },
                    { "start=", "the start \"radius\" of the search area in blocks/meters", (int s) => stng.Start = s },
                    { "stop=", "the end \"radius\" of the search area in blocks/meters", (int s) => stng.Stop = s },
                    { "w|work-size=", "length of the square chunk of work sent to the GPU at once less than 2^14", (short w) => stng.GpuWorkChunkDimension = w  },
                    { "r|readme", "print the readme and exit. Includes a how-to", r => printReadme = r != null },
                    { "d|device=", "the index of the OpenCL device to use", (int d) => { stng.Device = OpenCLWrapper.GetDevices()[d]; deviceInput = true; } }
                };

                options.Parse(args);

                if (!string.IsNullOrEmpty(inputFile))
                {
                    settingsResults = JsonSerializer.Deserialize<Progress>(System.IO.File.ReadAllText(inputFile), new JsonSerializerOptions() { WriteIndented = true });
                    stng = settingsResults.Settings;
                } else
                {
                    if (shouldShowHelp)
                    {
                        Console.Write(optionsHeader);
                        options.WriteOptionDescriptions(Console.Out);
                        Console.WriteLine(optionsFooter);
                        return false;
                    }
                    if (printReadme)
                    {
                        Console.WriteLine(getOptionsOutputString(ResourceManager.Readme));
                        return false;
                    }
                    if (!seedInput)
                    {
                        Console.WriteLine(getOptionsOutputString("A world seed must be specified with -s [world seed]"));
                        return false;
                    }
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
    }
}