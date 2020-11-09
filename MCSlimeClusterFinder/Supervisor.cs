using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace MCSlimeClusterFinder
{
    public class Supervisor
    {
        public bool Completed { get; protected set; }
        private SettingsResults settingsResults { get; }
        private Settings settings => settingsResults.Settings;
        private Thread thread { get; }
        public Supervisor(SettingsResults sr)
        {
            settingsResults = sr;
            thread = new Thread(run);
        }
        public void Start() => thread.Start();
        public void Pause() => throw new NotImplementedException();
        public void Abort() => thread.Abort(); // An extreme step
        private void run()
        {
            var opencl = new OpenCLWrapper(settings.GpuWorkChunkDimension);
            opencl.Ready();
            for (long i = settings.Start; i < settings.Stop; i++)
            {
                opencl.Step();
            }
            Completed = true;
        }
    }
}
