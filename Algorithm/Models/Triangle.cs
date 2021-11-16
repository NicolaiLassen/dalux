using OpenTK.Mathematics;

namespace Algorithm.Models
{
    public class Triangle
    {
        public Vector3 a, b, c;
        public Bounds bounds;
        public bool frontFacing;

        public Triangle(Vector3 a, Vector3 b, Vector3 c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            
            // var cross = Vector3.Cross(b - a, c - a);
            // frontFacing = (Vector3.Dot(cross, dir) <= 0f);
            //
            // var min = Vector3.Min(Vector3.Min(a, b), c);
            // var max = Vector3.Max(Vector3.Max(a, b), c);
            // bounds.SetMinMax(min, max);
        }

        public Vector2 GetUV(Vector3 p, Vector2 uva, Vector2 uvb, Vector2 uvc)
        {
            float u, v, w;
            // Vector3.BaryCentric(p, out u, out v, out w);
            return Vector2.One;
        }
        
    }
}