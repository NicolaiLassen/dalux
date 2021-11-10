using System.Collections.Generic;
using System.Threading.Tasks;
using Algorithm.Helpers;
using Algorithm.Models;
using OpenTK.Mathematics;

namespace Algorithm.Lib
{
    public static class Similarity
    {
        /// <summary>
        /// MeshPointCloudOverlapDetection
        /// Find a distance map from point in space ti map
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="pts"></param>
        /// <param name="precision"></param>
        /// <returns>Distance map</returns>
        public static async Task MeshPointCloudOverlapDetectionAsync(
            STL mesh,
            IAsyncEnumerable<Vector3> pts,
            double precision = 0.1)
        {
            // voxel denote point on grid with p value of depth, width, height
            // using snaps for each point to detect cluster

            // voxel grid holds pointers to all contained point clusters  
            var voxelMeshPosGrid = VoxelHelper.VoxelizeSTL(mesh, precision);
            var voxelMeshCluster = new Dictionary<Vector3, MeshCluster>();

            // map distance
            // var distanceMap = new List<Vector3>();

            // divide points for threadpool
            // pool

            // TODO
            // create thread pool
            
            await DetectionAsync(
                pts,
                voxelMeshPosGrid,
                voxelMeshCluster,
                precision
            );

            // return distanceMap;
        }

        /// <summary>
        /// Detection of distance overlap between cloud and mesh 
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        private static async Task DetectionAsync(
            IAsyncEnumerable<Vector3> stream,
            List<Vector3> voxelMeshPosGrid,
            Dictionary<Vector3, MeshCluster> voxelMeshCluster,
            double precision = 0.1
        )
        {
            // P
            await foreach (var pos in stream)
            {
                // Compute on GPU
            }
        }
    }
}