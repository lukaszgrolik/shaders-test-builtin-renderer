Shader "Lukasz/WindTilt"
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
            "RenderType"="Opaque"
            "PreviewType"="Plane"
            // "CanUseSpriteAtlas"="True"
        }

        Cull Back
        ZWrite Off
        ZTest LEqual
        Blend One One

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

            float _Enabled;
            sampler2D _MainTex;
            float4 _BaseColor;

            static const float PI = 3.14159265359f;
            static const float TAU = 2 * PI;

            v2f vert (appdata v)
            {
                v2f OUT;

                float speed = 1;
                float distance = .5;

                v.vertex.x += (v.vertex.y > 0) * distance * (cos(_Time.y * TAU * speed) * .5 + .5);

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
                // return 0;
                return _BaseColor;
            }

            ENDCG
        }
    }
}
