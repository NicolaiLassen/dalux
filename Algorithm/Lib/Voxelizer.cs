using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Algorithm.Helpers;
using Algorithm.Models;
using Algorithm.Services;
using Cloo;
using OpenTK.Mathematics;

namespace Algorithm.Lib
{
    public static class VoxelGridType
    {
        public const byte OutOfMesh = 0;
        public const byte OnMesh = 1;
        public const byte InsideMesh = 2;
    }

    public class Voxelizer
    {
        /// <summary>
        /// OpenCL kernel function name
        /// </summary>
        private const string VoxelizeSurfaceFunctionName = "voxelize_surface";

        private const string VoxelizeVolumeFunctionName = "voxelize_volume";

        /// <summary>
        /// OpenCl kernel source
        /// Voxelizer
        /// </summary>
        private const string VoxelizerSource =
            @"
            typedef struct _Vector3 
            { 
                float X, Y, Z;
            } Vector3; 

            typedef struct _Bounds 
            { 
                Vector3 Center, Extents; 
            } Bounds; 

            typedef struct _Triangle 
            { 
                Vector3 A, B, C, Normal;
            } Triangle; 
            
            typedef struct _Plane
            {
                float3 Normal;
                float Distance;
            } Plane;

            typedef struct _AABB
            {
                float3 Max, Min, Center;        
            } AABB;

            bool intersects_plane_aabb(Plane pl, AABB aabb)
            {
                float3 center = aabb.Center;
                float3 extents = aabb.Max - center;

                float r = extents.x * fabs(pl.Normal.x) + extents.y * fabs(pl.Normal.y) + extents.z * fabs(pl.Normal.z);
                float s = dot(pl.Normal, center) - pl.Distance;

                return fabs(s) <= r;
            }

            bool intersects_tri_aabb_onto_axis(float3 v0, float3 v1, float3 v2, float3 extents, float3 axis)
            {
                float p0 = dot(v0, axis);
                float p1 = dot(v1, axis);
                float p2 = dot(v2, axis);

                float r = extents.x * fabs(axis.x) + extents.y * fabs(axis.y) + extents.z * fabs(axis.z);
                float minP = min(p0, min(p1, p2));
                float maxP = max(p0, max(p1, p2));
                return !((maxP < -r) || (r < minP));
            }

            bool intersects_tri_aabb(float3 va, float3 vb, float3 vc, AABB aabb)
            {
                float p0, p1, p2, r;
                float3 center = aabb.Center, extents = aabb.Max - center;

                float3 v0 = va - center,
                v1 = vb - center,
                v2 = vc - center;

                float3 f0 = v1 - v0,
                f1 = v2 - v1,
                f2 = v0 - v2;
                
                float3 a00 = (float3)(0, -f0.z, f0.y), // cross product of X and f0
                a01 = (float3)(0, -f1.z, f1.y), // X and f1
                a02 = (float3)(0, -f2.z, f2.y), // X and f2
                a10 = (float3)(f0.z, 0, -f0.x), // Y and f0
                a11 = (float3)(f1.z, 0, -f1.x), // Y and f1
                a12 = (float3)(f2.z, 0, -f2.x), // Y and f2
                a20 = (float3)(-f0.y, f0.x, 0), // Z and f0
                a21 = (float3)(-f1.y, f1.x, 0), // Z and f1
                a22 = (float3)(-f2.y, f2.x, 0); // Z and f2

                if (
                    !intersects_tri_aabb_onto_axis(v0, v1, v2, extents, a00) ||
                    !intersects_tri_aabb_onto_axis(v0, v1, v2, extents, a01) ||
                    !intersects_tri_aabb_onto_axis(v0, v1, v2, extents, a02) ||
                    !intersects_tri_aabb_onto_axis(v0, v1, v2, extents, a10) ||
                    !intersects_tri_aabb_onto_axis(v0, v1, v2, extents, a11) ||
                    !intersects_tri_aabb_onto_axis(v0, v1, v2, extents, a12) ||
                    !intersects_tri_aabb_onto_axis(v0, v1, v2, extents, a20) ||
                    !intersects_tri_aabb_onto_axis(v0, v1, v2, extents, a21) ||
                    !intersects_tri_aabb_onto_axis(v0, v1, v2, extents, a22)
                )
                {
                    return false;
                }

                if (max(v0.x, max(v1.x, v2.x)) < -extents.x || min(v0.x, min(v1.x, v2.x)) > extents.x)
                {
                    return false;
                }

                if (max(v0.y, max(v1.y, v2.y)) < -extents.y || min(v0.y, min(v1.y, v2.y)) > extents.y)
                {
                    return false;
                }

                if (max(v0.z, max(v1.z, v2.z)) < -extents.z || min(v0.z, min(v1.z, v2.z)) > extents.z)
                {
                    return false;
                }

                Plane pl;
                pl.Normal = normalize(cross(f1, f0));
                pl.Distance = dot(pl.Normal, va);
                return intersects_plane_aabb(pl, aabb);
            }

