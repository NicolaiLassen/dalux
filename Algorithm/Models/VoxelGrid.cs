namespace Algorithm.Models
{
    public struct VoxelGrid
    {
        public byte[,,] Grid { get; set; }

        public static VoxelGrid CreateFromBounds(int w, int h, int d)
        {
            return new VoxelGrid
            {
                Grid = new byte[w, h, d]
            };
        }
    }
}