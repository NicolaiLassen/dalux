using System;
using System.Collections.Generic;
using System.Linq;
using Cloo;

namespace Algorithm.Services
{
    public class ComputeService : IDisposable
    {
        public ComputePlatform Platform { get; private set; }
        public ComputeContext Context { get; private set; }
        public ComputeCommandQueue Queue { get; private set; }

        public static int ComputePlatformIndex { get; private set; } = 1;

        public static void SetComputePlatformIndex(int index)
        {
            ComputePlatformIndex = index;
        }

        public static IEnumerable<string> GetComputePlatformNames =>
            ComputePlatform.Platforms.Select(platform => platform.Name);

        public static ComputeService UseComputeService()
        {
            // TODO SET GPU FROM ENV NOT JUST 1
            var platform = ComputePlatform.Platforms[ComputePlatformIndex];

            // create context with all gpu devices
            var context = new ComputeContext(ComputeDeviceTypes.Gpu,
                new ComputeContextPropertyList(platform), null, IntPtr.Zero);

            // create a command queue with gpu
            var queue = new ComputeCommandQueue(context, context.Devices[0], ComputeCommandQueueFlags.None);

            return new ComputeService
            {
                Platform = platform,
                Context = context,
                Queue = queue
            };
        }

        public void Dispose()
        {
            Context.Dispose();
            Queue.Dispose();
        }
    }
}