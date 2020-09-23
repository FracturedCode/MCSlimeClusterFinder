using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenCL.NetCore;
using OpenCL.NetCore.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace Tests
{
    [TestClass]
    public class OpenCLTests
    {
        [TestMethod]
        public void Prototype()
        {
            ErrorCode error;
            Device device = (from d in
                           Cl.GetDeviceIDs(
                               (from platform in Cl.GetPlatformIDs(out error)
                                where Cl.GetPlatformInfo(platform, PlatformInfo.Name, out error).ToString() == "AMD Accelerated Parallel Processing" // Use "NVIDIA CUDA" if you don't have amd
                                select platform).First(), DeviceType.Gpu, out error)
                       select d).First();

            Context context = Cl.CreateContext(null, 1, new[] { device }, null, IntPtr.Zero, out error);

            string source = System.IO.File.ReadAllText("kernels.cl");

            int chunkHalfLength = 3000000;
            int worldSeed = 420;
            int workItems = 3000;
            int outputAllocation = 100;
            IntPtr outputSize = new IntPtr(workItems * outputAllocation);

            var xr = new int[outputSize.ToInt32()];
            var zr = new int[outputSize.ToInt32()];
            var sc = new int[outputSize.ToInt32()];


            using (Program program = Cl.CreateProgramWithSource(context, 1, new[] { source }, null, out error))
            {
                Assert.AreEqual(error, ErrorCode.Success);
                error = Cl.BuildProgram(program, 1, new[] { device }, "", null, IntPtr.Zero);
                Assert.AreEqual(error, ErrorCode.Success);
                var buildInfo = Cl.GetProgramBuildInfo(program, device, ProgramBuildInfo.Status, out error).CastTo<BuildStatus>();
                Assert.AreEqual(buildInfo, BuildStatus.Success);
                Assert.AreEqual(error, ErrorCode.Success);

                Kernel[] kernels = Cl.CreateKernelsInProgram(program, out error);
                Assert.AreEqual(error, ErrorCode.Success);
                Kernel kernel = kernels[0];

                IMem hDeviceMemXr = Cl.CreateBuffer(context, MemFlags.WriteOnly, (IntPtr)(sizeof(int) * outputSize.ToInt32()), IntPtr.Zero, out error);
                Assert.AreEqual(ErrorCode.Success, error);
                IMem hDeviceMemZr = Cl.CreateBuffer(context, MemFlags.WriteOnly, (IntPtr)(sizeof(int) * outputSize.ToInt32()), IntPtr.Zero, out error);
                Assert.AreEqual(ErrorCode.Success, error);
                IMem hDeviceMemSc = Cl.CreateBuffer(context, MemFlags.WriteOnly, (IntPtr)(sizeof(int) * outputSize.ToInt32()), IntPtr.Zero, out error);
                Assert.AreEqual(ErrorCode.Success, error);


                CommandQueue cmdQueue = Cl.CreateCommandQueue(context, device, (CommandQueueProperties)0, out error);

                int intPtrSize = Marshal.SizeOf(typeof(IntPtr));
                int intSize = Marshal.SizeOf(typeof(int));

                error = Cl.SetKernelArg(kernel, 0, new IntPtr(intPtrSize), hDeviceMemXr);
                Assert.AreEqual(ErrorCode.Success, error);
                error = Cl.SetKernelArg(kernel, 1, new IntPtr(intPtrSize), hDeviceMemZr);
                Assert.AreEqual(ErrorCode.Success, error);
                error = Cl.SetKernelArg(kernel, 2, new IntPtr(intPtrSize), hDeviceMemSc);
                Assert.AreEqual(ErrorCode.Success, error);
                error = Cl.SetKernelArg(kernel, 3, new IntPtr(intSize), new IntPtr(chunkHalfLength));
                Assert.AreEqual(ErrorCode.Success, error);
                error = Cl.SetKernelArg(kernel, 4, new IntPtr(intSize), new IntPtr(worldSeed));
                Assert.AreEqual(ErrorCode.Success, error);
                error = Cl.SetKernelArg(kernel, 5, new IntPtr(intSize), new IntPtr(workItems));
                Assert.AreEqual(ErrorCode.Success, error);
                error = Cl.SetKernelArg(kernel, 6, new IntPtr(intSize), new IntPtr(outputAllocation));
                Assert.AreEqual(ErrorCode.Success, error);

                error = Cl.EnqueueWriteBuffer(cmdQueue, hDeviceMemXr, Bool.True, IntPtr.Zero,
                    new IntPtr(outputSize.ToInt32() * sizeof(float)),
                    xr, 0, null, out Event clevent);
                Assert.AreEqual(ErrorCode.Success, error);

                error = Cl.EnqueueNDRangeKernel(cmdQueue, kernel, 1, null, new IntPtr[] { new IntPtr(workItems) }, null, 0, null, out clevent);

                error = Cl.EnqueueReadBuffer(cmdQueue, hDeviceMemXr, Bool.True, 0, xr.Length, xr, 0, null, out clevent);
                Assert.AreEqual(ErrorCode.Success, error, error.ToString());
    
                Cl.Finish(cmdQueue);
                
            }
        }
        [TestMethod]
        public void AddArrayAddsCorrectly()
        {
            ErrorCode error;

            Device device = (from d in
                           Cl.GetDeviceIDs(
                               (from platform in Cl.GetPlatformIDs(out error)
                                where Cl.GetPlatformInfo(platform, PlatformInfo.Name, out error).ToString() == "AMD Accelerated Parallel Processing" // Use "NVIDIA CUDA" if you don't have amd
                                select platform).First(), DeviceType.Gpu, out error)
                       select d).First();

            Context context = Cl.CreateContext(null, 1, new[] { device }, null, IntPtr.Zero, out error);
            const string correctSource = @"
                // Simple test; c[i] = a[i] + b[i]

                __kernel void add_array(__global float *a, __global float *b, __global float *c)
                {
                    int xid = get_global_id(0);
                    c[xid] = a[xid] + b[xid];
                }
                
                __kernel void sub_array(__global float *a, __global float *b, __global float *c)
                {
                    int xid = get_global_id(0);
                    c[xid] = a[xid] - b[xid];
                }

                ";

            using (Program program = Cl.CreateProgramWithSource(context, 1, new[] { correctSource }, null, out error))
            {
                Assert.AreEqual(error, ErrorCode.Success);
                error = Cl.BuildProgram(program, 1, new[] { device }, string.Empty, null, IntPtr.Zero);
                Assert.AreEqual(ErrorCode.Success, error);
                Assert.AreEqual(Cl.GetProgramBuildInfo(program, device, ProgramBuildInfo.Status, out error).CastTo<BuildStatus>(), BuildStatus.Success);

                Kernel[] kernels = Cl.CreateKernelsInProgram(program, out error);
                Kernel kernel = kernels[0];
                
                const int cnBlockSize = 4;
                const int cnBlocks = 3;
                IntPtr cnDimension = new IntPtr(cnBlocks * cnBlockSize);

                // allocate host  vectors
                float[] A = new float[cnDimension.ToInt32()];
                float[] B = new float[cnDimension.ToInt32()];
                float[] C = new float[cnDimension.ToInt32()];

                // initialize host memory
                Random rand = new Random();
                for (int i = 0; i < A.Length; i++)
                {
                    A[i] = rand.Next() % 256;
                    B[i] = rand.Next() % 256;
                }

                //Cl.IMem hDeviceMemA = Cl.CreateBuffer(_context, Cl.MemFlags.CopyHostPtr | Cl.MemFlags.ReadOnly, (IntPtr)(sizeof(float) * cnDimension.ToInt32()), A, out error);
                //Assert.AreEqual(Cl.ErrorCode.Success, error);
                
                IMem<float> hDeviceMemA = Cl.CreateBuffer(context, MemFlags.CopyHostPtr | MemFlags.ReadOnly, A, out error);
                Assert.AreEqual(ErrorCode.Success, error);

                IMem hDeviceMemB = Cl.CreateBuffer(context, MemFlags.CopyHostPtr | MemFlags.ReadOnly, (IntPtr)(sizeof(float) * cnDimension.ToInt32()), B, out error);
                Assert.AreEqual(ErrorCode.Success, error);
                IMem hDeviceMemC = Cl.CreateBuffer(context, MemFlags.WriteOnly, (IntPtr)(sizeof(float) * cnDimension.ToInt32()), IntPtr.Zero, out error);
                Assert.AreEqual(ErrorCode.Success, error);

                CommandQueue cmdQueue = Cl.CreateCommandQueue(context, device, (CommandQueueProperties)0, out error);

                Event clevent;

                int intPtrSize = 0;
                intPtrSize = Marshal.SizeOf(typeof(IntPtr));

                // setup parameter values
                error = Cl.SetKernelArg(kernel, 0, new IntPtr(intPtrSize), hDeviceMemA);
                Assert.AreEqual(ErrorCode.Success, error);
                error = Cl.SetKernelArg(kernel, 1, new IntPtr(intPtrSize), hDeviceMemB);
                Assert.AreEqual(ErrorCode.Success, error);
                error = Cl.SetKernelArg(kernel, 2, new IntPtr(intPtrSize), hDeviceMemC);
                Assert.AreEqual(ErrorCode.Success, error);

                // write data from host to device
                error = Cl.EnqueueWriteBuffer(cmdQueue, hDeviceMemA, Bool.True, IntPtr.Zero,
                    new IntPtr(cnDimension.ToInt32() * sizeof(float)),
                    A, 0, null, out clevent);
                Assert.AreEqual(ErrorCode.Success, error);
                error = Cl.EnqueueWriteBuffer(cmdQueue, hDeviceMemB, Bool.True, IntPtr.Zero,
                    new IntPtr(cnDimension.ToInt32() * sizeof(float)),
                    B, 0, null, out clevent);
                Assert.AreEqual(ErrorCode.Success, error);

                // execute kernel
                error = Cl.EnqueueNDRangeKernel(cmdQueue, kernel, 1, null, new IntPtr[] { cnDimension }, null, 0, null, out clevent);
                Assert.AreEqual(ErrorCode.Success, error, error.ToString());
                
                // copy results from device back to host
                IntPtr event_handle = IntPtr.Zero;

                error = Cl.EnqueueReadBuffer(cmdQueue, hDeviceMemC, Bool.True, 0, C.Length, C, 0, null, out clevent);
                Assert.AreEqual(ErrorCode.Success, error, error.ToString());

                for (int i = 0; i < A.Length; i++)
                {
                    Assert.AreEqual(A[i] + B[i], C[i]);
                }

                Cl.Finish(cmdQueue);

                Cl.ReleaseMemObject(hDeviceMemA);
                Cl.ReleaseMemObject(hDeviceMemB);
                Cl.ReleaseMemObject(hDeviceMemC);
            }
        }
    }
}