using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Algorithm.Helpers;
using Algorithm.Models;
using OpenTK.Mathematics;

namespace Algorithm.Lib
{
    public static class Similarity
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
            int resolution = 100,
            bool normalizeCenter = false)
        {
            // voxel denote point on grid with p value of depth, width, height
            // using snaps for each point to detect cluster

            // voxel grid holds pointers to all contained point clusters  
            var voxelMeshCluster = new Dictionary<Vector3, MeshCluster>();

            var unNormalBoundsCenter = new Vector3(mesh.Bounds.Center);

            if (normalizeCenter)
            {
                mesh.NormalizeToCenter();
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var (voxelGrid, unit) = await VoxelHelper.VoxelizeSTLGPU(mesh, resolution);

            stopwatch.Stop();
            // Console.WriteLine(stopwatch.ElapsedMilliseconds);

            stopwatch.Reset();
            stopwatch.Start();

            var hunit = unit * 0.5f;

            var w = voxelGrid.GetLength(0);
            var h = voxelGrid.GetLength(1);
            var d = voxelGrid.GetLength(2);
            var voxelPointGrid = new byte[w, h, d];

            // CPU 
            /////// TODO MOVE TO GPU - threading
            await foreach (var vector3 in pts)
            {
                // Could convert unit float distance to 

                var (x, y, z) = vector3 - new Vector3(hunit, hunit, hunit);
                var xGridSteps = (int) Math.Round(x / unit);
                var yGridSteps = (int) Math.Round(y / unit);
                var zGridSteps = (int) Math.Round(z / unit);

                // copy errors
                voxelPointGrid[xGridSteps, yGridSteps, zGridSteps]
                    = voxelGrid[xGridSteps, yGridSteps, zGridSteps];
            }
            
            Console.WriteLine(voxelPointGrid[0,0,0]);

            // TODO
            // TODO FILL BUFFER IN A Q 
            // Create a command Q and execute for each buffer on the same aomic pointer

            stopwatch.Stop();
            // Console.WriteLine(stopwatch.ElapsedMilliseconds);
        }
    }
}