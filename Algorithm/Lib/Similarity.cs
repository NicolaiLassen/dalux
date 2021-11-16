using System;
using System.Collections.Generic;
using System.Threading;
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
        public static async Task MeshPointCloudOverlapDetectionAsync(
            STL mesh,
            IAsyncEnumerable<Vector3> pts,
            int resolution = 100)
        {
            // voxel denote point on grid with p value of depth, width, height
            // using snaps for each point to detect cluster

            // voxel grid holds pointers to all contained point clusters  
            // var voxelMeshPosGrid = VoxelHelper.VoxelizeSTL(mesh,precision);

            VoxelHelper.VoxelizeSTL(mesh, resolution);
            var voxelMeshCluster = new Dictionary<Vector3, MeshCluster>();

            // map distance
            // var distanceMap = new List<Vector3>();

            // divide points for threadpool
            // pool

            // TODO
            // create thread pool
            Environment.Exit(0);

            // var detectionState = new DetectionState
            // {
            //     Stream = pts,
            //     VoxelMeshPosGrid = voxelMeshPosGrid,
            //     VoxelMeshCluster = voxelMeshCluster,
            //     Precision = precision
            // };

            // DetectionAsync(detectionState);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stateInfo"></param>
        private static async void DetectionAsync(object stateInfo)
        {
            var detectionState = (DetectionState) stateInfo;

            var meshPointIrregularity = new List<Vector3>();

            // P
            await foreach (var pos in detectionState.Stream)
            {
                var distance = Vector3.Distance(pos, pos);
                Console.WriteLine(distance);
                // Compute on GPU
            }

            // add found points to clusters
            Monitor.Enter(detectionState.VoxelMeshCluster);
            // DO WORK
            Monitor.Exit(detectionState.VoxelMeshCluster);
        }
    }
}