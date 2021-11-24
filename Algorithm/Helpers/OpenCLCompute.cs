using System;
using Cloo;

namespace Algorithm.Helpers
{
    public static class OpenCLCompute
    {
        public static ComputePlatform Platform { get; private set; }
        public static ComputeContext Context { get; private set; }
        public static ComputeCommandQueue Queue { get; private set; }

        public static void SetContext()
        {
            // TODO SET GPU FROM ENV NOT JUST 1
            Platform = ComputePlatform.Platforms[1];

            // create context with all gpu devices
            Context = new ComputeContext(ComputeDeviceTypes.Gpu,
                new ComputeContextPropertyList(Platform), null, IntPtr.Zero);

            // create a command queue with gpu
            Queue = new ComputeCommandQueue(Context, Context.Devices[0], ComputeCommandQueueFlags.None);
        }
    }
}