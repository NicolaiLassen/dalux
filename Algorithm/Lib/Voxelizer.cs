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

            typedef struct _Bounds 
            { 
                Vector3 Center; 
                Vector3 Extents;
            } Bounds; 

            typedef struct _Triangle 
            { 
                Vector3 A; 
                Vector3 B; 
                Vector3 C; 
                Vector3 Normal;
                Bounds Bounds;
            } Triangle; 
            
            typedef struct _Plane
            {
                float3 Normal;
                float Distance;
            } Plane;

            typedef struct _AABB
            {
                float3 Max;
                float3 Min;
                float3 Center;             
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

            bool intersects_tri_aabb(Triangle tri, AABB aabb)
            {
                float p0, p1, p2, r;

                float3 center = aabb.Center, extents = aabb.Max - center;
                float3 va = (float3)(tri.A.X, tri.A.Y, tri.A.X);
                float3 vb = (float3)(tri.B.X, tri.B.Y, tri.B.X);
                float3 vc = (float3)(tri.C.X, tri.C.Y, tri.C.X);

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

            kernel void voxelize
            (
                global read_only Triangle* triangles,
                float unit,
                int w,
                int h,
                int d,
                Vector3 s,
                Vector3 e,
                global char* dst
            ) 
            {
               int index = get_global_id(0);     
               Triangle tri = triangles[index];

               float3 bCenter = (float3) 
               (tri.Bounds.Center.X, tri.Bounds.Center.Y, tri.Bounds.Center.Z);

               float3 bExtents;
               (tri.Bounds.Extents.X, tri.Bounds.Extents.Y, tri.Bounds.Extents.Z);
 
               float3 start = (float3)
               (s.X, s.Y, s.Z);

               float3 bMin = bCenter - bExtents;
               float3 bMax = bCenter + bExtents;
       
               float3 fmin = bMin - start; 
               float3 fmax = bMax - start;     
            
               int iMinX = round(fmin.x / unit);
               int iMinY = round(fmin.y / unit);
               int iMinZ = round(fmin.z / unit);

               int iMaxX = round(fmax.x / unit);
               int iMaxY = round(fmax.y / unit);
               int iMaxZ = round(fmax.z / unit);
               
               iMinX = clamp(iMinX, 0, w - 1);
               iMinY = clamp(iMinY, 0, h - 1);
               iMinZ = clamp(iMinZ, 0, d - 1);

               iMaxX = clamp(iMaxX, 0, w - 1);
               iMaxY = clamp(iMaxY, 0, h - 1);
               iMaxZ = clamp(iMaxZ, 0, d - 1);
                
               for (int z = iMinZ; z < iMaxZ; z++) {
                    for (int y = iMinY; y < iMaxY; y++) {
                        for (int x = iMinZ; x < iMaxY; x++) {  

                            float3 vUnit = (float3)(unit, unit, unit);
                            float3 vHUnit = (float3)(unit * 0.5, unit * 0.5, unit * 0.5);
                            float3 center = (float3)(x, y, z) * start + vUnit;
                            
                            AABB aabb;
                            aabb.Center = center;
                            aabb.Min = center - vHUnit;
                            aabb.Max = center + vHUnit;
                            if(intersects_tri_aabb(tri, aabb))
                            {
                                dst[z + d * (y + h * x)] = 1;
                            }
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

            // set bounds
            kernel.SetValueArgument(2, w);
            kernel.SetValueArgument(3, h);
            kernel.SetValueArgument(4, d);
            kernel.SetValueArgument(5, s);
            kernel.SetValueArgument(6, e);

            // set out buffer for byte matrix
            // correct
            // 0: out of mesh
            // 1: on mesh
            // 2: inside mesh

            // flatten buffer representation 
            var flatten = w * h * d;
            var dst = new byte[flatten];

            using var dstBuffer = new ComputeBuffer<byte>(compute.Context,
                ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, dst);
            kernel.SetMemoryArgument(7, dstBuffer);

            // time
            var stopwatch = new Stopwatch();

            // execute kernel
            compute.Queue.ExecuteTask(kernel, null);
            // compute.Queue.Execute(kernel, null,
            //     new long[] {mesh.Triangles.Length}, null, null);

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