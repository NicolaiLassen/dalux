﻿using System.IO;
using System.Threading.Tasks;
using Algorithm.Lib;
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
            var ptsPaths = new[] {Path.Combine(TEMP_PATH, "mesh_not_correct.pts")};
            var ptsStream = PTS.ConsumeStreamAsync(ptsPaths, 1);

            // import mesh
            // var meshPath = Path.Combine(TEMP_PATH, "alignedIFCfile.stl"); // for speed test
            var meshPath = Path.Combine(TEMP_PATH, "part1_solid_correct.STL");
            var stl = STL.Read(new BinaryReader(new FileStream(meshPath, FileMode.Open)));

            // stl.NormalizeToCenter();
            stl.SaveAsBinary("normal_mesh.stl");

            // generate distance Map
            await Intersect.MeshPointCloudIntersectionAsync(stl, ptsStream, 100);
        }
    }
}