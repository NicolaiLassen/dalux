﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace Algorithm.Lib
{
    public static class PTS
    {
        private static readonly Random Random = new Random();
        // private static readonly Queue<>

        /// <summary>
        /// Producer of point stream
        /// </summary>
        /// <returns></returns>
        public static async Task ProduceStream()
        {
        }

        /// <summary>
        ///  Consumer for point stream
        /// </summary>
        /// <param name="paths"></param>
        /// <param name="density"></param>
        /// <returns></returns>
        public static async IAsyncEnumerable<Vector3> ReadStreamAsync(IEnumerable<string> paths, double density)
        {
            // for each file path
            foreach (var path in paths)
            {
                // TODO AMAZON S3 STREAM
                using var sr = new StreamReader(path);

                while (!sr.EndOfStream)
                {
                    // read point line
                    var line = await sr.ReadLineAsync();

                    // distribute selected points randomly
                    if (!(density > Random.NextDouble())) continue;

                    // read more than one line to queq

                    // split line to access properties
                    // don't enfear string 
                    var point = line.Split(" ");

                    // convert axies to 3d vector for GPU calculation

                    // TODO: use concurrent Q and sem to load N entries into the RAM

                    yield return
                        new Vector3(
                            float.Parse(point[0], CultureInfo.GetCultureInfo("en-US")),
                            float.Parse(point[1], CultureInfo.GetCultureInfo("en-US")),
                            float.Parse(point[2], CultureInfo.GetCultureInfo("en-US"))
                        );
                }

                // close file
                sr.Close();
            }
        }
    }
}