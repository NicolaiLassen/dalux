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
        public Bounds Bounds = new Bounds();

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

            var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            // read (and ignore) the header and number of triangles
            reader.ReadBytes(80);
            reader.ReadBytes(4);

            // read each facet until the end of the stream. Stop when the end of the stream is reached
            while (reader.BaseStream.Position != reader.BaseStream.Length &&
                   (currentFacet = Facet.Read(reader)) != null)
            {
                // add facet to stl doc
                stl.Facets.Add(currentFacet);

                // set bounds for stl model
                foreach (var vertex in currentFacet.Vertices)
                {
                    min = Vector3.ComponentMin(min, vertex);
                    max = Vector3.ComponentMax(max, vertex);
                    stl.Bounds.SetMinMax(min, max);
                }
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
    }
}