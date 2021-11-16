using System.Collections.Generic;
using OpenTK.Mathematics;

namespace Algorithm.Models
{
    public class DetectionState
    {
        public IAsyncEnumerable<Vector3> Stream { get; set; }
        public bool[,,] VoxelMeshPosGrid { get; set; }
        public Dictionary<Vector3, MeshCluster> VoxelMeshCluster { get; set; }
        public Vector3 NormalizeBoundCenter { get; set; } = Vector3.Zero;
    }
}