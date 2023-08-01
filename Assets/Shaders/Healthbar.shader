Shader "Lukasz/Healthbar"
{
    Properties
    {
        _Progress ("Progress", Range(0, 1)) = .5
        _BorderSize ("Border Size", Range(0, 1)) = .2
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            // "IgnoreProjector"="True"
            // "RenderType"="Opaque"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            // "CanUseSpriteAtlas"="True"
        }

        Cull Off
        ZWrite Off
        // ZTest Always

        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Assets/ShaderExtensions.cginc"

            float _Progress;
            float _BorderSize;
            sampler2D _MainTex;

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

            fixed4 frag (v2f IN) : SV_Target
            {
                // return float4(0, 0, 0, 1 - IN.uv.x);
                // return IN.uv.y;

                // float4 texCol = tex2D(_MainTex, IN.uv);
                // if (texCol.a != 0)
                // {
                //     if (texCol.r < .15) return texCol.a * COLOR_RED;
                //     if (texCol.r < .3) return texCol.a * COLOR_GREEN;
                //     if (texCol.r < .6) return texCol.a * COLOR_BLUE;
                //     return texCol.a * COLOR_YELLOW;
                // }

                // float4 colorA = float4(1, 0, 0, 1);
                // float4 colorB = float4(0, 0, 1, 1);
                // float start = .4;
                // float end = .6;
                // // float t = inversedLerp(start, end, IN.uv.y);
                // float t = saturate(inversedLerp(start, end, IN.uv.y));
                // // t = frac(t);
                // // return t;
                // float3 outColor = lerp(colorA, colorB, t);

                // float borderSize = .05;
                // float borderMask = (IN.uv.x < borderSize || IN.uv.x > (1 - borderSize)) || (IN.uv.y < borderSize || IN.uv.y > (1 - borderSize));

                // if (borderMask == 1)
                // {
                //     return COLOR_YELLOW;
                // }

                float2 coords = IN.uv;
                coords.x *= 5;

                float2 pointOnLine = float2(clamp(coords.x, .5, 4.5), .5);
                float sdf = distance(coords, pointOnLine) * 2 - 1;

                clip(-sdf);

                float borderSdf = sdf + _BorderSize;
                float pd = fwidth(borderSdf); // screen space partial derivative
                // float borderMask = 1 - step(0, borderSdf);
                float borderMask = 1 - saturate(borderSdf / pd);

                // if (borderMask == 1)
                // {
                //     return COLOR_BLACK;
                // }

                // return float4(borderMask.xxx, 1);

                // return float4(sdf.xxx, 1);

                // float4 hbColor = lerp(COLOR_RED, COLOR_GREEN, roundPrec(saturate(IN.uv.x - .25 / 2), .25));
                float hbMask = IN.uv.x <= _Progress;
                // clip(hbMask - 0.5);

                float progressRounding = 1; // .25
                float hbColorT = progressRounding == 1 ? _Progress : roundPrec(saturate(_Progress - progressRounding / 2), progressRounding);
                float progressPadding = .1;
                hbColorT = progressPadding == 0 ? hbColorT : saturate(inversedLerp(progressPadding, 1 - progressPadding, hbColorT));

                // float4 hbColor = lerp(COLOR_RED, COLOR_GREEN, hbColorT) * tex2D(_MainTex, IN.uv);
                float4 hbColor = lerp(COLOR_RED, COLOR_GREEN, hbColorT);
                return lerp(COLOR_BLACK, hbColor, hbMask * borderMask);
            }
            ENDCG
        }
    }
}
