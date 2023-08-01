// Upgrade NOTE: replaced '_Projector' with 'unity_Projector'
// Upgrade NOTE: replaced '_ProjectorClip' with 'unity_ProjectorClip'

Shader "Lukasz/ProjectorCircle" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
		_ShadowTex ("Cookie", 2D) = "" {}
		_FalloffTex ("FallOff", 2D) = "" {}
	}

	Subshader {
		Tags {
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
		Pass {
			ZWrite Off
			ColorMask RGB
			// Blend DstColor One
            Blend SrcAlpha OneMinusSrcAlpha
			Offset -1, -1

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// #pragma multi_compile_fog
			#include "UnityCG.cginc"
            #include "Assets/ShaderExtensions.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                // float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

			struct v2f {
				float4 uvShadow : TEXCOORD0;
				float4 uvFalloff : TEXCOORD1;
                float2 uv : TEXCOORD2;
				// UNITY_FOG_COORDS(2)
				float4 pos : SV_POSITION;
                float3 globalPos : TEXCOORD3;
			};

			float4x4 unity_Projector;
			float4x4 unity_ProjectorClip;

			// v2f vert (float4 vertex : POSITION)
			v2f vert (appdata v)
			{
				v2f o;
                o.uv = v.uv;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uvShadow = mul (unity_Projector, v.vertex);
				o.uvFalloff = mul (unity_ProjectorClip, v.vertex);
                o.globalPos = mul(unity_ObjectToWorld, v.vertex);
				// UNITY_TRANSFER_FOG(o, o.pos);
				return o;
			}

			fixed4 _Color;
			sampler2D _ShadowTex;
			sampler2D _FalloffTex;

			fixed4 frag (v2f i) : SV_Target
			{
                // float3 cursorPos = float3(28, 0, 12);
                // float3 cursorPos = UNITY_PROJ_COORD(i.uvShadow);
				// return i.uvShadow.x;

                float2 uvCenter = i.uvShadow.xy * 2 - 1;
                float uvRadDist = length(uvCenter);
                // float4 mask_uv = 1 - saturate(uvRadDist);
				// return mask_uv;


                // float distFromCursor = distance(i.uvShadow.xy, i.globalPos.xz);
                float distMin = 1;
                float distMax = 1;
                float mask_distFromCursorOutward = 1 - saturate(inversedLerp(distMin, distMax, uvRadDist));
				float dist2Min = .8;
				float dist2Max = .8;
                float mask_distFromCursorInward = 1 - saturate(inversedLerp(dist2Min, dist2Max, uvRadDist));
				float mask_distFromCursor = mask_distFromCursorOutward && !mask_distFromCursorInward;

                return mask_distFromCursor * _Color;

                // bgColor, lineColor

				fixed4 texS = tex2Dproj (_ShadowTex, UNITY_PROJ_COORD(i.uvShadow));
				texS.rgb *= _Color.rgb;
				texS.a = 1.0-texS.a;

				fixed4 texF = tex2Dproj (_FalloffTex, UNITY_PROJ_COORD(i.uvFalloff));

				fixed4 res = texS * texF.a;

				// UNITY_APPLY_FOG_COLOR(i.fogCoord, res, fixed4(0,0,0,0));

                // res *= mask_uv;
                res *= mask_distFromCursor;

				return res;
			}
			ENDCG
		}
	}
}
