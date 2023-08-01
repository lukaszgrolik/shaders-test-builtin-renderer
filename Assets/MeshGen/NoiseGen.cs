using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TerrainMeshGen
{
    [System.Serializable]
    public struct NoiseGenSettings
    {
        public int mapWidth;
        public int mapHeight;
        public int seed;
        public float scale;
        [Min(1)] public int octaves;
        [Range(0, 1)] public float persistence;
        public float lacunarity;
        public Vector2 offset;
        public bool normalize;
    }

    public struct NoiseGenResult
    {
        public float[,] values;
        public float minHeight;
        public float maxHeight;

        public NoiseGenResult(float[,] values, float minHeight, float maxHeight)
        {
            this.values = values;
            this.minHeight = minHeight;
            this.maxHeight = maxHeight;
        }
    }

    public static class NoiseGen
    {
        public static MapNoiseResult Generate(NoiseGenSettings options)
        {
            float[,] noiseMap = new float[options.mapWidth, options.mapHeight];

            System.Random prng = new System.Random(options.seed);
            Vector2[] octaveOffsets = new Vector2[options.octaves];

            for (int i = 0; i < options.octaves; i++)
            {
                float offsetX = prng.Next(-100000, 100000) + options.offset.x / options.scale;
                float offsetY = prng.Next(-100000, 100000) + options.offset.y / options.scale;

                octaveOffsets[i] = new Vector2(offsetX, offsetY);
            }
            // Debug.Log($"octaves: {octaves} | {System.String.Join(", ", octaveOffsets.Select((o) => $"({o.x}|{o.y})"))}");

            if (options.scale <= 0) options.scale = 0.0001f;

            float maxNoiseHeight = float.MinValue;
            float minNoiseHeight = float.MaxValue;

            float halfWidth = options.mapWidth / 2f;
            float halfHeight = options.mapHeight / 2f;

            for (int y = 0; y < options.mapHeight; y++)
            {
                for (int x = 0; x < options.mapWidth; x++)
                {
                    float amp = 1;
                    float freq = 1;
                    float noiseHeight = 0;

                    for (int i = 0; i < options.octaves; i++)
                    {
                        // float sampleX = (x - halfWidth) / scale * freq + octaveOffsets[i].x;
                        // float sampleX = (x) / scale * freq + octaveOffsets[i].x;
                        float sampleX = ((x) / options.scale + octaveOffsets[i].x) * freq;
                        // float sampleY = (y - halfHeight) / scale * freq + octaveOffsets[i].y;
                        // float sampleY = (y) / scale * freq + octaveOffsets[i].y;
                        float sampleY = ((y) / options.scale + octaveOffsets[i].y) * freq;
                        // float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                        float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);

                        noiseHeight += perlinValue * amp;

                        amp *= options.persistence;
                        freq *= options.lacunarity;
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

            if (options.normalize)
            {
                for (int y = 0; y < options.mapHeight; ++y)
                {
                    for (int x = 0; x < options.mapWidth; ++x)
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
