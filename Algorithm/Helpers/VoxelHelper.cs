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
                int d
            ) 
            {
                
                printf(""Y: %d\n"", verts[10].X);
            }
        ";

        /// <summary>
        /// Convert triangle mesh into voxel list representation
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        public static async Task<byte[,,]> VoxelizeSTLGPU(STL mesh, int resolution = 100)
        {
            // TODO SET GPU FROM ENV NOT JUST 1
            var platform = ComputePlatform.Platforms[1];

            // create context with all gpu devices
            var context = new ComputeContext(ComputeDeviceTypes.Gpu,
                new ComputeContextPropertyList(platform), null, IntPtr.Zero);

            // create a command queue with gpu
            var queue = new ComputeCommandQueue(context, context.Devices[0], ComputeCommandQueueFlags.None);

            // create program with opencl source
            var program = new ComputeProgram(context, VoxelizerSource);

            // compile opencl source
            program.Build(null, null, null, IntPtr.Zero);

            // load chosen kernel from program
            var kernel = program.CreateKernel(VoxelizeFunctionName);

            var vertBuffer = new ComputeBuffer<Vector3>(context,
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

            // grid max size for allocation
            var w = (int) MathHelper.Ceiling(fx / unit);
            var h = (int) MathHelper.Ceiling(fy / unit);
            var d = (int) MathHelper.Ceiling(fz / unit);

            // set bounds
            kernel.SetValueArgument(1, unit);
            kernel.SetValueArgument(2, hunit);

            kernel.SetValueArgument(3, w);
            kernel.SetValueArgument(4, h);
            kernel.SetValueArgument(5, d);

            // set out buffer for matrix

            // execute kernel
            queue.ExecuteTask(kernel, null);

            // wait for completion
            queue.Finish();

            // release data from GPU buffer
            // queue.ReadFromBuffer(dstBuffer, ref dst, true, null);

            var voxelGrid = new byte[2, 2, 2];

            // clean the context
            vertBuffer.Dispose();
            kernel.Dispose();
            program.Dispose();

            // do static helper to keep the 
            program.Dispose();
            context.Dispose();

            return voxelGrid;
        }

        /// <summary>
        /// Simple cube STL Shape
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static List<Facet> CreateSTLVoxel(Vector3 pos, float size, ushort attribute = ushort.MinValue)
        {
            // CPU

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