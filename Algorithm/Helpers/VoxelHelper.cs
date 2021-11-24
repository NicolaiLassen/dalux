using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Algorithm.Lib;
using Algorithm.Models;
using Cloo;
using OpenTK.Mathematics;

namespace Algorithm.Helpers
{
    public struct Point3D
    {
        public float X;
        public float Y;
        public float Z;
    }

    public static class VoxelHelper
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
        /// Expand flatten buffer 2 3d
        /// </summary>
        /// <param name="value"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="d"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private static T[,,] Expand3d<T>(T[] value, int w, int h, int d)
        {
            // create 3d grid [,,]
            var result = new T[w, h, d];
            for (var i = 0; i < value.Length; ++i)
            {
                // unpack
                var x = i / (d * h);
                var y = i / d % h;
                var z = i % d;
                result[x, y, z] = value[i];
            }

            return result;
        }

        /// <summary>
        /// Convert triangle mesh into voxel grid representation
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        public static async Task<(byte[,,], float)> VoxelizeSTLGPU(STL mesh, int resolution = 100)
        {
            // TODO SET GPU FROM ENV NOT JUST 1
            var platform = ComputePlatform.Platforms[1];

            // scoped

            // create context with all gpu devices
            using var context = new ComputeContext(ComputeDeviceTypes.Gpu,
                new ComputeContextPropertyList(platform), null, IntPtr.Zero);

            // create a command queue with gpu
            using var queue = new ComputeCommandQueue(context, context.Devices[0], ComputeCommandQueueFlags.None);

            // create program with opencl source
            using var program = new ComputeProgram(context, VoxelizerSource);

            // compile opencl source
            program.Build(null, null, null, IntPtr.Zero);

            // load chosen kernel from program
            using var kernel = program.CreateKernel(VoxelizeFunctionName);

            // mesh vertices in
            using var vertBuffer = new ComputeBuffer<Vector3>(context,
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

            Console.WriteLine(w);
            Console.WriteLine(h);
            Console.WriteLine(d);

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

            using var dstBuffer = new ComputeBuffer<byte>(context,
                ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, dst);
            kernel.SetMemoryArgument(6, dstBuffer);

            // execute kernel
            queue.ExecuteTask(kernel, null);

            // wait for completion
            queue.Finish();

            // release data from GPU buffer
            queue.ReadFromBuffer(dstBuffer, ref dst, true, null);

            // expand back to byte[,,]
            return (Expand3d(dst, w, h, d), unit);
        }

        /// <summary>
        /// Simple cube STL Shape
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static List<Facet> CreateSTLVoxel(Vector3 pos, float size, ushort attribute = ushort.MinValue)
        {
            // CPU bound
            /////// TODO

            // -1 and 1 center of point 
            // half the size to correct for two sided scaling [-1...1]
            var cubeSize = size / 2;

            // vertices 
            var v1 = new Vector3(1, 1, 1) * cubeSize + pos;
            var v2 = new Vector3(-1, 1, 1) * cubeSize + pos;
            var v3 = new Vector3(-1, -1, 1) * cubeSize + pos;
            var v4 = new Vector3(1, -1, 1) * cubeSize + pos;
            var v5 = new Vector3(1, -1, -1) * cubeSize + pos;
            var v6 = new Vector3(-1, -1, -1) * cubeSize + pos;
            var v7 = new Vector3(-1, 1, -1) * cubeSize + pos;
            var v8 = new Vector3(1, 1, -1) * cubeSize + pos;

            // normals 
            var n1 = new Vector3(0, 0, 1);
            var n2 = new Vector3(0, -1, 0);
            var n3 = new Vector3(-1, 0, 0);
            var n4 = new Vector3(0, 0, -1);
            var n5 = new Vector3(1, 0, 0);
            var n6 = new Vector3(0, 1, 0);

            // cube
            return new List<Facet>
            {
                new Facet
                {
                    Normal = n1,
                    Vertices = new List<Vector3>
                    {
                        v1,
                        v2,
                        v3
                    },
                    AttributeByteCount = attribute
                },
                new Facet
                {
                    Normal = n1,
                    Vertices = new List<Vector3>
                    {
                        v1,
                        v3,
                        v4
                    }
                },
                new Facet
                {
                    Normal = n2,
                    Vertices = new List<Vector3>
                    {
                        v5,
                        v4,
                        v3
                    },
                    AttributeByteCount = attribute
                },
                new Facet
                {
                    Normal = n2,
                    Vertices = new List<Vector3>
                    {
                        v5,
                        v3,
                        v6
                    },
                    AttributeByteCount = attribute
                },
                new Facet
                {
                    Normal = n3,
                    Vertices = new List<Vector3>
                    {
                        v6,
                        v3,
                        v2
                    },
                    AttributeByteCount = attribute
                },
                new Facet
                {
                    Normal = n3,
                    Vertices = new List<Vector3>
                    {
                        v6,
                        v2,
                        v7
                    },
                    AttributeByteCount = attribute
                },
                new Facet
                {
                    Normal = n4,
                    Vertices = new List<Vector3>
                    {
                        v7,
                        v8,
                        v5
                    },
                    AttributeByteCount = attribute
                },
                new Facet
                {
                    Normal = n4,
                    Vertices = new List<Vector3>
                    {
                        v7,
                        v5,
                        v6
                    },
                    AttributeByteCount = attribute
                },
                new Facet
                {
                    Normal = n5,
                    Vertices = new List<Vector3>
                    {
                        v8,
                        v1,
                        v4
                    },
                    AttributeByteCount = attribute
                },
                new Facet
                {
                    Normal = n5,
                    Vertices = new List<Vector3>
                    {
                        v8,
                        v4,
                        v5
                    },
                    AttributeByteCount = attribute
                },
                new Facet
                {
                    Normal = n6,
                    Vertices = new List<Vector3>
                    {
                        v7,
                        v2,
                        v1
                    },
                    AttributeByteCount = attribute
                },
                new Facet
                {
                    Normal = n6,
                    Vertices = new List<Vector3>
                    {
                        v7,
                        v1,
                        v8
                    },
                    AttributeByteCount = attribute
                }
            };
        }
    }
}