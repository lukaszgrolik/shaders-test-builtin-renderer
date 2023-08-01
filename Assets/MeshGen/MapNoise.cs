using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TerrainMeshGen
{
    [System.Serializable]
    public class MapNoiseResult : ISerializationCallbackReceiver
    {
        public float[,] values;
        public float minHeight;
        public float maxHeight;

        [SerializeField] private int sizeX;
        [SerializeField] private int sizeY;
        [SerializeField] private ResultValue[] valuesAsStructs;

        [System.Serializable]
        public struct ResultValue
        {
            public int x;
            public int y;
            public float value;
        }

        public MapNoiseResult(float[,] values, float minHeight, float maxHeight)
        {
            this.values = values;
            this.minHeight = minHeight;
            this.maxHeight = maxHeight;
        }

        public void OnBeforeSerialize()
        {
            if (values == null) return;

            sizeX = values.GetLength(0);
            sizeY = values.GetLength(1);
            valuesAsStructs = new ResultValue[sizeX * sizeY];

            var i = 0;
            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    valuesAsStructs[i] = new ResultValue { x = x, y = y, value = values[x, y] };
                    i += 1;
                }
            }
        }

        public void OnAfterDeserialize()
        {
            values = new float[sizeX, sizeY];

            for (int i = 0; i < valuesAsStructs.Length; i++)
            {
                var val = valuesAsStructs[i];

                values[val.x, val.y] = val.value;
            }
        }
    }

    public static class MapNoise
    {
        public static MapNoiseResult Generate(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistence, float lacunarity, Vector2 offset, bool normalize = false)
        {
            float[,] noiseMap = new float[mapWidth, mapHeight];

            System.Random prng = new System.Random(seed);
            Vector2[] octaveOffsets = new Vector2[octaves];

            for (int i = 0; i < octaves; i++)
            {
                float offsetX = prng.Next(-100000, 100000) + offset.x / scale;
                float offsetY = prng.Next(-100000, 100000) + offset.y / scale;

                octaveOffsets[i] = new Vector2(offsetX, offsetY);
            }
            // Debug.Log($"octaves: {octaves} | {System.String.Join(", ", octaveOffsets.Select((o) => $"({o.x}|{o.y})"))}");

            if (scale <= 0) scale = 0.0001f;

            float maxNoiseHeight = float.MinValue;
            float minNoiseHeight = float.MaxValue;

            float halfWidth = mapWidth / 2f;
            float halfHeight = mapHeight / 2f;

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    float amp = 1;
                    float freq = 1;
                    float noiseHeight = 0;

                    for (int i = 0; i < octaves; i++)
                    {
                        // float sampleX = (x - halfWidth) / scale * freq + octaveOffsets[i].x;
                        // float sampleX = (x) / scale * freq + octaveOffsets[i].x;
                        float sampleX = ((x) / scale + octaveOffsets[i].x) * freq;
                        // float sampleY = (y - halfHeight) / scale * freq + octaveOffsets[i].y;
                        // float sampleY = (y) / scale * freq + octaveOffsets[i].y;
                        float sampleY = ((y) / scale + octaveOffsets[i].y) * freq;
                        // float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                        float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);

                        noiseHeight += perlinValue * amp;

                        amp *= persistence;
                        freq *= lacunarity;
                    }

                    if (noiseHeight > maxNoiseHeight)
                    {
                        maxNoiseHeight = noiseHeight;
                    }
                    else if (noiseHeight < minNoiseHeight)
                    {
                        minNoiseHeight = noiseHeight;
                    }

                    noiseMap[x, y] = noiseHeight;
                }
            }

            // Debug.Log($"min: {minNoiseHeight} | max: {maxNoiseHeight}");
            if (normalize)
            {
                for (int y = 0; y < mapHeight; ++y)
                {
                    for (int x = 0; x < mapWidth; ++x)
                    {
                        noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
                        // noiseMap[x, y] = Mathf.InverseLerp(0.265f, 1.638f, noiseMap[x, y]);
                    }
                }
            }

            var result = new MapNoiseResult(noiseMap, minNoiseHeight, maxNoiseHeight);

            return result;
        }
    }
}
