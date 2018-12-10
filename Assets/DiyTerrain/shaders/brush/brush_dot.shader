﻿Shader "Unlit/brush_dot"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_height("height",float)=64.0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "Queue"="Transparent" }
		LOD 100

		Pass
		{
			Blend One One
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _height;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			float4 frag (v2f i) : SV_Target
			{
				// 高度 -128~128
				float range=128.0;
				float strength = _height/range; // 0~1

				// 高度正規化後會落在-1~1間，但最後只能存0~1，所以要x0.5
				strength=strength*0.5; 

				float2 center=(0.5,0.5);
				float2 v =i.uv-center;
				float r=length(v); // => \/ 0.5~0~0.5

				// smooth step
				r=-r+1.0; // reverse =>  /\ 0.5~1~0.5
				r=strength*smoothstep(0.5,1.0,r);

				// 球
				// r= 2.0*r;
				// r = clamp(r,0.0,1.0);
				// r=strength*cos(asin(r));

				return float4(r,r,r,1.0);
			}
			ENDCG
		}
	}
}
