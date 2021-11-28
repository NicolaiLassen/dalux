using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
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

                // make check for neighbours branching

                // gird wrong place
                if (xGridSteps > w || yGridSteps > h || zGridSteps > d)
                {
                    // DO STUFF
                    continue;
                }

                // data form voxel alg
                var voxelData = voxelGrid[zGridSteps + d * (yGridSteps + h * xGridSteps)];

                // point on mesh
                if (voxelData == 1) continue;

                // point outside model mesh
                if (voxelData == 0)
                {
                    voxelPointGrid[xGridSteps, yGridSteps, zGridSteps] = 1;
                }

                // point inside model mesh
                voxelPointGrid[xGridSteps, yGridSteps, zGridSteps] = 2;
            }

            Console.WriteLine(
                $"(Intersection) grid size: {w}x{h}x{d}, time: {stopwatch.ElapsedMilliseconds}ms");
            stopwatch.Stop();

            Voxelizer.SaveVoxelGridAsSTL(voxelPointGrid, unit, "error.stl");
        }
    }
}