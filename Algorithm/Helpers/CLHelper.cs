using System;
using System.Collections.Generic;
using Cloo;

namespace Algorithm.Helpers
{
    public static class CLHelper
    {
        public static ComputeContext GetComputeContext()
        {
            // setup GPU
            var platform = ComputePlatform.Platforms[1];

            var devices = new List<ComputeDevice> {platform.Devices[0]};
            
            var properties = new ComputeContextPropertyList(platform);

            var context = new ComputeContext(devices, properties, null, IntPtr.Zero);

            return context;
        }
    }
}