using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Algorithm.Models;
using OpenTK.Mathematics;

namespace Algorithm.Lib
{
    public static class IntersectType
    {
        public const byte PointOutsideOfMesh = 0;
        public const byte PointInsideOfMesh = 1;
    }

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

            Voxelizer.SaveVoxelGridAsSTL(voxelGrid, unit, w, h, d);

            // init arguments for copy of errors
            var hunit = unit * 0.5f;
            var selectedForSearch = new Dictionary<Vector3, byte>();

            // runtime test
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            // CPU 
            /////// TODO: MOVE TO GPU - threading, buffers
            await foreach (var vector3 in pts)
            {
                var (x, y, z) = vector3 - new Vector3(hunit, hunit, hunit);
                var xGridIndex = (int) Math.Round(x / unit);
                var yGridIndex = (int) Math.Round(y / unit);
                var zGridIndex = (int) Math.Round(z / unit);

                var key = new Vector3(xGridIndex, yGridIndex, zGridIndex);
                var index = zGridIndex + d * (yGridIndex + h * xGridIndex);

                if (selectedForSearch.ContainsKey(key))
                {
                    continue;
                }

                // grid wrong place
                if (xGridIndex > w || yGridIndex > h || zGridIndex > d)
                {
                    // DO STUFF
                    continue;
                }

                // data form voxel alg
                var voxelData = voxelGrid[index];

                // point on mesh
                if (voxelData == VoxelGridType.OnMesh) continue;

                // point outside model mesh
                if (voxelData == VoxelGridType.OutOfMesh)
                {
                    selectedForSearch.Add(key, IntersectType.PointOutsideOfMesh);
                    continue;
                }

                // point inside model mesh
                // VoxelGrid.OutOfMesh
                selectedForSearch.Add(key, IntersectType.PointInsideOfMesh);
            }

            // make check for neighbours branching
            //   |
            // - E -
            //   |
            var voxelPointGrid = new byte[w, h, d];

            Console.WriteLine(selectedForSearch.Count);
            
            // foreach (var (pos, value) in selectedForSearch)
            // {
            //     var index = pos.Z + d * (pos.Y + h * pos.X);
            // }

            Console.WriteLine(
                $"(Intersection) grid size: {w}x{h}x{d}, time: {stopwatch.ElapsedMilliseconds}ms");
            stopwatch.Stop();

            Voxelizer.SaveVoxelGridAsSTL(voxelPointGrid, unit, "error.stl");
        }
    }
}