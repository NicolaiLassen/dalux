using System;
using System.IO;
using OpenTK.Mathematics;

namespace Algorithm.Helpers
{
    public static class Vector3Helpers
    {
        /// <summary>
        /// Read Vector3 from binary 3 floats
        /// </summary>
        /// <param name="reader"></param>
        /// <returns>Vector3 pos</returns>
        /// <exception cref="FormatException"></exception>
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

        /// <summary>
        /// Convert Vector3 to binary 
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="vector3"></param>
        public static void ToBinary(BinaryWriter writer, Vector3 vector3)
        {
            // write floats x,y,z
            var (x, y, z) = vector3;
            writer.Write(x);
            writer.Write(y);
            writer.Write(z);
        }
    }
}