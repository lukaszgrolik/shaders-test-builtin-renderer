using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TerrainMeshGen
{
    public abstract class MeshGeneratorPainter : MonoBehaviour
    {
        public abstract void Generate(Vector2 meshSize, MapNoiseResult mapNoise);
        public abstract Color GetTriangleColor(int x, int z, int index, float value);
    }
}