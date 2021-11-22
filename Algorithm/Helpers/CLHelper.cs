using System;
using OpenTK.Compute.OpenCL;

namespace Algorithm.Helpers
{
    public class ClHelperContextResponse
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        public CLContext CTX { get; set; }

        public static ClHelperContextResponse FromContext(CLContext ctx)
        {
            return new ClHelperContextResponse
            {
                Success = true,
                CTX = ctx
            };
        }

        public static ClHelperContextResponse FromError(string message)
        {
            return new ClHelperContextResponse
            {
                Success = false,
                Message = message
            };
        }
    }

    public static class ClHelper
    {
        public static ClHelperContextResponse CreateClContextGpu()
        {
            // Init openCL context for GPU Compute

            var platformsResultCode = CL.GetPlatformIds(out var allPlatforms);

            if (platformsResultCode != CLResultCode.Success)
            {
                return ClHelperContextResponse.FromError("No platforms found");
            }

            var defaultPlatform = allPlatforms[0];

            var devicesResultCode = CL.GetDeviceIds(defaultPlatform, DeviceType.Gpu, out var allGpuDevices);

            if (devicesResultCode != CLResultCode.Success)
            {
                return ClHelperContextResponse.FromError("No devices found");
            }

            var defaultGpuDevice = allGpuDevices[0];

            var callback = new IntPtr();

            var ctx = CL.CreateContext(
                null,
                1,
                new[] {defaultGpuDevice},
                callback,
                IntPtr.Zero,
                out var contextResultCode);

            if (contextResultCode != CLResultCode.Success)
            {
                return ClHelperContextResponse.FromError("Context could not be created");
            }

            return ClHelperContextResponse.FromContext(ctx);
        }
    }
}