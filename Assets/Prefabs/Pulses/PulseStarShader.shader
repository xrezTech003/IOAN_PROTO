// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Example/PulseStarShader"
{
	Properties
	{
		_MainTex("Color (RGB) Alpha (A)", 2D) = "white" {}
		_Color("Color", Color) = (1, 1, 1, 1)
		_MaxHeight("Max Height", float) = 0.0
	}

		SubShader
		{
			Tags
			{
				"Queue" = "Transparent"
				"RenderType" = "Transparent"
				"IgnoreProjector" = "True"
			}
			LOD 100

			ZWrite On
			Blend SrcAlpha OneMinusSrcAlpha

			Pass
			{
				CGPROGRAM
				#pragma vertex vert  
				#pragma fragment frag Lambert Alpha
				#include "UnityCG.cginc"

				fixed4 _Color;
				float _MaxHeight;

				struct v2f
				{
					float4 col : COLOR;
					float4 pos : SV_POSITION;
					float3 nrm : NORMAL;
					float4 texcoord : TEXCOORD0;
				};

				UNITY_INSTANCING_BUFFER_START(Props)
				UNITY_INSTANCING_BUFFER_END(Props)

				//Vertex Function / Pass data to fragment shader
				v2f vert(appdata_full v)
				{
					v2f o;

					o.col = _Color;
					o.pos = UnityObjectToClipPos(v.vertex);
					o.nrm = v.normal;
					o.texcoord = v.vertex;

					return o;
				}

				//Fragment Function
				float4 frag(v2f i) : COLOR
				{
					//Diminish brightness as the star travels away from the spawn point
					float4 col = i.col;
					float4 pos = mul(unity_ObjectToWorld, i.texcoord);

					if (pos.y > 0.0 && pos.y <= _MaxHeight) col.a *= lerp(0.1, 1.0, pos.y / _MaxHeight);
					else if (pos.y <= 0 && pos.y >= -_MaxHeight) col.a *= lerp(0.1, 1.0, -pos.y / _MaxHeight);

					return col;
				}

				ENDCG
			}
		}
}
