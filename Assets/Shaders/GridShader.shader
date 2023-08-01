Shader "Lukasz/GridShader"
{
    // @todo transparent bg color // or semi-transparent - user can define bg color
    // @todo semi-transparent line color
    // @todo rows and columns - don't blend colors at intersections

    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _LinesCount ("Lines Count", Float) = 5
        _LineSize ("Line Size", Float) = .05
        _BackgroundColor ("Background Color", Color) = (0,0,0,0)
        _LineColor ("Line Color", Color) = (0,0,0,.2)
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

        Pass
        {
            Cull Back // Back, Front, Off
            ZWrite Off
            ZTest LEqual // LEqual (default), Always, GEqual // GEqual - for a character beyond and obstacle
            // Blend One One // additive
            // Blend DstColor Zero // multiplicative
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Assets/ShaderExtensions.cginc"

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

            v2f vert (appdata v)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(v.vertex);
                // OUT.normal = v.normal;
                OUT.normal = UnityObjectToWorldNormal(v.normal);
                OUT.uv = v.uv;
                OUT.localPosition = v.vertex;
                OUT.color = float4(v.vertex.xyz, 1);
                return OUT;
            }

            sampler2D _MainTex;
            float _LinesCount;
            float _LineSize;
            float4 _BackgroundColor;
            float4 _LineColor;

            float sawtooth(float t)
            {
                return t - floor(t + 1/2);
            }

            float trianglex(float t)
            {
                return abs(sawtooth(t));
            }

            float GridLine(float pos, float n, float offset, float lineSize)
            {
                float x = roundPrec(pos, 1 / n);

                return abs(x - pos) <= lineSize / 2;
                // return pos % (1 / n) <= lineSize;

                // float val = cos((pos + offset) * TAU * n) * .5 + .5;
                // // float val = trianglex((pos + offset) * n);
                // val = (val % TAU) - lineSize * .5 < .00001;
                // // val = floor(roundPrec(val, lineSize * 2));
                // // val = val <= lineSize * 2;

                // return val;
            }

            fixed4 frag (v2f IN) : SV_Target
            {
                // float bgColorVal = 0;
                // float4 bgColor = float4(bgColorVal, bgColorVal, bgColorVal, 1);
                // float lineColorVal = .5;
                // float4 lineColor = float4(lineColorVal, lineColorVal, lineColorVal, 1);

                // float n = 10;
                // float lineSize = 1.0 / 64;
                // float offset = 0; // PI / 2;
                // float tX = gridColor(IN.uv.y, n, lineSize, offset);
                // float tY = gridColor(IN.uv.x, n, lineSize, offset);

                // float4 outColorX = 1 - tX;
                // float4 outColorY = 1 - tY;
                // float4 outColor = lerp(bgColor, lineColor, max(outColorX, outColorY) > .99);

                // fixed4 texCol = tex2D(_MainTex, IN.uv);

                // return outColor;
                // // return outColor * float4(1, 0,0,1);
                // // return outColor * float4(1, 0,0,1) * texCol;
                // return cos((IN.uv.x) * TAU * 5) * .5 + .5;
                // return trianglex((IN.uv.x) * 5);

                float offset = 0; // .25 to center
                float lineSize = _LineSize / _LinesCount;
                // float lineSize = 1.0 / 100;

                float pattern_y = GridLine(IN.uv.x, _LinesCount, offset, lineSize);
                float pattern_x = GridLine(IN.uv.y, _LinesCount, offset, lineSize);
                float pattern = pattern_y == 1 || pattern_x == 1;

                return pattern == 0 ? _BackgroundColor : _LineColor;
                // return pattern + lineColor * (pattern == 0);
                // return max(pattern_y, pattern_x) * float4(1, 0, 0, 0);
            }
            ENDCG
        }
    }
}
