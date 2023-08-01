Shader "Lukasz/Ripple"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _Red ("Red", Float) = 1
    }
    SubShader
    {
        Tags
        {
            "Queue" = "Geometry"
            // "IgnoreProjector"="True"
            "RenderType"="Opaque"
            "PreviewType"="Plane"
            // "CanUseSpriteAtlas"="True"
        }

        // Cull Off
        // ZWrite Off
        // ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD0;
                float2 uv : TEXCOORD1;
                float4 localPosition : TEXCOORD2;
                float4 color : COLOR;
            };

            float GetWave(float2 uv)
            {
                float2 uvCenter = uv * 2 - 1;
                float radDist = length(uvCenter);
                float val = cos((radDist - _Time.y * .1) * 10);
                val *= 1 - radDist;

                return val;
            }

            v2f vert (appdata v)
            {
                v2f OUT;

                // length(v.vertex)

                v.vertex.y = GetWave(v.uv) * .5;

                OUT.vertex = UnityObjectToClipPos(v.vertex);
                // OUT.normal = v.normal;
                OUT.normal = UnityObjectToWorldNormal(v.normal);
                OUT.uv = v.uv;
                OUT.localPosition = v.vertex;
                OUT.color = float4(v.vertex.xyz, 1);
                return OUT;
            }

            sampler2D _MainTex;
            float4 _BaseColor;
            float _Red;

            float3 roundDecimal(float3 val, float prec)
            {
                float x = pow(10, prec);

                return round(val * x) / x;
            }

            float2 roundDecimal(float2 val, float prec)
            {
                float x = pow(10, prec);

                return round(val * x) / x;
            }


            float inversedLerp(float a, float b, float v)
            {
                return (v - a) / (b - a);
            }

            fixed4 frag (v2f IN) : SV_Target
            {
                // fixed4 col = tex2D(_MainTex, IN.uv + float2(0, sin(IN.vertex.x / 25 + _Time.y * 5) / 25));
                fixed4 texCol = tex2D(_MainTex, IN.uv);

                float4 colorA = float4(1, 0, 0, 1);
                float4 colorB = float4(0, 0, 1, 1);

                // return float4(IN.uv, 0, 1);
                return texCol * lerp(colorB, colorA, GetWave(IN.uv));

                float start = .4;
                float end = .6;
                // float t = inversedLerp(start, end, IN.uv.y);
                float t = saturate(inversedLerp(start, end, IN.uv.y));
                // t = frac(t);
                // return t;
                float3 outColor = lerp(colorA, colorB, t);

                return texCol * float4(outColor, 1);
            }
            ENDCG
        }
    }
}
