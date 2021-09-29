Shader "Unlit/AlphaSpriteShader"
{
	Properties
	{
		_MainTex("_MainTex", 2D) = "white" {}
		Alpha("Alpha", Range(0.0, 1.0)) = 1.0
	}

		SubShader
		{
			Pass
			{
				Name "ColorizeSubshader"

				Tags
				{
					"Queue" = "Transparent"
					"Rendertpe" = "Transparent"
				}

				Blend SrcAlpha OneMinusSrcAlpha
				//ZWrite Off
				CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest 
				#include "UnityCG.cginc"

				sampler2D _MainTex;

				float Alpha;

				struct input
				{
					float4 vertex   : POSITION;
					float4 texcoord : TEXCOORD0;
					float4 color    : COLOR;
				};

				struct v2f
				{
					float4  pos : SV_POSITION;
					float2  uv : TEXCOORD0;
					float4  color : COLOR;
				};

				//Vertex Function / Pass data to fragment shader
				v2f vert(input v)
				{
					v2f  result;

					result.pos = UnityObjectToClipPos(v.vertex);
					result.uv = v.texcoord.xy;
					result.color = v.color;

					return result;
				}

				//Fragment Function / Color text white and clear out alpha channel
				float4 frag(v2f i) : COLOR
				{
					float4 texcolor = tex2D(_MainTex, i.uv);
					texcolor.rgba = tex2D(_MainTex, i.uv);
					texcolor.a *= Alpha;

					return texcolor;
				}

				ENDCG
			}
		}
}