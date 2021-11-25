using OpenTK.Mathematics;

namespace Algorithm.Models
{
    public class Bounds
    {
        public Vector3 Center { get; set; }
        public Vector3 Extents { get; set; }

        public Bounds()
        {
        }

        public Bounds(Vector3 center, Vector3 size)
        {
            Center = center;
            Extents = size * 0.5F;
        }

        public Vector3 size
        {
            get => Extents * 2.0F;
            set => Extents = value * 0.5F;
        }

        public void SetMinMax(Vector3 min, Vector3 max)
        {
            Extents = (max - min) * 0.5F;
            Center = min + Extents;
        }

        public Vector3 min
        {
            get => Center - Extents;
            set => SetMinMax(value, max);
        }

        public Vector3 max
        {
            get => Center + Extents;
            set => SetMinMax(min, value);
        }

        public override string ToString()
        {
            return $"Center {Center}, Extents{Extents}";
        }
    }
}