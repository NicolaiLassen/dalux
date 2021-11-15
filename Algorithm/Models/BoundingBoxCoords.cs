using OpenTK.Mathematics;

namespace Algorithm.Models
{
    public class BoundingBoxCoords
    {
        public Vector3 XMax { get; set; }
        public Vector3 YMax { get; set; }
        public Vector3 ZMax { get; set; }
        public Vector3 XMin { get; set; }
        public Vector3 YMin { get; set; }
        public Vector3 ZMin { get; set; }
    }
}