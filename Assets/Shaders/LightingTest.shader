Shader "Lukasz/LightingTest"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Gloss ("Gloss", Range(0, 1)) = 1
        _GlossRounding ("Gloss Rounding", Range(0, 1)) = 1
    }
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            // "IgnoreProjector"="True"
            "RenderType"="Opaque"
            // "PreviewType"="Plane"
            // "CanUseSpriteAtlas"="True"
        }

        Cull Off
        // ZWrite Off
        // ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            // #include "AutoLighting.cginc"
            #include "UnityLightingCommon.cginc"
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
                float3 localPos : TEXCOORD2;
                float3 globalPos : TEXCOORD3;
                float3 normal2 : TEXCOORD4;
            };

            v2f vert (appdata v)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(v.vertex);
                OUT.normal = UnityObjectToWorldNormal(v.normal);
                OUT.normal2 = (v.normal);
                OUT.uv = v.uv;
                OUT.localPos = v.vertex;
                OUT.globalPos = mul(unity_ObjectToWorld, v.vertex);
                return OUT;
            }

            float4 _Color;
            float _Gloss;
            float _GlossRounding;

            fixed4 frag (v2f IN) : SV_Target
            {
                // diffuse lighting

                float3 N = normalize(IN.normal); // needs to be normalized to remove glitch in specular lighting
                float3 L = _WorldSpaceLightPos0.xyz;
                float3 lambert = saturate(dot(N, L));
                float3 diffuseLight = lambert * _LightColor0.rgb;
                // float _DiffuseRounding = .25;
                // if (_DiffuseRounding < 1) diffuseLight = round(diffuseLight / _DiffuseRounding) * _DiffuseRounding;

                // return float4(diffuseLight, 1);

                // specular lighting (phong)

                // float3 V = normalize(_WorldSpaceCameraPos - IN.globalPos);
                // float3 R = reflect(-L, N);
                // float3 specularLight = saturate(dot(V, R));
                // specularLight = pow(specularLight, _Gloss); // gloss - specular component
                // if (_GlossRounding < 1) specularLight = round(specularLight / _GlossRounding) * _GlossRounding;

                // return float4(specularLight, 1);

                // specular lighting (blinn-phong)

                float3 V = normalize(_WorldSpaceCameraPos - IN.globalPos);
                // float3 R = reflect(-L, N);
                float3 H = normalize(L + V);
                float3 specularLight = saturate(dot(H, N)) * (lambert > 0);
                float specularExponent = exp2(_Gloss * 11) + 2;
                specularLight = pow(specularLight, specularExponent) * _Gloss; // gloss - specular component
                if (_GlossRounding < 1) specularLight = round(specularLight / _GlossRounding) * _GlossRounding;
                specularLight *= _LightColor0.rgb;

                // return float4(specularLight, 1);

                // composite

                float fresnelSizeEnabled = 0;
                float fresnelSize = .1;
                float fresnel = fresnelSizeEnabled ? 1 - (dot(V, N) > fresnelSize) : 1 - dot(V, N)  ;

                // return fresnel;

                return float4(_Color * diffuseLight + specularLight + fresnel * COLOR_RED, 1);

                // return float4(diffuseLight, 1) * abs(float4(IN.normal, 1));
                // return float4(diffuseLight, 1) * abs(float4(IN.uv, 1, 1));
                // return float4(_WorldSpaceLightPos0.xyz, 1);
                // return float4(IN.normal, 1);

                // fixed4 texCol = tex2D(_MainTex, IN.uv);


                // // return IN.uv.y;
                // float3 colorA = float3(1, 0, 0);
                // float3 colorB = float3(0, 0, 1);
                // float start = .4;
                // float end = .6;
                // // float t = inversedLerp(start, end, IN.uv.y);
                // float t = saturate(inversedLerp(start, end, IN.uv.y));
                // // t = frac(t);
                // // return t;
                // float3 outColor = lerp(colorA, colorB, t);

                // return texCol * float4(outColor, 1);
            }
            ENDCG
        }
    }
}
