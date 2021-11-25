using System.IO;
using Algorithm.Helpers;
using OpenTK.Mathematics;

namespace Algorithm.Models
{
    public class Facet
    {
        public Vector3 Normal { get; set; }

        public Vector3[] Vertices { get; set; }

        public Triangle Triangle { get; set; }

        public ushort AttributeByteCount { get; set; }

        public void Write(BinaryWriter writer)
        {
            // write the normal
            Vector3Helpers.ToBinary(writer, Normal);

            // write each vertex
            foreach (var vertex in Vertices)
            {
                Vector3Helpers.ToBinary(writer, vertex);
            }

            // write the attribute byte count
            writer.Write(AttributeByteCount);
        }

        public static Facet Read(BinaryReader reader)
        {
            // read normal
            var normal = Vector3Helpers.FromBinary(reader);

            // read very vertex
            var vertices = new Vector3[3];
            for (var i = 0; i < 3; i++)
            {
                vertices[i] = Vector3Helpers.FromBinary(reader);
            }

            // create facet
            return new Facet
            {
                Normal = normal,
                Vertices = vertices,
                Triangle = new Triangle(vertices[0], vertices[1], vertices[2], normal),
                AttributeByteCount = reader.ReadUInt16()
            };
        }
    }
}