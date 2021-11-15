using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Algorithm.Models;
using OpenTK.Mathematics;

namespace Algorithm.Lib
{
    public class STL : IEnumerable<Facet>
    {
        private readonly List<Facet> Facets = new List<Facet>();
        public BoundingBoxCoords BoundingBoxCoords = new BoundingBoxCoords();

        public STL()
        {
        }

        public STL(List<Facet> facets)
        {
            Facets = facets;
        }

        public IEnumerator<Facet> GetEnumerator()
        {
            return Facets.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Write current mesh as binary 
        /// </summary>
        /// <param name="path"></param>
        public void SaveAsBinary(string path)
        {
            using Stream stream = File.Create(path);
            WriteBinary(stream);
        }

        /// <summary>
        /// Read a binary STL file
        /// </summary>
        /// <param name="stream"></param>
        public static STL Read(BinaryReader reader)
        {
            var stl = new STL();

            Facet currentFacet;

            // read (and ignore) the header and number of triangles
            reader.ReadBytes(80);
            reader.ReadBytes(4);

            // read each facet until the end of the stream. Stop when the end of the stream is reached
            while (reader.BaseStream.Position != reader.BaseStream.Length &&
                   (currentFacet = Facet.Read(reader)) != null)
            {
                stl.Facets.Add(currentFacet);
                DecorateStlBoundingBox(stl.BoundingBoxCoords, currentFacet.Vertices);
            }

            return stl;
        }

        /// <summary>
        /// Write STL as binary
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private void WriteBinary(Stream stream)
        {
            // writer
            using var writer = new BinaryWriter(stream, Encoding.ASCII, true);

            // buffer
            var header = Encoding.ASCII.GetBytes("Dalux");
            var headerFull = new byte[80];
            var count = Math.Min(header.Length, headerFull.Length);
            Buffer.BlockCopy(header, 0, headerFull, 0, count);

            // write the header and facet count.
            writer.Write(headerFull);
            writer.Write((uint) Facets.Count);

            // write each facet.
            Facets.ForEach(o => o.Write(writer));
        }

        /// <summary>
        /// Find the max and min bounds of the mesh
        /// </summary>
        /// <param name="currentBound"></param>
        /// <param name="vertices"></param>
        private static void DecorateStlBoundingBox(BoundingBoxCoords currentBound, IEnumerable<Vector3> vertices)
        {
            foreach (var vector3 in vertices)
            {
                currentBound.XMax = MathHelper.Max(currentBound.XMax, vector3[0]);
                currentBound.YMax = MathHelper.Max(currentBound.YMax, vector3[1]);
                currentBound.ZMax = MathHelper.Max(currentBound.ZMax, vector3[2]);
                currentBound.XMin = MathHelper.Min(currentBound.XMin, vector3[0]);
                currentBound.YMin = MathHelper.Min(currentBound.YMin, vector3[1]);
                currentBound.ZMin = MathHelper.Min(currentBound.ZMin, vector3[2]);
            }
        }
    }
}