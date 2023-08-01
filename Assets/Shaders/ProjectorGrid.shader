// Upgrade NOTE: replaced '_Projector' with 'unity_Projector'
// Upgrade NOTE: replaced '_ProjectorClip' with 'unity_ProjectorClip'

Shader "Lukasz/ProjectorGrid" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
		_ShadowTex ("Cookie", 2D) = "" {}
		_FalloffTex ("FallOff", 2D) = "" {}
		_Radius ("Radius", Float) = 10
		_BorderBlendSize ("BorderBlendSize", Float) = 5
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

            float gridLine(float pos, float n, float offset, float lineSize)
            {
                float x = roundPrec(pos, 1 / n);

                return abs(x - pos) <= lineSize / 2;
            }

            float grid(float2 pos, float _LinesCount, float _LineSize)
            {
                float offset = 0; // .25 to center
                float lineSize = _LineSize / _LinesCount;
                // float lineSize = 1.0 / 100;

                float pattern_y = gridLine(pos.x, _LinesCount, offset, lineSize);
                float pattern_x = gridLine(pos.y, _LinesCount, offset, lineSize);
                float pattern = pattern_y == 1 || pattern_x == 1;

                // return pattern == 0 ? _BackgroundColor : _LineColor;
                return pattern;
            }

			fixed4 _Color;
			sampler2D _ShadowTex;
			sampler2D _FalloffTex;
			float _Radius;
			float _BorderBlendSize;

			fixed4 frag (v2f i) : SV_Target
			{

                // return float4(i.globalPos.xyz, 1);
                // return i.uv.y;
                // float2 uvCenter = i.uv * 2 - 1;
                float2 uvCenter = i.uvShadow * 2 - 1;
                float uvRadDist = length(uvCenter);
                float4 mask_uv = 1 - saturate(uvRadDist);
                // float4 mask_uv = lerp(colorA, colorB, uvRadDist);
                // return 0;
                // return mask_uv;

                // float3 cursorPos = float3(28, 0, 12);
                // float3 cursorPos = _Position;
                float3 cursorPos = float3(uvCenter.x, 0, uvCenter.y);

				// convert to projector UV?
                float distFromCursor = distance(cursorPos.xz, i.globalPos.xz);
                float distMin = _Radius - _BorderBlendSize;
                float distMax = _Radius;
                // float mask_distFromCenter = 1 - saturate(inversedLerp(distMin, distMax, distFromCursor));
                float mask_distFromCenter = 1 - saturate(inversedLerp(distMin, distMax, uvRadDist));
                // return mask_distFromCenter;

                // float mask_distFromCenterOutward = 1 - saturate(inversedLerp(1, 1, uvRadDist));
				// return mask_distFromCenterOutward;

                // bgColor, lineColor
                float gridMask = grid(i.globalPos.xz, 1, .025);
                return mask_distFromCenter * gridMask * _Color;

				fixed4 texS = tex2Dproj (_ShadowTex, UNITY_PROJ_COORD(i.uvShadow));
				texS.rgb *= _Color.rgb;
				texS.a = 1.0-texS.a;

				fixed4 texF = tex2Dproj (_FalloffTex, UNITY_PROJ_COORD(i.uvFalloff));

				fixed4 res = texS * texF.a;

				// UNITY_APPLY_FOG_COLOR(i.fogCoord, res, fixed4(0,0,0,0));

                // res *= mask_uv;
                res *= mask_distFromCenter;

				return res;
			}
			ENDCG
		}
	}
}
