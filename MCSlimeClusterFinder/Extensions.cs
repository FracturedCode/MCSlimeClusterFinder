using OpenCL.NetCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCSlimeClusterFinder
{
    public static class Extensions
    {
        public static InfoBuffer GetInfo(this Device device, DeviceInfo param)
            => Cl.GetDeviceInfo(device, param, out ErrorCode error);
        public static InfoBuffer GetInfo(this Platform platform, PlatformInfo param)
            => Cl.GetPlatformInfo(platform, param, out ErrorCode error);
        public static string DeviceInfoLine(this Device device)
            => $"{device.GetInfo(DeviceInfo.Name)} {device.GetInfo(DeviceInfo.Platform).CastTo<Platform>().GetInfo(PlatformInfo.Name)}";
    }
}
