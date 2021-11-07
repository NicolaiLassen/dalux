﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Algorithm.Helpers;
using OpenTK.Mathematics;

namespace Algorithm.Models
{
    public class Facet : IEnumerable<Vector3>
    {
        public Vector3 Normal { get; set; }

        public List<Vector3> Vertices { get; set; }

        public ushort AttributeByteCount { get; set; }

        public Facet()
        {
        }

        public Facet(Vector3 normal, List<Vector3> vertices, ushort attribute = ushort.MinValue)
        {
            Normal = normal;
            Vertices = vertices;
            AttributeByteCount = attribute;
        }

        public IEnumerator<Vector3> GetEnumerator()
        {
            return Vertices.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Write(BinaryWriter writer)
        {
            // write the normal
            Vector3Helpers.ToBinary(writer, Normal);

            // write each vertex
            Vertices.ForEach(vector3 => Vector3Helpers.ToBinary(writer, vector3));

            // write the attribute byte count
            writer.Write(AttributeByteCount);
        }

        public static Facet Read(BinaryReader reader)
        {
            // create the facet.
            return new Facet
            {
                Normal = Vector3Helpers.FromBinary(reader),
                Vertices = Enumerable.Range(0, 3).Select(o =>
                    Vector3Helpers.FromBinary(reader)
                ).ToList(),
                AttributeByteCount = reader.ReadUInt16()
            };
        }
    }
}