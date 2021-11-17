using System.Diagnostics;

namespace Algorithm.Models
{
    public class Kernel
    {
        public int Index { get; }

        public uint ThreadX => threadX;

        public uint ThreadY => threadY;

        public uint ThreadZ => threadZ;

        uint threadX, threadY, threadZ;

        // public Kernel(ComputeShader shader, string key)
        // {
        //     Index = shader.FindKernel(key);
        //     if (Index < 0)
        //     {
        //         Debug.WriteLine("Can't find kernel");
        //         return;
        //     }
        //
        //     shader.GetKernelThreadGroupSizes(Index, out threadX, out threadY, out threadZ);
        // }
    }
}