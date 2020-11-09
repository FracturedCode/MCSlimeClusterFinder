﻿using System;
using System.Collections.Generic;
using System.Text;
using OpenCL.NetCore;
using System.Linq;
using System.Runtime.InteropServices;
using MoreLinq;

namespace MCSlimeClusterFinder
{
    public class OpenCLWrapper
    {
        CommandQueue queue;
        Kernel kernel;
        IMem dataOut;
        Program program;
        Context context;
        int[] candidates;
        int squareLength;
        int globalSize;

        public OpenCLWrapper(int squareLength)
        {
            this.squareLength = squareLength;
            globalSize = squareLength * squareLength;
            candidates = new int[globalSize];
        }

        ~OpenCLWrapper()
        {
            Cl.ReleaseKernel(kernel);
            Cl.ReleaseMemObject(dataOut);
            Cl.ReleaseCommandQueue(queue);
            Cl.ReleaseProgram(program);
            Cl.ReleaseContext(context);
        }
        private void allGood(ErrorCode ec)
        {
            if (ec != ErrorCode.Success)
                throw new Exception($"OpenCL had an error: {ec}");
        }
        public void Ready()
        {
            ErrorCode error;
            Device device = (from d in
                           Cl.GetDeviceIDs(
                               (from platform in Cl.GetPlatformIDs(out error)
                                where Cl.GetPlatformInfo(platform, PlatformInfo.Name, out error).ToString() == "AMD Accelerated Parallel Processing" // Use "NVIDIA CUDA" if you don't have amd
                                select platform).First(), DeviceType.Gpu, out error)
                             select d).First();

            context = Cl.CreateContext(null, 1, new[] { device }, null, IntPtr.Zero, out error);

            string source = System.IO.File.ReadAllText("kernels.cl");
            program = Cl.CreateProgramWithSource(context, 1, new[] { source }, null, out error);

            error = Cl.BuildProgram(program, 1, new[] { device }, string.Empty, null, IntPtr.Zero);
            InfoBuffer buildStatus = Cl.GetProgramBuildInfo(program, device, ProgramBuildInfo.Status, out error);
            if (buildStatus.CastTo<BuildStatus>() != BuildStatus.Success)
                throw new Exception($"OpenCL could not build the kernel successfully: {buildStatus.CastTo<BuildStatus>()}");
            allGood(error);

            Kernel[] kernels = Cl.CreateKernelsInProgram(program, out error);
            kernel = kernels[0];
            allGood(error);

            queue = Cl.CreateCommandQueue(context, device, CommandQueueProperties.None, out error);
            allGood(error);

            dataOut = Cl.CreateBuffer(context, MemFlags.WriteOnly, (IntPtr)(globalSize * sizeof(int)), out error);
            allGood(error);

            var intSizePtr = new IntPtr(Marshal.SizeOf(typeof(int)));
            error = Cl.SetKernelArg(kernel, 0, intSizePtr, new IntPtr(0));
            error |= Cl.SetKernelArg(kernel, 1, intSizePtr, new IntPtr(0));
            error |= Cl.SetKernelArg(kernel, 2, new IntPtr(Marshal.SizeOf(typeof(IntPtr))), dataOut);
            error |= Cl.SetKernelArg(kernel, 3, intSizePtr, new IntPtr(420));
            error |= Cl.SetKernelArg(kernel, 4, intSizePtr, new IntPtr(globalSize));
            allGood(error);
        }

        public void Step()
        {
            ErrorCode error;
            int local_size = 256;
            int global_size = (int)Math.Ceiling(globalSize / (float)local_size) * local_size;
            error = Cl.EnqueueNDRangeKernel(queue, kernel, 1, null, new IntPtr[] { new IntPtr(global_size) }, new IntPtr[] { new IntPtr(local_size) }, 0, null, out Event clevent);
            allGood(error);
            Cl.EnqueueReadBuffer(queue, dataOut, Bool.False, IntPtr.Zero, (IntPtr)(globalSize * sizeof(int)), candidates, 0, null, out clevent);
            Cl.Finish(queue);
            string output = "";
            candidates.ForEach(c =>
            {
                if (c > 50)
                    output += $"{c},";
            });
            Console.WriteLine(output);
        }
    }
}
