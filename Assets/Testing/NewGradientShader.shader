// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/NewGradientShader" 
{
	Properties
	{
		_MainTex("Color (RGB) Alpha (A)", 2D) = "white" {}
		_ColorA("Color A", Color) = (1, 1, 1, 1)
		_ColorB("Color B", Color) = (0, 0, 0, 0)
		_ColorE("Color Emission", Color) = (0, 0, 0, 0)
		_Middle("Middle", Range(0.001, 0.999)) = 1
		_MiddleStep("Middle Step", Range(0.001, 0.499)) = 0.5
		_EmissionStep("Emission Step", Range(0.001, 0.499)) = 0.5
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

			fixed4 _ColorA;
			fixed4 _ColorB;
			fixed4 _ColorE;
			float _Middle;
			float _MiddleStep;
			float _EmissionStep;

			struct v2f
			{
				float4 pos : SV_POSITION;
				float4 texcoord : TEXCOORD0;
			};

			v2f vert(appdata_full v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.texcoord = v.texcoord;
				return o;
			}

			fixed4 frag(v2f i) : COLOR
			{
				fixed4 c;

				float a = _Middle - _MiddleStep;
				float b = _Middle + _MiddleStep;

				if (i.texcoord.x < a) c = _ColorA;
				else if (i.texcoord.x <= b) c = lerp(_ColorA, _ColorB, (i.texcoord.x - a) / (b - a));
				else c = _ColorB;

				float m = _EmissionStep;
				float n = 1.0 - _EmissionStep;

				if (i.texcoord.y <= m) c = lerp(_ColorE, c, i.texcoord.y / m);
				else if (i.texcoord.y >= n) c = lerp(c, _ColorE, (i.texcoord.y - n) / (1 - n));

				return c;
			}
			ENDCG
		}
	}
}