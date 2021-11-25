using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Algorithm.Models;
using Algorithm.Services;
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

            typedef struct _Triangle 
            { 
                Vector3 A; 
                Vector3 B; 
                Vector3 C; 
                Vector3 Normal;
            } Triangle; 

            kernel void voxelize
            (
                global read_only Triangle* triangles,
                float unit,
                float hunit,
                int w,
                int h,
                int d,
                global char* dst
            ) 
            {
               int index = get_global_id(0);     
               for (int z = 0; z < d; z++) {
                    for (int y = 0; y < h; y++) {
                        for (int x = 0; x < w; x++) {  
                            dst[z + d * (y + h * x)] = 5;
                            
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
            using var compute = ComputeService.UseComputeService();

            // create program with opencl source
            using var program = new ComputeProgram(compute.Context, VoxelizerSource);

            // compile opencl source
            program.Build(null, null, null, IntPtr.Zero);

            var kernel = program.CreateKernel(VoxelizeFunctionName);

            // mesh vertices in
            using var triangleBuffer = new ComputeBuffer<Triangle>(compute.Context,
                ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.UseHostPointer, mesh.Triangles);
            kernel.SetMemoryArgument(0, triangleBuffer);

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

            using var dstBuffer = new ComputeBuffer<byte>(compute.Context,
                ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, dst);
            kernel.SetMemoryArgument(6, dstBuffer);

            // time
            var stopwatch = new Stopwatch();

            // execute kernel
            compute.Queue.Execute(kernel, null,
                new long[] {mesh.Triangles.Length}, null, null);

            // test alg time voxels
            stopwatch.Start();

            // wait for completion
            compute.Queue.Finish();

            Console.WriteLine($"(Voxelizer) grid size: {w}x{h}x{d}, time: {stopwatch.ElapsedMilliseconds}ms");
            stopwatch.Stop();

            // release data from GPU buffer
            compute.Queue.ReadFromBuffer(dstBuffer, ref dst, true, null);

            // expand back buffer to 3d e.g. byte[,,]
            return (dst, unit, (w, h, d));
        }
    }
}