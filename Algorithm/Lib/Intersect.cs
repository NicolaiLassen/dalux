using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Algorithm.Helpers;
using Algorithm.Models;
using OpenTK.Mathematics;

namespace Algorithm.Lib
{
    public static class Intersect
    {
        /// <summary>
        /// Find a distance map from point in space to mesh
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="pts"></param>
        /// <param name="resolution"></param>
        /// <returns>Distance map</returns>
        public static async Task MeshPointCloudIntersectionAsync(
            STL mesh,
            IAsyncEnumerable<Vector3> pts,
            int resolution = 100
        )
        {
            // generate voxel grid
            var (voxelGrid, unit, (w, h, d))
                = await Voxelizer.STLGPU(mesh, resolution);

            // init arguments for copy of errors
            var hunit = unit * 0.5f;
            var voxelPointGrid = new byte[w, h, d];

            // runtime test
            var stopwatch = new Stopwatch();

            stopwatch.Start();
            // CPU 
            /////// TODO: MOVE TO GPU - threading, buffers
            await foreach (var vector3 in pts)
            {
                var (x, y, z) = vector3 - new Vector3(hunit, hunit, hunit);
                var xGridSteps = (int) Math.Round(x / unit);
                var yGridSteps = (int) Math.Round(y / unit);
                var zGridSteps = (int) Math.Round(z / unit);

                // copy errors from voxel rep to cloud rep 
                voxelPointGrid[xGridSteps, yGridSteps, zGridSteps]
                    // read from flatten buffer as 3d byte[,,]
                    = voxelGrid[zGridSteps + d * (yGridSteps + h * xGridSteps)];
            }

            Console.WriteLine(
                $"(Intersection) grid size: {w}x{h}x{d}, time: {stopwatch.ElapsedMilliseconds}ms");
            stopwatch.Stop();
        }
    }
}