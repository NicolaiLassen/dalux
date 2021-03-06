using System;
using System.IO;
using System.Threading.Tasks;
using Algorithm.Lib;
using Algorithm.Models;
using Algorithm.Services;

namespace Algorithm
{
    class Program
    {
        private const string TEMP_PATH =
            "C:\\Users\\nicol\\OneDrive\\Dokumenter\\DTU\\02830 Advanced Project in Digital Media Engineering\\dalux\\dalux\\Dalux\\Algorithm\\Tests";

        static async Task Main(string[] args)
        {
            // set compute
            ComputeService.SetComputePlatformIndex(1);

            // point stream async itr
            //  var ptsPaths = Directory.GetFiles(Path.Combine(TEMP_PATH, "pts")) // for speed test
            var ptsPaths = new[] {Path.Combine(TEMP_PATH, "COWI_25pct__SW181.pts")};
            var ptsStream = PTS.PointStreamAsync(ptsPaths, 0.001);

            // import mesh
            var meshPath = Path.Combine(TEMP_PATH, "part1_solid_correct.STL");
            var stl = STL.Read(new BinaryReader(new FileStream(meshPath, FileMode.Open)));

            // stl.NormalizeToCenter();
            stl.SaveAsBinary("mesh.stl");

            // generate error grid
            await Intersect.MeshPointCloudIntersectionAsync(stl, ptsStream, 256);
        }
    }
}