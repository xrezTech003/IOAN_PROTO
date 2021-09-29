﻿Shader "Unlit/ClothPosShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
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
                float3 pos : NORMAL;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
                
				v2f o;

                o.pos = v.vertex.xyz;

				//o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv = v.uv;

                o.vertex = float4(-v.uv.x * 2.0 + 1.0,v.uv.y * 2.0 - 1.0, 0, 1);
                
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
                fixed4 col = float4( i.pos, 1);
                return col;
			}
			ENDCG
		}
	}
}
