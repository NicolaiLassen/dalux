using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace Algorithm.Lib
{
    public static class SimilarityDetection
    {
        /// <summary>
        /// MeshPointCloudOverlapDetection
        /// Find a distance map from point in space ti map
        ///  
        /// </summary>
        /// <param name="ptsPaths"></param>
        /// <param name="meshPath"></param>
        /// <param name="ptsDensity"></param>
        /// <returns>Creates a shader for simulairty</returns>
        public static async Task MeshPointCloudOverlapDetectionAsync(
            STL mesh,
            IAsyncEnumerable<Vector3> pts,
            double precision = 0.1)
        {
            // voxel grid holds pointers to all contained point clusters  
            var voxelPosGrid = new Dictionary<Vector3, List<Vector3>>();
            
            
            // map distance
            var distanceMap = new Vector2();


            // divide points for threadpool
            // pool

            // Create thread pool
            await Detection(pts);

            // Return distance map
        }

        /// <summary>
        /// Time complexity
        /// P, V, 
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        private static async Task Detection(IAsyncEnumerable<Vector3> stream)
        {
            // P
            await foreach (var pos in stream)
            {
                // Compute on GPU

                Console.WriteLine(pos);
            }
        }
    }
}