using System.IO;
using System.Threading.Tasks;
using Algorithm.Lib;
using Algorithm.Models;

namespace Algorithm
{
    class Program
    {
        private const string TEMP_PATH =
            "C:\\Users\\nicol\\OneDrive\\Dokumenter\\DTU\\02830 Advanced Project in Digital Media Engineering\\dalux\\dalux\\Dalux\\Algorithm\\Tests";

        static async Task Main(string[] args)
        {
            // point stream async itr
            var ptsPaths = new[] {Path.Combine(TEMP_PATH, "mesh_not_correct.pts")};
            var ptsStream = PTS.ConsumeStreamAsync(ptsPaths, 0.4);

            // import mesh
            var meshPath = Path.Combine(TEMP_PATH, "bunny.stl");
            var stl = STL.Read(new BinaryReader(new FileStream(meshPath, FileMode.Open)));

            // generate distance Map
            await Similarity.MeshPointCloudOverlapDetectionAsync(stl, ptsStream, 10);

            // feed distance map to shader
            var shader = Shader.ShaderFromDistanceMap();

            // write shader to file
            shader.Write();
        }
    }
}