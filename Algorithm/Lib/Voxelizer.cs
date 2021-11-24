using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Algorithm.Helpers;
using Cloo;
using OpenTK.Mathematics;

namespace Algorithm.Lib
{
    public class Voxelizer
    {
        /// <summary>
        /// OpenCL kernel function name
        /// </summary>
        private const string VoxelizeFunctionName = "voxelize";

        /// <summary>
        /// OpenCl kernel source
        /// Voxelizer
        /// </summary>
        private const string VoxelizerSource =
            @"
            typedef struct _Vector3 
            { 
                float X; 
                float Y; 
                float Z; 
            } Vector3; 

            kernel void voxelize
            (
                global read_only Vector3* verts,
                float unit,
                float hunit,
                int w,
                int h,
                int d,
                global char* dst
            ) 
            {

                for (int z = 0; z < d; z++) {
                    for (int y = 0; y < h; y++) {
                        for (int x = 0; x < w; x++) {
                            
                            dst[z + d * (y + h * x)] = 1;
                        }
                    }                    
                }

            }
        ";

        /// <summary>
        /// Convert triangle mesh into voxel grid representation
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        public static async Task<(byte[], float, (int, int, int))> STLGPU(STL mesh, int resolution = 100)
        {
            // create program with opencl source
            using var program = new ComputeProgram(OpenCLCompute.Context, VoxelizerSource);

            // compile opencl source
            program.Build(null, null, null, IntPtr.Zero);

            // load chosen kernel from program
            using var kernel = program.CreateKernel(VoxelizeFunctionName);

            // mesh vertices in
            using var vertBuffer = new ComputeBuffer<Vector3>(OpenCLCompute.Context,
                ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.UseHostPointer, mesh.Vertices);
            kernel.SetMemoryArgument(0, vertBuffer);

            // parameters for the voxelizer alg
            var bounds = mesh.Bounds;

            var maxLength =
                MathHelper.Max(bounds.size.X, MathHelper.Max(bounds.size.Y, bounds.size.Z));
            var unit = maxLength / resolution;
            var hunit = unit * 0.5f;

            var s = bounds.min - new Vector3(hunit, hunit, hunit);
            var e = bounds.max + new Vector3(hunit, hunit, hunit);
            var (fx, fy, fz) = e - s;

            // ceiling of grid max size for allocation
            var w = (int) MathHelper.Ceiling(fx / unit);
            var h = (int) MathHelper.Ceiling(fy / unit);
            var d = (int) MathHelper.Ceiling(fz / unit);

            // set units
            kernel.SetValueArgument(1, unit);
            kernel.SetValueArgument(2, hunit);

            // set bounds
            kernel.SetValueArgument(3, w);
            kernel.SetValueArgument(4, h);
            kernel.SetValueArgument(5, d);

            // set out buffer for byte matrix
            // correct
            // 0: on mesh
            // errors
            // 1: out of mesh
            // 2: inside mesh

            // flatten buffer representation 
            var flatten = w * h * d;
            var dst = new byte[flatten];

            using var dstBuffer = new ComputeBuffer<byte>(OpenCLCompute.Context,
                ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, dst);
            kernel.SetMemoryArgument(6, dstBuffer);

            // time
            var stopwatch = new Stopwatch();

            // execute kernel
            OpenCLCompute.Queue.ExecuteTask(kernel, null);

            // test alg time voxels
            stopwatch.Start();

            // wait for completion
            OpenCLCompute.Queue.Finish();

            stopwatch.Stop();
            Console.WriteLine($"(Voxelizer) grid size: {w}x{h}x{d}, time: {stopwatch.ElapsedMilliseconds}ms");

            // release data from GPU buffer
            OpenCLCompute.Queue.ReadFromBuffer(dstBuffer, ref dst, true, null);

            // expand back buffer to 3d e.g. byte[,,]
            return (dst, unit, (w, h, d));
        }
    }
}