using System.Collections.Generic;
using OpenTK.Mathematics;

namespace Algorithm.Models
{
    public class MeshCluster
    {
        public Vector3 MeshPoint { get; set; }
        public List<Vector3> Cluster { get; set; }
    }
}