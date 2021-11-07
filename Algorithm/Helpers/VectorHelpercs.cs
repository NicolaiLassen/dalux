using System;
using System.IO;
using OpenTK.Mathematics;

namespace Algorithm.Helpers
{
    public static class Vector3Helpers
    {
        public static Vector3 FromBinary(BinaryReader reader)
        {
            const int floatSize = sizeof(float);
            const int vertexSize = (floatSize * 3);

            // read the 3 binary floats
            var data = new byte[vertexSize];
            var bytesRead = reader.Read(data, 0, data.Length);

            // if malformed
            if (bytesRead != data.Length)
                throw new FormatException(
                    $"Could not convert the binary data to a vertex. Expected ${vertexSize} bytes but found ${bytesRead}.");

            // convert float bytes to numeric representations as a Vector3
            return new Vector3
            {
                X = BitConverter.ToSingle(data, 0),
                Y = BitConverter.ToSingle(data, floatSize),
                Z = BitConverter.ToSingle(data, (floatSize * 2))
            };
        }

        public static void ToBinary(BinaryWriter writer, Vector3 vector3)
        {
            // write floats x,y,z
            writer.Write(vector3.X);
            writer.Write(vector3.Y);
            writer.Write(vector3.Z);
        }
    }
}