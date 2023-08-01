using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TerrainMeshGen
{
#if UNITY_EDITOR
    [CustomEditor(typeof(MeshGenerator))]
    public class MeshGenerator3Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            var script = (MeshGenerator)target;

            if (DrawDefaultInspector())
            {
                if (script.autoUpdate)
                {
                    script.Generate();
                }
            }

            if (GUILayout.Button("Generate"))
            {
                script.Generate();
            }

            if (GUILayout.Button("Clear")) {
                script.Clear();
            }
        }
    }
#endif

    [System.Serializable]
    public enum TriangleColorMode
    {
        Avg,
        Min,
        Max,
    }

    [System.Serializable]
    public enum TileStyle
    {
        Rect,
        TrianglesTopBottom,
        TrianglesBottomTop,
        Diamond,
    }

    public class MeshGenerator : MonoBehaviour
    {
        [System.Serializable]
        public enum HeightConfigLevelMode
        {
            MinValue,
            Step,
        }

        [System.Serializable]
        public class HeightConfigLevel
        {
            public float minValue = 0;
            public HeightConfigLevelMode mode = HeightConfigLevelMode.MinValue;
            public float step = .1f;
        }

        [System.Serializable]
        public class HeightConfig
        {
            public float step = 0;
            public HeightConfigLevel[] levels;
        }

        [System.Serializable]
        public class ColorReplace
        {
            public float chance = .5f;
            public int radius = 1;
        }

        public bool autoUpdate = false;
        public float tileSize = 1f;
        public bool isFlat = false;
        public HeightConfig heightConfig;
        public TileStyle tileStyle = TileStyle.Diamond;
        public TriangleColorMode[] triangleColorModes = new TriangleColorMode[2]{
            TriangleColorMode.Avg,
            TriangleColorMode.Avg,
        };
        public NoiseGenSettings noiseSettings;
        public MeshGeneratorPainter meshGeneratorPainter;
        public ColorReplace colorReplace;

        [HideInInspector] public float minHeight = Mathf.Infinity;
        [HideInInspector] public float maxHeight = -Mathf.Infinity;

        // Start is called before the first frame update
        void Start()
        {

        }

        public void Generate()
        {
            minHeight = Mathf.Infinity;
            maxHeight = -Mathf.Infinity;

            var mapNoises = new Dictionary<Transform, MapNoiseResult>();

            foreach (Transform child in transform)
            {
                var chunkScale = child.localScale;
                var xSize = Mathf.FloorToInt(chunkScale.x / tileSize);
                var zSize = Mathf.FloorToInt(chunkScale.z / tileSize);

                var ns = noiseSettings;
                ns.mapWidth = xSize + 1;
                ns.mapHeight = zSize + 1;
                ns.offset = (noiseSettings.offset + new Vector2(child.localPosition.x, child.localPosition.z)) / tileSize;
                var mapNoise = NoiseGen.Generate(ns);
                mapNoises.Add(child, mapNoise);

                if (mapNoise.minHeight < minHeight)
                {
                    minHeight = mapNoise.minHeight;
                }

                if (mapNoise.maxHeight > maxHeight)
                {
                    maxHeight = mapNoise.maxHeight;
                }
            }

            foreach (Transform child in transform)
            {
                var meshGenChunk = child.GetComponent<MeshGeneratorChunk2>();
                // if (!meshGenChunk) {
                //     meshGenChunk = child.gameObject.AddComponent<MeshGeneratorChunk>();
                // }
                meshGenChunk.meshGen = this;
                meshGenChunk.tileSize = tileSize;
                meshGenChunk.isFlat = isFlat;
                meshGenChunk.heightConfig = heightConfig;
                meshGenChunk.tileStyle = tileStyle;
                meshGenChunk.triangleColorModes = triangleColorModes;
                meshGenChunk.noiseSettings = noiseSettings;
                meshGenChunk.mapNoise = mapNoises[child];

                // if (meshGeneratorPainter is HeightColorPainter)
                // {
                //     var mgp = (meshGeneratorPainter as HeightColorPainter);
                //     mgp.lerpSettings.enabled = true;
                //     mgp.lerpSettings.a = minHeight;
                //     mgp.lerpSettings.b = maxHeight;
                // }

                meshGenChunk.meshGeneratorPainter = meshGeneratorPainter;
                meshGenChunk.colorReplace = colorReplace;

                meshGenChunk.GenerateMap(child);
            }
        }

        public void AddChunk()
        {

        }

        public void Clear()
        {
            foreach (Transform child in transform)
            {
                var meshGenChunk = child.GetComponent<MeshGeneratorChunk2>();
                meshGenChunk.Clear();
            }
        }

        private void CalcHeights()
        {

        }

    #if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            // if (!meshFilter) return;

            // // Debug.Log($"verts: {mesh.vertices.Length}");
            // // foreach (var vert in meshFilter.vertices)
            // // {
            // //     Gizmos.color = Color.red;
            // //     Gizmos.DrawSphere(transform.TransformPoint(vert), .1f);
            // // }

            // Gizmos.color = Color.red;
            // var pos = meshFilter.transform.position;
            // var scale = meshFilter.transform.localScale;

            // // Gizmos.DrawLine(pos, pos + Vector3.right * scale.x);
            // // Gizmos.DrawLine(pos, pos + Vector3.forward * scale.z);
            // // Gizmos.DrawLine(pos + Vector3.forward * scale.z, pos + Vector3.forward * scale.z + Vector3.right * scale.x);
            // // Gizmos.DrawLine(pos + Vector3.right * scale.x, pos + Vector3.right * scale.x + Vector3.forward * scale.z);
            // Gizmos.DrawWireCube(pos + scale / 2, scale);
            // Handles.Label(transform.position, $"({minHeight}, {maxHeight})");
        }
    #endif
    }
}