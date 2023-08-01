using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TerrainMeshGen
{
    [System.Serializable]
    public class LerpSettings {
        public bool enabled = false;
        // public bool useHeightNoiseMinMax = false;
        public float a = 0.36f;
        public float b = 1.6f;
    }

    public class HeightColorPainter : MeshGeneratorPainter
    {
        public Gradient gradient;
        public LerpSettings lerpSettings = new LerpSettings();
        public bool evaluateGradient = true;

        private MapNoiseResult mapNoise;

        public override void Generate(Vector2 meshSize, MapNoiseResult mapNoise)
        {
            this.mapNoise = mapNoise;
        }

        public override Color GetTriangleColor(int x, int z, int index, float value)
        {
            float val;

            if (lerpSettings.enabled)
            {
                // if (lerpSettings.useHeightNoiseMinMax)
                // {
                //     val = Mathf.InverseLerp(mapNoise.minHeight, mapNoise.maxHeight, value);
                // }
                // else
                // {
                    val = Mathf.InverseLerp(lerpSettings.a, lerpSettings.b, value);
                // }
            }
            else
            {
                val = value;
            }

            if (evaluateGradient)
            {
                return gradient.Evaluate(val);
            }
            else
            {
                var c = Mathf.RoundToInt(Mathf.Lerp(0, gradient.colorKeys.Length - 1, value));
                // Debug.Log($"c: {c}");

                return gradient.colorKeys[c].color;
            }
        }

        // void OnValidate() {
        //     var meshGenerator = GetComponent<MeshGenerator>();

        //     if (meshGenerator) {
        //         meshGenerator.GenerateMap();
        //     }
        // }
    }
}