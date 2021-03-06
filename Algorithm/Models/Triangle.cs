using OpenTK.Mathematics;

namespace Algorithm.Models
{
    public struct Triangle
    {
        public Vector3 A, B, C;
        public Vector3 Normal;

        public Triangle(Vector3 a, Vector3 b, Vector3 c, Vector3 normal)
        {
            A = a;
            B = b;
            C = c;
            Normal = normal;
        }
    }
}