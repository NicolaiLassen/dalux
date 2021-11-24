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
        private List<Facet> Facets = new List<Facet>();
        public Bounds Bounds = new Bounds();
        public Triangle[] Triangles => GetTriangles();
        public Vector3[] Vertices => GetVertices();

        public STL()
        {
        }

        public STL(List<Facet> facets)
        {
            Facets = facets;
        }

        private Triangle[] GetTriangles()
        {
            var triangles = new Triangle[Facets.Count];
            for (var i = 0; i < Facets.Count; i++)
            {
                triangles[i] = Facets[i].Triangle;
            }

            return triangles;
        }

        private Vector3[] GetVertices()
        {
            var vertices = new Vector3[Facets.Count * 3];
            for (var i = 0; i < Facets.Count; i++)
            {
                for (var j = 0; j < 3; j++)
                {
                    vertices[i] = Facets[i].Vertices[j];
                }
            }

            return vertices;
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
        ///  Normalize mesh to center of XYZ from bounds
        /// </summary>
        public void NormalizeToCenter()
        {
            // CPU Bound
            //////// TODO

            // Create new facets list
            var facets = new List<Facet>();

            // subtract center from all vertexes so to norm around 000
            foreach (var facet in Facets)
            {
                var normVerts = new[]
                {
                    Vector3.Subtract(facet.Vertices[0], Bounds.Center),
                    Vector3.Subtract(facet.Vertices[1], Bounds.Center),
                    Vector3.Subtract(facet.Vertices[2], Bounds.Center),
                };

                facet.Vertices = normVerts;
                facets.Add(facet);
            }

            Facets = facets;

            // reset bounds
            CalculateBounds();
        }

        /// <summary>
        /// Recalculate the bounds of the mesh
        /// </summary>
        private void CalculateBounds()
        {
            var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            // update bounds for stl model
            foreach (var facet in Facets)
            {
                foreach (var vertex in facet.Vertices)
                {
                    min = Vector3.ComponentMin(min, vertex);
                    max = Vector3.ComponentMax(max, vertex);
                    Bounds.SetMinMax(min, max);
                }
            }
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
                // add facet to stl doc
                stl.Facets.Add(currentFacet);
            }

            stl.CalculateBounds();

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
            foreach (var facet in Facets)
            {
                facet.Write(writer);
            }
        }
    }
}