            kernel void voxelize_surface
            (
                global read_only Triangle* triangles,
                float unit,
                int w,
                int h,
                int d,
                Vector3 s,
                Vector3 e,
                global char* voxelDST,
                global bool* frontDST
            ) 
            {
               int index = get_global_id(0);     
               Triangle tri = triangles[index];

               float3 start = (float3)(s.X, s.Y, s.Z);

               float3 va = (float3)(tri.A.X, tri.A.Y, tri.A.Z);
               float3 vb = (float3)(tri.B.X, tri.B.Y, tri.B.Z);
               float3 vc = (float3)(tri.C.X, tri.C.Y, tri.C.Z);

               float3 normal = cross((vb - va), (vc - vb));
               bool isf = dot(normal, (float3)(0, 0, 1)) < 0;

               float3 tbmin = min(min(va, vb), vc);
               float3 tbmax = max(max(va, vb), vc);

               float3 fmin = tbmin - start; 
               float3 fmax = tbmax - start; 

               int iMinX = round(floor(fmin.x) / unit);
               int iMinY = round(floor(fmin.y) / unit);
               int iMinZ = round(floor(fmin.z) / unit);

               int iMaxX = round(ceil(fmax.x) / unit);
               int iMaxY = round(ceil(fmax.y) / unit);
               int iMaxZ = round(ceil(fmax.z) / unit);

               iMinX = clamp(iMinX, 0, w - 1);
               iMinY = clamp(iMinY, 0, h - 1);
               iMinZ = clamp(iMinZ, 0, d - 1);

               iMaxX = clamp(iMaxX + 1, 0, w - 1);
               iMaxY = clamp(iMaxY + 1, 0, h - 1);
               iMaxZ = clamp(iMaxZ + 1, 0, d - 1);

               float3 vUnit = (float3)(unit, unit, unit);
               float3 vHUnit = (float3)(unit * 0.5, unit * 0.5, unit * 0.5);
         
               for (int x = iMinX; x < iMaxX; x++) {  
                    for (int y = iMinY; y < iMaxY; y++) {
                        for (int z = iMinZ; z < iMaxZ; z++) {

                            float3 center = (float3)(x, y, z)  * vUnit + start;
                            
                            AABB aabb;
                            aabb.Center = center;
                            aabb.Min = center - vHUnit;
                            aabb.Max = center + vHUnit;

                            if(intersects_tri_aabb(va, vb, vc, aabb))
                            {    
                                int index = z + d * (y + h * x);
                                // on mesh border
                                voxelDST[index] = 1;
                                // is front or back
                                frontDST[index] = isf;
                            }
                        }
                    }                    
                }
            }
        ";

        private static T[,,] Expand<T>(IReadOnlyList<T> value, int length1, int length2, int length3)
        {
            var result = new T[length1, length2, length3];

            for (var i = 0; i < value.Count; ++i)
            {
                var r = i / (length3 * length2);
                var c = i / length3 % length2;
                var h = i % length3;

                result[r, c, h] = value[i];
            }

            return result;
        }

