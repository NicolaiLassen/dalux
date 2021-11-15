namespace Algorithm.Models
{
    public class BoundingBoxCoords
    {
        public float XMax { get; set; } = float.MinValue;
        public float YMax { get; set; } = float.MinValue;
        public float ZMax { get; set; } = float.MinValue;
        public float XMin { get; set; } = float.MaxValue;
        public float YMin { get; set; } = float.MaxValue;
        public float ZMin { get; set; } = float.MaxValue;

        public override string ToString()
        {
            return $"XMax {XMax}, YMax {YMax}, ZMax {ZMax}, XMin {XMin}, YMin {YMin}, YMin {YMin},";
        }
    }
}