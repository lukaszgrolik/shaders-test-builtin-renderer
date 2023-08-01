Shader "Lukasz/Tint"
{
    Properties
    {
        _Enabled ("Enabled", Range(0, 1)) = 1
        _MainTex ("Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,0,0,1)
    }
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            // "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            // "CanUseSpriteAtlas"="True"
        }

        Cull Back
        ZWrite Off
        ZTest LEqual
        Blend One OneMinusSrcAlpha

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
                float3 localPos : TEXCOORD2;
                float3 globalPos : TEXCOORD3;
                float4 color : COLOR;
            };

            v2f vert (appdata v)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(v.vertex);
                // OUT.normal = v.normal;
                OUT.normal = UnityObjectToWorldNormal(v.normal);
                OUT.uv = v.uv;
                OUT.localPos = v.vertex;
                OUT.globalPos = mul(unity_ObjectToWorld, v.vertex);
                OUT.color = float4(v.vertex.xyz, 1);
                return OUT;
            }

            float _Enabled;
            sampler2D _MainTex;
            float4 _BaseColor;

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

            float3 roundResolution(float3 val, float res)
            {
                // return round(val * prec / parts) / prec;
                // return round(val / res) - val % res;
            }

            float inversedLerp(float a, float b, float v)
            {
                return (v - a) / (b - a);
            }

            fixed4 frag (v2f IN) : SV_Target
            {
                return _Enabled * _BaseColor;
                // return _Red;
                // return _BaseColor;
                // return float4(abs(IN.globalPos) / 10, 1);

                // fixed4 col = tex2D(_MainTex, IN.uv + float2(0, sin(IN.vertex.x / 25 + _Time.y * 5) / 25));
                fixed4 texCol = tex2D(_MainTex, IN.uv);
                // just invert the colors
                // col.rgb = 1 - IN.Color.rgb;
                // IN.color = texCol * float4(1, 0, 0, 1);
                // IN.color = texCol * IN.color;
                // IN.color = texCol * _BaseColor;
                // IN.color = _BaseColor;

                // return IN.color;
                // return IN.vertex.x > 120 ? IN.color : float4(1, 1, 1, 1);
                // return IN.localPos.x > 0 && IN.localPos.y > 0 ? IN.color : float4(1, 1, 1, 1);

                // return float4(IN.normal, 1);
                // return float4(abs(IN.normal), 1);
                // return float4(roundDecimal(IN.normal, 1), 1);

                // return float4(IN.uv, 0, 1);
                // return float4(IN.uv.xxx, 1);
                // return float4(roundDecimal(IN.uv.xxx, 1), 1);
                // return float4(roundDecimal(IN.uv, 1), 0, 1);

                // float4 col = texCol.r > .75 ? float4(1, 0, 0, 1) : texCol.r > .5 ? float4(0, 0, 1, 1) : texCol.r > .25 ? float4(0, 1, 0, 1) : float4(0, 1, 1, 1);
                // return IN.uv.x < .25 ? texCol.r : texCol.r * col;

                // return IN.uv.y;
                float3 colorA = float3(1, 0, 0);
                float3 colorB = float3(0, 0, 1);
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