        public static void SaveVoxelGridAsSTL(byte[] dst, float unit, int w, int h, int d)
        {
            var t = Expand(dst, w, h, d);
            SaveVoxelGridAsSTL(t, unit, "voxels.stl");
        }

        public static void SaveVoxelGridAsSTL(byte[,,] t, float unit, string name)
        {
            var hunit = unit * 0.5f;
            var stlFacets = new List<Facet>();
            for (var x = 0; x < t.GetLength(0); x++)
            {
                for (var y = 0; y < t.GetLength(1); y++)
                {
                    for (var z = 0; z < t.GetLength(2); z++)
                    {
                        if (t[x, y, z] == 0) continue;
                        var v = new Vector3(x * unit, y * unit, z * unit);
                        var boxFacets = STLShapes.Cube(v, hunit);
                        stlFacets.AddRange(boxFacets);
                    }
                }
            }

            new STL(stlFacets).SaveAsBinary(name);
        }

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

            // SURFACE 
            
            var surfaceKernel = program.CreateKernel(VoxelizeSurfaceFunctionName);

            // mesh vertices in
            using var triangleBuffer = new ComputeBuffer<Triangle>(compute.Context,
                ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.UseHostPointer, mesh.Triangles);
            surfaceKernel.SetMemoryArgument(0, triangleBuffer);

            // parameters for the voxelizer alg
            var bounds = mesh.Bounds;

            var maxLength =
                MathHelper.Max(bounds.size.X, MathHelper.Max(bounds.size.Y, bounds.size.Z));
            var unit = maxLength / resolution;
            var hunit = unit * 0.5f;

            var s = bounds.min - new Vector3(hunit, hunit, hunit);
            var e = bounds.max + new Vector3(hunit, hunit, hunit);
            var (f, f1, z1) = e - s;

            // ceiling of grid max size for allocation
            var w = (int) MathHelper.Ceiling(f / unit) + 1;
            var h = (int) MathHelper.Ceiling(f1 / unit) + 1;
            var d = (int) MathHelper.Ceiling(z1 / unit) + 1;

            // set units
            surfaceKernel.SetValueArgument(1, unit);

            // set bounds
            surfaceKernel.SetValueArgument(2, w);
            surfaceKernel.SetValueArgument(3, h);
            surfaceKernel.SetValueArgument(4, d);
            surfaceKernel.SetValueArgument(5, s);
            surfaceKernel.SetValueArgument(6, e);

            // set out buffer for byte matrix
            // flatten buffer representation 
            var voxelDst = new byte[w * h * d];

            using var voxelDstBuffer = new ComputeBuffer<byte>(compute.Context,
                ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, voxelDst);
            surfaceKernel.SetMemoryArgument(7, voxelDstBuffer);

            var frontDst = new bool[w * h * d];

            using var frontDstBuffer = new ComputeBuffer<bool>(compute.Context,
                ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, frontDst);
            surfaceKernel.SetMemoryArgument(8, frontDstBuffer);

            // time
            var stopwatch = new Stopwatch();

            // execute kernel
            compute.Queue.Execute(surfaceKernel, null,
                new long[] {mesh.Triangles.Length}, null, null);

            // test alg time voxels
            stopwatch.Start();

            // wait for completion
            compute.Queue.Finish();
            
            // release data from GPU buffer
            compute.Queue.ReadFromBuffer(voxelDstBuffer, ref voxelDst, true, null);
            compute.Queue.ReadFromBuffer(frontDstBuffer, ref frontDst, true, null);
            
            // VOLUME
            
            // var volumeKernel = program.CreateKernel(VoxelizeVolumeFunctionName);
            //
            Console.WriteLine($"(Voxelizer) grid size: {w}x{h}x{d}, time: {stopwatch.ElapsedMilliseconds}ms");
            stopwatch.Stop();

            // expand back buffer to 3d e.g. byte[,,]
            return (voxelDst, unit, (w, h, d));
        }
    }
}