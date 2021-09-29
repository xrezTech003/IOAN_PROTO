Shader "Unlit/TextPageBoxShader"
{
	Properties
	{
		_MainTex("_MainTex", 2D) = "white" {}
		_Color("_Color", Color) = (1, 1, 1, 1)
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"Rendertype" = "Transparent"
		}

		Blend SrcAlpha OneMinusSrcAlpha
		Zwrite Off

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			sampler2D _MainTex;

			float _Alpha;
			float4 _Color;

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

			v2f vert(input v)
			{
				v2f  result;

				result.pos = UnityObjectToClipPos(v.vertex);
				result.uv = v.texcoord.xy;
				result.color = v.color;

				return result;
			}

			float4 frag(v2f i) : COLOR
			{
				return _Color;
			}

			ENDCG
		}
	}
}