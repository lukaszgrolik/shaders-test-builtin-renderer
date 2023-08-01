using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TerrainMeshGen
{
#if UNITY_EDITOR
    [CustomEditor(typeof(MeshGeneratorChunk2))]
    public class MeshGeneratorChunk2Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            var script = (MeshGeneratorChunk2)target;

            DrawDefaultInspector();

            // if (DrawDefaultInspector())
            // {
            //     if (script.autoUpdate)
            //     {
            //         script.Generate();
            //     }
            // }

            if (GUILayout.Button("Generate"))
            {
                script.GenerateMap(script.transform);
            }

            // if (GUILayout.Button("Clear")) {
            //     script.Clear();
            // }
        }
    }
#endif

    public class MeshGeneratorChunk2 : MonoBehaviour
    {
        [System.Serializable]
        public struct ColorLayer
        {
            public ColorTriangle[] colors;
        }

        [System.Serializable]
        public struct ColorTriangle
        {
            public Color color;
            public float opacity;
        }

        public float tileSize = 1f;
        public bool isFlat = false;
        public MeshGenerator meshGen;
        public MeshGenerator.HeightConfig heightConfig;
        public TileStyle tileStyle = TileStyle.Diamond;
        public TriangleColorMode[] triangleColorModes = new TriangleColorMode[2]{
            TriangleColorMode.Avg,
            TriangleColorMode.Avg,
        };
        // [HideInInspector] public NoiseGenSettings noiseSettings;
        public NoiseGenSettings noiseSettings;
        [HideInInspector] public MeshGeneratorPainter meshGeneratorPainter;
        [HideInInspector] public MeshGenerator.ColorReplace colorReplace;

        public MapNoiseResult mapNoise;
        // private MapNoiseResult mapNoise;
        public List<ColorLayer> colorLayers = new List<ColorLayer>();

        public int xSize { get; private set; }
        public int zSize { get; private set; }

        private Mesh mesh;
        private Vector3[] vertices;
        private Vector2[] uv;
        private int[] triangles;
        private Color[] colors;

        public List<int> displacedTriangles = new List<int>();

        void Start()
        {
            // @todo DRY
            var chunkScale = transform.localScale;
            xSize = Mathf.FloorToInt(chunkScale.x / tileSize);
            zSize = Mathf.FloorToInt(chunkScale.z / tileSize);
        }

        public void GenerateMap(Transform chunkTransform)
        {
            var meshFilter = GetComponent<MeshFilter>();

            CreateShape(chunkTransform);
            UpdateMesh(meshFilter);
        }

        public void Clear()
        {
            var meshFilter = GetComponent<MeshFilter>();

            if (vertices != null) System.Array.Clear(vertices, 0, vertices.Length);
            if (uv != null) System.Array.Clear(uv, 0, uv.Length);
            if (triangles != null) System.Array.Clear(triangles, 0, triangles.Length);
            if (colors != null) System.Array.Clear(colors, 0, colors.Length);

            colorLayers.Clear();

            UpdateMesh(meshFilter);
        }

        private void CreateShape(Transform chunkGO)
        {
            var chunkScale = chunkGO.localScale;
            xSize = Mathf.FloorToInt(chunkScale.x / tileSize);
            zSize = Mathf.FloorToInt(chunkScale.z / tileSize);

            vertices = new Vector3[xSize * zSize * 6];
            uv = new Vector2[xSize * zSize * 6];
            triangles = new int[xSize * zSize * 6];
            colors = new Color[xSize * zSize * 6];

            // var ns = noiseSettings;
            // ns.mapWidth = xSize + 1;
            // ns.mapHeight = zSize + 1;
            // ns.offset = (noiseSettings.offset + new Vector2(transform.localPosition.x, transform.localPosition.z)) / tileSize;
            // mapNoise = NoiseGen.Generate(ns);

            meshGeneratorPainter.Generate(new Vector2(xSize, zSize), mapNoise);

            if (colorLayers.Count == 0) AddColorLayer();
            var baseColorLayer = colorLayers[0];

            displacedTriangles.Clear();

            var iQuad = 0;
            var i = 0;

            for (int z = 0; z < zSize; z++)
            {
                for (int x = 0; x < xSize; x++)
                {
                    var quadVertices = ObtainQuadVertices(x, z);

                    for (int j = 0; j < 6; j++)
                    {
                        vertices[i + j] = quadVertices[j];
                        uv[i + j] = new Vector2(x / (float)xSize, z / (float)zSize);
                        triangles[i + j] = i + j;
                    }

                    var noiseValA = ObtainTriangleNoiseValue(quadVertices, 0);
                    var noiseValB = ObtainTriangleNoiseValue(quadVertices, 1);
                    var colorA = meshGeneratorPainter.GetTriangleColor(x, z, 0, noiseValA);
                    Color colorB;

                    if (tileStyle == TileStyle.Rect)
                        colorB = colorA;
                    else
                        colorB = meshGeneratorPainter.GetTriangleColor(x, z, 1, noiseValB);

                    colors[i + 0] = colorA;
                    colors[i + 1] = colorA;
                    colors[i + 2] = colorA;
                    colors[i + 3] = colorB;
                    colors[i + 4] = colorB;
                    colors[i + 5] = colorB;

                    baseColorLayer.colors[i / 3] = new ColorTriangle { color = colorA, opacity = 1 };
                    baseColorLayer.colors[i / 3 + 1] = new ColorTriangle { color = colorB, opacity = 1 };

                    iQuad += 1;
                    i += 6;
                }
            }

            {
                Vector3 vert;
                for (int j = 0; j < vertices.Length; j++)
                {
                    vert = vertices[j];
                    vert.x = vert.x / chunkScale.x * tileSize;
                    vert.z = vert.z / chunkScale.z * tileSize;
                    vertices[j] = vert;
                }
            }

            if (isFlat)
            {
                Vector3 vert;

                for (int j = 0; j < vertices.Length; j++)
                {
                    vert = vertices[j];
                    vert.y = 0;
                    vertices[j] = vert;

                    // @todo adjust uv?
                }
            }
            else if (heightConfig != null)
            {
                Vector3 vert;

                if (heightConfig.step > 0)
                {
                    for (int j = 0; j < vertices.Length; j++)
                    {
                        vert = vertices[j];
                        vert.y -= vert.y % heightConfig.step;

                        vertices[j] = vert;
                    }
                }
                else if (heightConfig.levels != null && heightConfig.levels.Length > 0)
                {
                    for (int j = 0; j < vertices.Length; j++)
                    {
                        vert = vertices[j];

                        MeshGenerator.HeightConfigLevel level = null;
                        for (int k = heightConfig.levels.Length - 1; k >= 0; k--)
                        {
                            var lvl = heightConfig.levels[k];
                            if (vert.y >= lvl.minValue)
                            {
                                level = lvl;
                                break;
                            }
                        }

                        if (level != null)
                        {
                            if (level.mode == MeshGenerator.HeightConfigLevelMode.MinValue)
                                vert.y = level.minValue;
                            else if (level.mode == MeshGenerator.HeightConfigLevelMode.Step && level.step != 0)
                                vert.y -= vert.y % level.step;

                            vertices[j] = vert;
                        }
                    }
                }
            }

            // color replacing
            // @todo replace between chunks (on chunk edges)
            // @todo replace on when colors differ
            if (colorReplace.chance > 0 && colorReplace.radius > 0)
            {
                var randChance = new System.Random(noiseSettings.seed);
                var randSampleX = new System.Random(noiseSettings.seed);
                var randSampleZ = new System.Random(noiseSettings.seed);
                var sampleRadius = colorReplace.radius;

                for (int z = 0; z < zSize; z++)
                {
                    if (z >= zSize - sampleRadius || z <= sampleRadius) continue;

                    for (int x = 0; x < xSize; x++)
                    {
                        if (randChance.NextDouble() > colorReplace.chance) continue;
                        if (x >= xSize - sampleRadius || x <= sampleRadius) continue;

                        var colorAQuadIndex = x + zSize * z;
                        var colorATriangleIndex = colorAQuadIndex * 2;
                        if (displacedTriangles.Contains(colorATriangleIndex)) continue;

                        // @todo omit already processed
                        var sampleX = x + sampleRadius * (randSampleX.Next(2) > 0 ? 1 : -1);
                        var sampleZ = z + sampleRadius * (randSampleX.Next(2) > 0 ? 1 : -1);
                        var colorBQuadIndex = sampleX + zSize * sampleZ;
                        var colorBTriangleIndex = colorBQuadIndex * 2;
                        if (displacedTriangles.Contains(colorBTriangleIndex)) continue;

                        displacedTriangles.Add(colorATriangleIndex);
                        displacedTriangles.Add(colorBTriangleIndex);
                        var colorA = colors[colorAQuadIndex * 6 + 0];
                        var colorB = colors[colorBQuadIndex * 6 + 0];

                        colors[colorAQuadIndex * 6 + 0] = colorB;
                        colors[colorAQuadIndex * 6 + 1] = colorB;
                        colors[colorAQuadIndex * 6 + 2] = colorB;

                        colors[colorBQuadIndex * 6 + 0] = colorA;
                        colors[colorBQuadIndex * 6 + 1] = colorA;
                        colors[colorBQuadIndex * 6 + 2] = colorA;

                        var colorATriangle = baseColorLayer.colors[colorAQuadIndex];
                        colorATriangle.color = colorB;
                        var colorBTriangle = baseColorLayer.colors[colorBQuadIndex];
                        colorBTriangle.color = colorA;
                    }
                }
            }
        }

        Vector3[] ObtainQuadVertices(int x, int z)
        {
            switch (tileStyle)
            {
                case TileStyle.Rect:
                    return MeshGeneratorUtils.ObtainQuadDiamond(x, z, mapNoise.values);
                case TileStyle.TrianglesTopBottom:
                    return MeshGeneratorUtils.ObtainQuadTopBottom(x, z, mapNoise.values);
                case TileStyle.TrianglesBottomTop:
                    return MeshGeneratorUtils.ObtainQuadBottomTop(x, z, mapNoise.values);
                case TileStyle.Diamond:
                    return MeshGeneratorUtils.ObtainQuadDiamond(x, z, mapNoise.values);
                default:
                    return new Vector3[0];
            }
        }

        float ObtainTriangleNoiseValue(Vector3[] quadVertices, int index)
        {
            var heights = new float[]{
                quadVertices[3 * index + 0].y,
                quadVertices[3 * index + 1].y,
                quadVertices[3 * index + 2].y,
            };

            return MeshGeneratorUtils.ObtainTriangleNoise(heights, triangleColorModes[index]);
        }

        public float ObtainTriangleNoiseValue(int x, int z, int index)
        {
            var quadVertices = ObtainQuadVertices(x, z);

            return ObtainTriangleNoiseValue(quadVertices, index);
        }

        private void UpdateMesh(MeshFilter meshFilter)
        {
            // if (meshFilter.mesh == null)
            // {
            //     mesh = new Mesh();
            //     meshFilter.mesh = mesh;
            // }
            mesh = new Mesh();
            meshFilter.mesh = mesh;

            mesh.Clear();

            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.colors = colors;

            mesh.RecalculateNormals();
            // mesh.RecalculateBounds();

            var meshCollider = meshFilter.gameObject.GetComponent<MeshCollider>();

            if (meshCollider != null)
            {
                meshCollider.sharedMesh = null;
                meshCollider.sharedMesh = mesh;
            }
        }

        public void AddColorLayer()
        {
            var newColorLayer = new ColorLayer { colors = new ColorTriangle[xSize * zSize * 2] };
            colorLayers.Add(newColorLayer);
        }

        // private Vector3 ObtainVertex(float x, float z) {
        //     float y = Mathf.PerlinNoise(x * .3f, z * .3f) * 2f;
        //     if (y < minY) minY = y;
        //     if (y > maxY) maxY = y;

        //     return new Vector3(x, y, z);
        // }

    #if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            if (mapNoise == null) return;

            Handles.Label(transform.position + new Vector3(5, 0, 5), $"({mapNoise.minHeight}, {mapNoise.maxHeight})");
        }
    #endif
    }
}
