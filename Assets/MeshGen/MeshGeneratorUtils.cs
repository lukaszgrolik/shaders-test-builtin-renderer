using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TerrainMeshGen
{
    static class MeshGeneratorUtils
    {
        static private readonly int[,] QUAD_TOP_BOTTOM_POINTS = new int[6, 2]{
            {0, 0}, {0, 1}, {1, 0},
            {1, 0}, {0, 1}, {1, 1},
        };
        static private readonly int[,] QUAD_BOTTOM_TOP_POINTS = new int[6, 2]{
            {0, 0}, {0, 1}, {1, 1},
            {1, 1}, {1, 0}, {0, 0},
        };

        // static public Vector3[] ObtainQuadRect(int x, int z, float[,] heights)
        // {
        //     if ((z % 2 == 0 && x % 2 == 0) || (z % 2 != 0 && x % 2 != 0))
        //     {
        //         return ObtainQuadTopBottom(x, z, heights);
        //     }
        //     else
        //     {
        //         return ObtainQuadBottomTop(x, z, heights);
        //     }
        // }

        static public Vector3[] ObtainQuadDiamond(int x, int z, float[,] heights)
        {
            if ((z % 2 == 0 && x % 2 == 0) || (z % 2 != 0 && x % 2 != 0))
            {
                return ObtainQuadTopBottom(x, z, heights);
            }
            else
            {
                return ObtainQuadBottomTop(x, z, heights);
            }
        }

        static public Vector3[] ObtainQuadTopBottom(int x, int z, float[,] heights)
        {
            return ObtainQuad(QUAD_TOP_BOTTOM_POINTS, x, z, heights);
        }

        static public Vector3[] ObtainQuadBottomTop(int x, int z, float[,] heights)
        {
            return ObtainQuad(QUAD_BOTTOM_TOP_POINTS, x, z, heights);
        }

        static public Vector3[] ObtainQuad(int[,] points, int x, int z, float[,] heights)
        {
            var result = new Vector3[6];
            int valX, valZ;
            // float valY;
            for (int i = 0; i < 6; i++)
            {
                valX = x + points[i, 0];
                valZ = z + points[i, 1];
                // valY = isFlat ? 0 : heights[valX, valZ];

                // result[i] = new Vector3((float)valX / xSize, valY, (float)valZ / zSize);
                // result[i] = new Vector3((float)valX / xSize, heights[valX, valZ], (float)valZ / zSize);
                result[i] = new Vector3((float)valX, heights[valX, valZ], (float)valZ);
            }

            return result;
        }

        static public float ObtainTriangleNoise(float[] values, TriangleColorMode mode)
        {
            switch (mode)
            {
                case TriangleColorMode.Avg:
                    var avg = 0f;
                    for (int i = 0; i < values.Length; i++) avg += values[i] / values.Length;

                    return avg;
                case TriangleColorMode.Min:
                    return Mathf.Min(values);
                case TriangleColorMode.Max:
                    return Mathf.Max(values);
                default:
                    throw new System.Exception("mode not found");
            }
        }

        static public (int, int) QuadIndexToCoords(int w, int h, int index)
        {
            var valX = index % w;
            var valY = Mathf.FloorToInt(index / h);

            return (valX, valY);
        }

        static public int QuadCoordsToIndex(int w, int h, int x, int y)
        {
            return x + w * y;
        }
    }
    }