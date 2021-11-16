using System;
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
        public static bool[,,] VoxelizeSTL(STL mesh, int resolution = 100)
        {
            var bounds = mesh.Bounds;
            var maxLength =
                MathHelper.Max(bounds.size.X, MathHelper.Max(bounds.size.Y, bounds.size.Z));
            var unit = maxLength / resolution;
            var hunit = unit * 0.5f;

            var start = bounds.min - new Vector3(hunit, hunit, hunit);
            var end = bounds.max + new Vector3(hunit, hunit, hunit);
            var (fx, fy, fz) = end - start;

            var width = (int) MathHelper.Ceiling(fx / unit);
            var height = (int) MathHelper.Ceiling(fy / unit);
            var depth = (int) MathHelper.Ceiling(fz / unit);

            var volume = new Vector3[width, height, depth];
            var boxes = new Bounds[width, height, depth];
            var voxelSize = Vector3.One * unit;
            
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    for (var z = 0; z < depth; z++)
                    {
                        var p = new Vector3(x, y, z) * unit + start;
                        var aabb = new Bounds(p, voxelSize);
                        boxes[x, y, z] = aabb;
                    }
                }
            }


            return new bool[1,1,1];
            // var vertices = mesh;
            // var uvs = mesh.uv;
            // var uv00 = Vector2.zero;
            // var indices = mesh.triangles;
            // var direction = Vector3.forward;
            
            // var distance = Vector3.Distance(pos, pos);
        }

        /// <summary>
        /// Möller–Trumbore intersection algorithm
        /// </summary>
        /// <returns></returns>
        // bool RayIntersectsTriangle()
        // {
        //     
        // }


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