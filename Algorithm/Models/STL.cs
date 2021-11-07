using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Algorithm.Models;

namespace Algorithm.Lib
{
    public class STL : IEnumerable<Facet>
    {
        private readonly List<Facet> Facets = new List<Facet>();

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

        public void SaveAsBinary(string path)
        {
            using (Stream stream = File.Create(path))
                WriteBinary(stream);
        }

        public static STL Read(BinaryReader reader)
        {
            var stl = new STL();
            Facet currentFacet;

            // read (and ignore) the header and number of triangles
            reader.ReadBytes(80);
            reader.ReadBytes(4);

            // read each facet until the end of the stream. Stop when the end of the stream is reached
            while ((reader.BaseStream.Position != reader.BaseStream.Length) &&
                   (currentFacet = Facet.Read(reader)) != null)
                stl.Facets.Add(currentFacet);

            return stl;
        }

        private void WriteBinary(Stream stream)
        {
            using (BinaryWriter writer = new BinaryWriter(stream, Encoding.ASCII, true))
            {
                byte[] header = Encoding.ASCII.GetBytes("Dalux");
                byte[] headerFull = new byte[80];

                Buffer.BlockCopy(header, 0, headerFull, 0, Math.Min(header.Length, headerFull.Length));

                // write the header and facet count.
                writer.Write(headerFull);
                writer.Write((UInt32) Facets.Count);

                // write each facet.
                Facets.ForEach(o => o.Write(writer));
            }
        }
    }
}