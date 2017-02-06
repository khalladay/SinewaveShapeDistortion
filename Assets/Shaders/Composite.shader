Shader "Unlit/Composite"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	_DistortionRT("Dist Color Buffer", 2D) = "white"{}
	_DistortionMask("Dist mask", 2D) = "black"{}
	}
	SubShader
	{
		Tags{ "RenderType" = "Opaque" }

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			sampler2D _DistortionRT;
			sampler2D _CameraDepthTexture;

			float _DistortionOffset;
			float _DistortionAmount;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.uv,_MainTex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 screen = tex2D(_MainTex, float2(i.uv.x, i.uv.y));
	
				float2 distortUVs = i.uv;

			#if defined(UNITY_UV_STARTS_AT_TOP) && !defined(SHADER_API_MOBILE)
				distortUVs.y = 1.0 - distortUVs.y;
			#endif
				float4 distort = tex2D(_DistortionRT, fixed2(distortUVs.x + sin((distortUVs.y + _DistortionOffset) * 100)*_DistortionAmount, distortUVs.y));
				float d = tex2D(_CameraDepthTexture, distortUVs).r;

			#if UNITY_REVERSED_Z
				return lerp(screen, distort, distort.a > d);
			#else
				return lerp(screen, distort, distort.a < d);
			#endif
			}
			
			ENDCG
		}
	}
}
