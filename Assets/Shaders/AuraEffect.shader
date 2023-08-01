Shader "Lukasz/AuraEffect"
{
    // @todo transparent bg color // or semi-transparent - user can define bg color
    // @todo semi-transparent line color
    // @todo rows and columns - don't blend colors at intersections

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
            "Queue" = "Transparent"
            // "IgnoreProjector"="True"
            "RenderType"="Transparent"
            // "RenderType"="Transparent"
            "PreviewType"="Plane"
            // "CanUseSpriteAtlas"="True"
        }

        Pass
        {
            Cull Off // Back, Front, Off
            ZWrite Off
            ZTest LEqual // LEqual (default), Always, GEqual // GEqual - for a character beyond and obstacle
            Blend One One

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Assets/ShaderExtensions.cginc"

            // #define PI 3.14159265359

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
            float4 _BaseColor;
            float _Red;

            fixed4 frag (v2f IN) : SV_Target
            {
                // float4 texCol = tex2D(_MainTex, IN.uv);
                float noiseVal = tex2D(_MainTex, IN.uv);

                float offset = cos(IN.uv.x * TAU * 10) * .0125;

                float time = (_Time.y + cos(_Time.y * TAU * .1) * .5 + .5) * .2;
                float pattern = cos((IN.uv.y + offset - time) * TAU * 5) * .5 + .5;
                pattern *= 1 - IN.uv.y;

                float topBottomRemover = abs(IN.normal.y) < .999;
                pattern *= topBottomRemover;

                float4 colorTop = float4(1, 0, 0, 1);
                float colorTime = cos(_Time.y * TAU * .1) * .5 + .5;
                float4 colorBottom = float4(1, 1, colorTime, 1);
                float4 color = lerp(colorBottom, colorTop, IN.uv.y);

                float4 outColor = color * pattern;

                return outColor;
            }

            ENDCG
        }
    }
}
