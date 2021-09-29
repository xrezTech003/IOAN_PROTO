Shader "Unlit/PulseSpriteShader"
{
	Properties
	{
		_Color("Tint", Color) = (0, 0, 0, 1)
		_MainTex("Texture", 2D) = "white" {}
		_MaxHeight("Max Height", float) = 0.0
	}

	SubShader
	{
		Tags
		{
			"RenderType" = "Transparent"
			"Queue" = "Transparent"
		}

		Blend SrcAlpha OneMinusSrcAlpha

		ZWrite off
		Cull off

		Pass
		{
			CGPROGRAM

			#include "UnityCG.cginc"

			#pragma vertex vert
			#pragma fragment frag

			sampler2D _MainTex;
			float4 _MainTex_ST;

			fixed4 _Color;
			float _MaxHeight;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				fixed4 color : COLOR;
			};

			struct v2f
			{
				float4 position : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 texcoord : TEXCOORD1;
				fixed4 color : COLOR;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.position = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = v.color;
				o.texcoord = v.vertex;
				return o;
			}

			fixed4 frag(v2f i) : SV_TARGET
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				col *= _Color;
				col *= i.color;

				float4 pos = mul(unity_ObjectToWorld, i.texcoord);

				if (pos.y > 0.0 && pos.y <= _MaxHeight) col.a *= lerp(0.1, 1.0, pos.y / _MaxHeight);
				else if (pos.y <= 0 && pos.y >= -_MaxHeight) col.a *= lerp(0.1, 1.0, -pos.y / _MaxHeight);

				return col;
			}

			ENDCG
		}
	}
}