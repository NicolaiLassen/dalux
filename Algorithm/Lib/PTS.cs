using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Text;
using OpenTK.Mathematics;

namespace Algorithm.Lib
{
    public static class PTS
    {
        private static readonly Random Random = new Random();
        // private static readonly Queue<>

        /// <summary>
        /// Count point cloud
        /// </summary>
        /// <param name="paths"></param>
        public static void PointCloudsSize(IEnumerable<string> paths)
        {
            var cnt = 0;
            foreach (var path in paths)
            {
                using var sr = new StreamReader(path, Encoding.UTF8, true, 1024);
                var size = sr.ReadLine();
                cnt += int.Parse(size);
            }

            Console.WriteLine(cnt);
        }


        public static async IAsyncEnumerable<Vector3> PointStreamAsync(IEnumerable<string> paths, double density)
        {
            // for each file path
            foreach (var path in paths)
            {
                var pathSplit = path.Split("\\");
                var mapName = pathSplit[^1];

                var fileSize = new FileInfo(path).Length;
                var densitySize = (int) (fileSize * density);

                var batchChunkSize = 500;
                var batches = densitySize / batchChunkSize;
                Console.WriteLine(batches);

                const long offset = 0;
                var length = fileSize;

                using var mmf = MemoryMappedFile.CreateFromFile(path, FileMode.Open, mapName);
                // random access a view
                using var accessor = mmf.CreateViewAccessor(offset, length, MemoryMappedFileAccess.Read);

                var charSize = Marshal.SizeOf(typeof(char));

                byte c;
                var line = "";
                var newLineCnt = 0;

                // Make changes to the view.
                for (long i = 0; i < densitySize - charSize; i += charSize)
                {
                    accessor.Read(i, out c);
                    var c1 = Convert.ToChar(c);

                    if (c1 == '\n')
                    {
                        newLineCnt++;
                    }

                    switch (newLineCnt)
                    {
                        case 1:
                            line += c1.ToString();
                            break;
                        case 2:
                        {
                            var point = line.Trim().Split(" ");
                            yield return
                                new Vector3(
                                    float.Parse(point[0], CultureInfo.GetCultureInfo("en-US")),
                                    float.Parse(point[1], CultureInfo.GetCultureInfo("en-US")),
                                    float.Parse(point[2], CultureInfo.GetCultureInfo("en-US"))
                                );
                            line = "";
                            newLineCnt = 0;
                            break;
                        }
                    }
                }
            }
        }
    }

    // /// <summary>
    // ///  Consumer for point stream
    // /// </summary>
    // /// <param name="paths"></param>
    // /// <param name="density"></param>
    // /// <returns></returns>
    // public static async IAsyncEnumerable<Vector3> PointStreamAsync(IEnumerable<string> paths, double density)
    // {
    //     // for each file path
    //     foreach (var path in paths)
    //     {
    //         // open stream for path
    //         using var sr = new StreamReader(path, Encoding.UTF8, true, 1024);
    //         // skip first if format has count
    //         await sr.ReadLineAsync();
    //
    //         // line stream for path
    //         while (!sr.EndOfStream)
    //         {
    //             // read point line
    //             var line = await sr.ReadLineAsync();
    //
    //             // distribute selected points randomly
    //             if (!(density > Random.NextDouble())) continue;
    //
    //             // read more than one line to queq
    //
    //             // split line to access properties
    //             var point = line.Split(" ");
    //
    //             // convert axies to 3d vector for GPU calculation
    //
    //             // TODO: use concurrent Q and sem to load N entries into the RAM
    //             // Producer consumer pattern
    //
    //             yield return
    //                 new Vector3(
    //                     float.Parse(point[0], CultureInfo.GetCultureInfo("en-US")),
    //                     float.Parse(point[1], CultureInfo.GetCultureInfo("en-US")),
    //                     float.Parse(point[2], CultureInfo.GetCultureInfo("en-US"))
    //                 );
    //         }
    //
    //         // close file
    //         sr.Close();
    //     }
    // }
}