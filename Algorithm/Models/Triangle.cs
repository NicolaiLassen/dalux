using OpenTK.Mathematics;

namespace Algorithm.Models
{
    public struct Triangle
    {
        public Vector3 A, B, C;
        public Vector3 Normal;
        public Bounds Bounds;

        public Triangle(Vector3 a, Vector3 b, Vector3 c, Vector3 normal)
        {
            A = a;
            B = b;
            C = c;
            Normal = normal;
            Bounds = new Bounds();

            var min = Vector3.ComponentMin(Vector3.ComponentMin(a, b), c);
            var max = Vector3.ComponentMax(Vector3.ComponentMax(a, b), c);
            Bounds.SetMinMax(min, max);
        }
    }
}