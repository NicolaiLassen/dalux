﻿using System;
using System.Collections.Generic;
using Algorithm.Lib;
using Algorithm.Models;
using OpenTK.Mathematics;

namespace Algorithm.Helpers
{
    public static class VoxelHelper
    {
        /// <summary>
        /// Convert triangle mesh into voxel list representation
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        public static List<Vector3> VoxelizeSTL(STL mesh, double precision = 0.1)
        {
            // https://link.springer.com/chapter/10.1007/978-3-030-21293-3_17

            // shared voxel data

            var voxelPoints = new List<Vector3>();

            Console.WriteLine(mesh.BoundingBoxCoords);

            // var distance = Vector3.Distance(pos, pos);


            return voxelPoints;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stateInfo"></param>
        private static void Voxelize(object stateInfo)
        {
        }

        /// <summary>
        /// Simple cube STL Shape
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static List<Facet> CreateSTLVoxel(Vector3 pos, float size, ushort attribute = ushort.MinValue)
        {
            // -1 and 1 center of point 
            // half the size to correct for two sided scaling [-1...1]
            var cubeSize = size / 2;

            // vertices 
            var v1 = new Vector3(1, 1, 1) * cubeSize + pos;
            var v2 = new Vector3(-1, 1, 1) * cubeSize + pos;
            var v3 = new Vector3(-1, -1, 1) * cubeSize + pos;
            var v4 = new Vector3(1, -1, 1) * cubeSize + pos;
            var v5 = new Vector3(1, -1, -1) * cubeSize + pos;
            var v6 = new Vector3(-1, -1, -1) * cubeSize + pos;
            var v7 = new Vector3(-1, 1, -1) * cubeSize + pos;
            var v8 = new Vector3(1, 1, -1) * cubeSize + pos;

            // normals 
            var n1 = new Vector3(0, 0, 1);
            var n2 = new Vector3(0, -1, 0);
            var n3 = new Vector3(-1, 0, 0);
            var n4 = new Vector3(0, 0, -1);
            var n5 = new Vector3(1, 0, 0);
            var n6 = new Vector3(0, 1, 0);

            // cube
            return new List<Facet>
            {
                new Facet
                {
                    Normal = n1,
                    Vertices = new List<Vector3>
                    {
                        v1,
                        v2,
                        v3
                    },
                    AttributeByteCount = attribute
                },
                new Facet
                {
                    Normal = n1,
                    Vertices = new List<Vector3>
                    {
                        v1,
                        v3,
                        v4
                    }
                },
                new Facet
                {
                    Normal = n2,
                    Vertices = new List<Vector3>
                    {
                        v5,
                        v4,
                        v3
                    },
                    AttributeByteCount = attribute
                },
                new Facet
                {
                    Normal = n2,
                    Vertices = new List<Vector3>
                    {
                        v5,
                        v3,
                        v6
                    },
                    AttributeByteCount = attribute
                },
                new Facet
                {
                    Normal = n3,
                    Vertices = new List<Vector3>
                    {
                        v6,
                        v3,
                        v2
                    },
                    AttributeByteCount = attribute
                },
                new Facet
                {
                    Normal = n3,
                    Vertices = new List<Vector3>
                    {
                        v6,
                        v2,
                        v7
                    },
                    AttributeByteCount = attribute
                },
                new Facet
                {
                    Normal = n4,
                    Vertices = new List<Vector3>
                    {
                        v7,
                        v8,
                        v5
                    },
                    AttributeByteCount = attribute
                },
                new Facet
                {
                    Normal = n4,
                    Vertices = new List<Vector3>
                    {
                        v7,
                        v5,
                        v6
                    },
                    AttributeByteCount = attribute
                },
                new Facet
                {
                    Normal = n5,
                    Vertices = new List<Vector3>
                    {
                        v8,
                        v1,
                        v4
                    },
                    AttributeByteCount = attribute
                },
                new Facet
                {
                    Normal = n5,
                    Vertices = new List<Vector3>
                    {
                        v8,
                        v4,
                        v5
                    },
                    AttributeByteCount = attribute
                },
                new Facet
                {
                    Normal = n6,
                    Vertices = new List<Vector3>
                    {
                        v7,
                        v2,
                        v1
                    },
                    AttributeByteCount = attribute
                },
                new Facet
                {
                    Normal = n6,
                    Vertices = new List<Vector3>
                    {
                        v7,
                        v1,
                        v8
                    },
                    AttributeByteCount = attribute
                }
            };
        }
    }
}